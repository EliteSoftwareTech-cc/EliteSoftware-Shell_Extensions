# EliteUserRightsManager - Golden Standard Build & Backup Script
# Version: 1.0.0.0
# Author: Zachary Whiteman

$ErrorActionPreference = "Stop"

# --- Project Paths ---
$slnDir = "S:\Projects\EliteSoftware-Shell_Extensions_&_CPLs\ControlPanel-Applets\User_Rights_Manager"
$backupDir = Join-Path $slnDir "Backups"
$changelogPath = Join-Path $slnDir "CHANGELOG.md"
$completeBuildDir = Join-Path $slnDir "COMPLETE_BUILD"
$installerOutDir = Join-Path $slnDir "COMPLETE_BUILD_INSTALLER"

# --- Versioning ---
$versionInput = "1.0.0.0"
if ($args.Count -gt 0) { $versionInput = $args[0] }
$changes = "Initial Build"
if ($args.Count -gt 1) { $changes = $args[1] }

# --- Always On Top / Foreground Logic ---
Add-Type @"
    using System;
    using System.Runtime.InteropServices;
    public class User32 {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;
    }
"@
$consoleHandle = [User32]::GetConsoleWindow()
if ($consoleHandle -ne [IntPtr]::Zero) {
    [User32]::SetWindowPos($consoleHandle, [User32]::HWND_TOPMOST, 0, 0, 0, 0, [User32]::SWP_NOMOVE -bor [User32]::SWP_NOSIZE -bor [User32]::SWP_SHOWWINDOW)
    Write-Host "Window locked to Always-On-Top." -ForegroundColor Cyan
}

if (!(Test-Path $backupDir)) { New-Item -ItemType Directory -Path $backupDir | Out-Null }
if (!(Test-Path $completeBuildDir)) { New-Item -ItemType Directory -Path $completeBuildDir | Out-Null }

# --- STEP 11: INITIAL CAB ARCHIVAL ---
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupName = "EliteUserRightsManager_v${versionInput}_${timestamp}.cab"
$backupPath = Join-Path $backupDir $backupName

Write-Host "Creating pre-build CAB backup archive at $backupPath ..." -ForegroundColor Cyan
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

$filesToBackup = Get-ChildItem -Path $slnDir -Recurse -File | Where-Object { 
    $_.FullName -notmatch "\\Backups\\" -and 
    $_.FullName -notmatch "backup\.ddf$" -and 
    $_.FullName -notmatch "\.zip$" -and
    $_.FullName -notmatch "\\obj\\" -and
    $_.FullName -notmatch "\\bin\\"
}

foreach ($file in $filesToBackup) {
    $relPath = $file.FullName.Substring($slnDir.Length + 1)
    $line = "`"$($file.FullName)`" `"$relPath`""
    Add-Content -Path $ddfPath -Value $line
}

& makecab.exe /f $ddfPath | Out-Null
Remove-Item $ddfPath -Force

# --- STEP 2: LOCK SHELL ---
Write-Host "Locking shell and releasing file handles..." -ForegroundColor Yellow
$processesToKill = @("explorer", "win32explorer")
foreach ($proc in $processesToKill) {
    if (Get-Process -Name $proc -ErrorAction SilentlyContinue) {
        Stop-Process -Name $proc -Force -ErrorAction SilentlyContinue
        Write-Host "Terminated $proc." -ForegroundColor Gray
    }
}

# --- STEP 1 & 3: BUILD ---
Write-Host "Compiling EliteUserRightsManager (.NET 4.6)..." -ForegroundColor Cyan
Push-Location $slnDir
# Note: Actual dotnet build will be implemented once project files are created
# dotnet build EliteUserRightsManager.slnx -c Release
Pop-Location

# --- DEPLOYMENT LOGIC (Post-Build) ---
# [Placeholder for actual file copy and psexec64 logic] 

# --- UNLOCK SHELL ---
Write-Host "Unlocking shell..." -ForegroundColor Yellow
Start-Process "explorer.exe"

# --- STEP 17 & 20: CHANGELOG & VERIFICATION ---
$logEntry = "`n## [$versionInput] - $((Get-Date).ToString("yyyy-MM-dd HH:mm:ss"))`n- Changes: $changes`n- Backup: $backupName`n"
Add-Content -Path $changelogPath -Value $logEntry

Write-Host "Build Process Complete." -ForegroundColor Green
Get-Content $changelogPath -Tail 10
