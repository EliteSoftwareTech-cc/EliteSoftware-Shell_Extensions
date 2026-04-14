$ErrorActionPreference = "Stop"

$slnDir = "Z:\WIM_AND_INSTALLER_MANAGER"
$backupDir = Join-Path $slnDir "Backups"
$changelogPath = Join-Path $slnDir "CHANGELOG.md"
$completeBuildDir = Join-Path $slnDir "COMPLETE_BUILD"

if (!(Test-Path $backupDir)) { New-Item -ItemType Directory -Path $backupDir | Out-Null }
if (!(Test-Path $completeBuildDir)) { New-Item -ItemType Directory -Path $completeBuildDir | Out-Null }
if (!(Test-Path $changelogPath)) { Set-Content -Path $changelogPath -Value "# Changelog`n" }

Write-Host "Building the WIM_MERGE_SOLUTION solution..." -ForegroundColor Cyan
Push-Location $slnDir
dotnet build WIM_MERGE_SOLUTION.slnx -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting backup."
}
Pop-Location

Write-Host "Copying files to COMPLETE_BUILD directory..." -ForegroundColor Cyan
Remove-Item -Path "$completeBuildDir\*" -Recurse -Force -ErrorAction SilentlyContinue

Copy-Item "$slnDir\WIM_MERGE_APP\bin\Release\net48\*" -Destination $completeBuildDir -Recurse -Force
Copy-Item "$slnDir\WIM_MERGE_ENGINE\bin\Release\net48\*" -Destination $completeBuildDir -Recurse -Force -ErrorAction SilentlyContinue

$exePath = Join-Path $completeBuildDir "WIM_MERGE_APP.exe"
$lnkPath = Join-Path $slnDir "WIM_MERGE_APP_LATEST.lnk"
if (Test-Path $lnkPath) { Remove-Item $lnkPath -Force }
$symlinkPath = Join-Path $slnDir "WIM_MERGE_APP_LATEST.exe"
if (Test-Path $symlinkPath) { Remove-Item $symlinkPath -Force }

if (Test-Path $exePath) {
    Write-Host "Creating shortcut to latest executable at $lnkPath"
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($lnkPath)
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = $completeBuildDir
    $Shortcut.Save()
}

$versionInput = "1.0.0"
if ($args.Count -gt 0) { $versionInput = $args[0] }
$changes = "Automated backup build"
if ($args.Count -gt 1) { $changes = $args[1] }

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupName = "WIM_MERGE_SOLUTION_v${versionInput}_${timestamp}.cab"
$backupPath = Join-Path $backupDir $backupName

Write-Host "Creating entire backup archive as a CAB at $backupPath ..." -ForegroundColor Cyan
$ddfPath = Join-Path $backupDir "backup.ddf"
# Build DDF
$lines = @()
$lines += ".OPTION EXPLICIT"
$lines += ".Set CabinetNameTemplate=$backupName"
$lines += ".Set DiskDirectoryTemplate=$backupDir"
$lines += ".Set MaxDiskSize=CDROM"
$lines += ".Set Cabinet=on"
$lines += ".Set Compress=on"

# Include all files recursively from slnDir, but exclude Backups, obj, bin, and makecab artifacts
$excludePatterns = @("\\Backups\\", "\\obj\\", "\\bin\\", "setup\.inf$", "setup\.rpt$", "backup\.ddf$")
$files = Get-ChildItem -Path $slnDir -Recurse -File | Where-Object { 
    $fullName = $_.FullName
    $match = $false
    foreach ($p in $excludePatterns) { if ($fullName -match $p) { $match = $true; break } }
    !$match
}

foreach ($file in $files) {
    $relPath = $file.FullName.Substring($slnDir.Length + 1)
    $lines += ('"{0}" "{1}"' -f $file.FullName, $relPath)
}

Set-Content -Path $ddfPath -Value ($lines -join "`n")

# Run makecab from within the backup dir to keep root clean
Push-Location $backupDir
$makecabOut = makecab.exe /f "backup.ddf" 2>&1
Pop-Location

if ($LASTEXITCODE -ne 0) {
    Write-Warning "makecab failed: $makecabOut"
}

# Cleanup makecab temporary files in both locations to be safe
Remove-Item (Join-Path $backupDir "backup.ddf") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $backupDir "setup.inf") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $backupDir "setup.rpt") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $slnDir "setup.inf") -ErrorAction SilentlyContinue
Remove-Item (Join-Path $slnDir "setup.rpt") -ErrorAction SilentlyContinue

$logEntry = "## [v$versionInput] - $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))`n- **Changes**: $changes`n- **Backup**: $backupName`n"
Add-Content -Path $changelogPath -Value $logEntry

Write-Host "Backup and logging complete. v$versionInput saved successfully!" -ForegroundColor Green

Write-Host "`nPress any key to continue..." -ForegroundColor Yellow
$null = [System.Console]::ReadKey($true)
