$ErrorActionPreference = "Stop"

$slnDir = "Z:\MetaData-ShellExtension"
$backupDir = Join-Path $slnDir "Backups"
$changelogPath = Join-Path $slnDir "CHANGELOG.md"
$completeBuildDir = Join-Path $slnDir "COMPLETE_BUILD"
$cleanupScript = Join-Path $slnDir "TEST_ENVIRONMENT\System_Cleanup.ps1"

if (!(Test-Path $backupDir)) { New-Item -ItemType Directory -Path $backupDir | Out-Null }
if (!(Test-Path $completeBuildDir)) { New-Item -ItemType Directory -Path $completeBuildDir | Out-Null }
if (!(Test-Path $changelogPath)) { Set-Content -Path $changelogPath -Value "# Changelog`n" }

# --- LOCK SHELL ---
Write-Host "Performing pre-build system cleanup and locking shell..." -ForegroundColor Yellow
if (Test-Path $cleanupScript) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $cleanupScript
}

# Ensure they STAY dead during the entire build process
Stop-Process -Name "explorer" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "win32explorer" -Force -ErrorAction SilentlyContinue

Write-Host "Building the METADATA_EDITOR_APP_SOLUTION solution (.NET 4.8)..." -ForegroundColor Cyan
Push-Location $slnDir

dotnet build METADATA_EDITOR_APP_SOLUTION.slnx -c Release
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed. Aborting backup." }

Write-Host "Building x64 Shell Extension..." -ForegroundColor Cyan
dotnet build METADATA_EDITOR_SHELL_EXTENSION\METADATA_EDITOR_SHELL_EXTENSION.csproj -c Release -r win-x64 --self-contained false
if ($LASTEXITCODE -ne 0) { Write-Error "x64 Shell Extension build failed." }

Write-Host "Building x86 Shell Extension..." -ForegroundColor Cyan
dotnet build METADATA_EDITOR_SHELL_EXTENSION\METADATA_EDITOR_SHELL_EXTENSION.csproj -c Release -r win-x86 --self-contained false
if ($LASTEXITCODE -ne 0) { Write-Error "x86 Shell Extension build failed." }

Pop-Location

# Helper function for robust copying with retries
function Copy-WithRetry {
    param($Source, $Destination)
    for ($i = 1; $i -le 5; $i++) {
        try {
            Copy-Item $Source -Destination $Destination -Recurse -Force
            return
        } catch {
            Write-Host "Copy attempt $i failed. Retrying in 2 seconds..." -ForegroundColor Gray
            Start-Sleep -Seconds 2
        }
    }
    Write-Error "Failed to copy $Source to $Destination after 5 attempts. File is likely locked."
}

Write-Host "Copying files to COMPLETE_BUILD directory..." -ForegroundColor Cyan
Remove-Item -Path "$completeBuildDir\*" -Recurse -Force -ErrorAction SilentlyContinue

# Main App
Copy-WithRetry -Source "$slnDir\METADATA_EDITOR_APP\bin\Release\net48\*" -Destination $completeBuildDir

# Arch Folders
$x64Dir = Join-Path $completeBuildDir "x64"
$x86Dir = Join-Path $completeBuildDir "x86"
if (!(Test-Path $x64Dir)) { New-Item -ItemType Directory -Path $x64Dir | Out-Null }
if (!(Test-Path $x86Dir)) { New-Item -ItemType Directory -Path $x86Dir | Out-Null }

Copy-WithRetry -Source "$slnDir\METADATA_EDITOR_SHELL_EXTENSION\bin\Release\net48\win-x64\*" -Destination $x64Dir
Copy-WithRetry -Source "$slnDir\METADATA_EDITOR_SHELL_EXTENSION\bin\Release\net48\win-x86\*" -Destination $x86Dir

$debugDir = Join-Path $completeBuildDir "DEBUG"
if (!(Test-Path $debugDir)) { New-Item -ItemType Directory -Path $debugDir | Out-Null }
Get-ChildItem -Path $completeBuildDir -Filter "*.pdb" -File -Recurse | Move-Item -Destination $debugDir -Force

# Set .json files to Hidden
Get-ChildItem -Path $completeBuildDir -Filter "*.json" -File | ForEach-Object { $_.Attributes = "Hidden" }

$exePath = Join-Path $completeBuildDir "METADATA_EDITOR_APP.exe"
$lnkPath = Join-Path $slnDir "METADATA_EDITOR_APP_LATEST.lnk"
if (Test-Path $lnkPath) { Remove-Item $lnkPath -Force }

if (Test-Path $exePath) {
    Write-Host "Creating shortcut to latest executable at $lnkPath"
    $tmpPs1 = Join-Path $slnDir "CreateShortcut.ps1"
    $ps1Cmd = @"
`$WshShell = New-Object -comObject WScript.Shell
`$Shortcut = `$WshShell.CreateShortcut(`"$lnkPath`")
`$Shortcut.TargetPath = `"$exePath`"
`$Shortcut.WorkingDirectory = `"$completeBuildDir`"
`$Shortcut.Save()
"@
    Set-Content -Path $tmpPs1 -Value $ps1Cmd
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $tmpPs1
    Remove-Item $tmpPs1 -Force
}

$versionInput = "1.4.2.0"
if ($args.Count -gt 0) { $versionInput = $args[0] }
$changes = "Implemented Always-On-Top installer and global process locking during build/install."
if ($args.Count -gt 1) { $changes = $args[1] }

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$isccPath = "C:\Users\zwhiteman\AppData\Local\Programs\Inno Setup 6\iscc.exe"
if (Test-Path $isccPath) {
    Write-Host "Compiling Inno Setup Installer..." -ForegroundColor Cyan
    $issPath = Join-Path $slnDir "MetadataEditor.iss"
    $installerOutDir = Join-Path $slnDir "COMPLETE_BUILD_INSTALLER"
    $previousInstallersDir = Join-Path $installerOutDir "PREVIOUS_INSTALLERS_VERSIONS"
    
    if (!(Test-Path $installerOutDir)) { New-Item -ItemType Directory -Path $installerOutDir | Out-Null }
    if (!(Test-Path $previousInstallersDir)) { New-Item -ItemType Directory -Path $previousInstallersDir | Out-Null }
    
    $existingInstallers = Get-ChildItem -Path $installerOutDir -Filter "*.exe" -File
    foreach ($installer in $existingInstallers) {
        Move-Item -Path $installer.FullName -Destination $previousInstallersDir -Force
    }

    & $isccPath "/DMyAppVersion=$versionInput" "/O$installerOutDir" $issPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Installer created successfully in $installerOutDir" -ForegroundColor Green
    } else {
        Write-Warning "Inno Setup compilation failed with exit code $LASTEXITCODE."
    }
} else {
    Write-Warning "Inno Setup Compiler not found at $isccPath. Skipping installer generation."
}

$backupName = "METADATA_EDITOR_APP_v${versionInput}_${timestamp}.cab"
$backupPath = Join-Path $backupDir $backupName

Write-Host "Creating entire CAB backup archive at $backupPath ..." -ForegroundColor Cyan

$ddfPath = Join-Path $slnDir "backup.ddf"
$ddfContent = @"
.OPTION EXPLICIT
.Set CabinetNameTemplate=$backupName
.Set DiskDirectory1=$backupDir
.Set MaxDiskSize=CDROM
.Set Cabinet=on
.Set Compress=on
.Set CompressionType=LZX
"@
Set-Content -Path $ddfPath -Value $ddfContent

$backupPattern = [regex]::Escape("\Backups\")
$prevInstallerPattern = [regex]::Escape("\PREVIOUS_INSTALLERS_VERSIONS\")
$filesToBackup = Get-ChildItem -Path $slnDir -Recurse -File | Where-Object { $_.FullName -notmatch $backupPattern -and $_.FullName -notmatch $prevInstallerPattern -and $_.FullName -notmatch "backup\.ddf$" -and $_.FullName -notmatch "\.zip$" }


foreach ($file in $filesToBackup) {
    $relPath = $file.FullName.Substring($slnDir.Length + 1)
    $line = "`"$($file.FullName)`" `"$relPath`""
    Add-Content -Path $ddfPath -Value $line
}

& makecab.exe /f $ddfPath | Out-Null

if (Test-Path $ddfPath) { Remove-Item $ddfPath -Force }
if (Test-Path "setup.inf") { Remove-Item "setup.inf" -Force }
if (Test-Path "setup.rpt") { Remove-Item "setup.rpt" -Force }

$logEntry = @"
## [v$versionInput] - $((Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))
- **Changes**: $changes
- **Backup**: $backupName
"@

Add-Content -Path $changelogPath -Value $logEntry

Write-Host "Backup and logging complete. v$versionInput saved successfully!" -ForegroundColor Green

# --- UNLOCK SHELL ---
Write-Host "Unlocking shell (restarting processes)..." -ForegroundColor Yellow
$win32ExplorerPath = "D:\Projects\Explorer++_FORK\Win32Explorer_1.4.X\Win32Explorer.exe"
if (Test-Path $win32ExplorerPath) {
    Write-Host "Opening project root in Win32Explorer..." -ForegroundColor Cyan
    $cmdLine = "`"$win32ExplorerPath`" `"$slnDir`""
    Invoke-WmiMethod -Class Win32_Process -Name Create -ArgumentList $cmdLine | Out-Null
}
# Standard Explorer
Start-Process "explorer.exe"
