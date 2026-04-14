# - Always reference and maintain FIX_LOG.md before and after major changes to prevent regression and repeated technical errors.
# - Always Update Changelog after any changes and update version number using a 4 didget system 1.2.3.4 A.B.C.D X.X.X.X
# - Always reference the guide.md file and CHANGELOG.md file
---
# Backups are to be made prior to making any changes to the codebase. ALWAYS BACK UP ENTIRE SOURCE CODE AS WELL AS BUILDS. ZIP FILES OR IDEALLY CAB FILES
---
# IconExplorer Project Context

IconExplorer is a high-performance utility designed for viewing, extracting, and integrating visual assets (icons) from Windows Executable (PE) files. It consists of a modular C# .NET 8.0 architecture, having evolved from a specialized PowerShell prototype.

## Architecture & Project Structure

- **IconExplorer.App** (`IconExplorer.App/`): The WinForms frontend application.
  - `MainForm.cs`: Handles UI logic, asynchronous icon loading, debounced zooming (Ctrl+Scroll), and specialized "Picker" modes.
  - `Program.cs`: Application entry point.
- **IconExplorer.Engine** (`IconExplorer.Engine/`): A reusable Class Library for low-level resource extraction.
  - `EliteIconExtractor.cs`: Uses P/Invoke (`PrivateExtractIconsW`, `LoadLibraryEx`, etc.) and manual binary reconstruction to extract high-fidelity 256x256 PNG-compressed icons from `.rsrc` sections.
- **IconExplorer.ShellExtension** (`IconExplorer.ShellExtension/`): A planned project for integrating IconExplorer directly into the Windows Shell.
- **Prototypes & Scripts**:
  - `Icon_Explorer.PS1`: The original, fully-functional PowerShell prototype.
  - `backup_and_build.ps1`: The primary developer workflow script for building, versioning, and backing up the solution.

## Building and Running

### Development Build
```powershell
dotnet build IconExplorer.slnx -c Debug
```

### Production Workflow (Recommended)
Use the automated script to build, version, and backup:
```powershell
.\backup_and_build.ps1
```
This script performs a `Release` build, prompts for version info, updates `CHANGELOG.md`, and creates a timestamped ZIP in the `Backups/` directory.

### Running the App
The executable is located at `IconExplorer.App/bin/Release/net8.0-windows/IconExplorer.App.exe`.

## Core Features & Usage Modes

- **Standard Mode**: Full GUI for browsing and extracting icons to the Desktop.
- **Strict Picker Mode (`-pick`)**: Transforms the UI into a minimal dialog for integration with other scripts. Double-clicking an icon returns `FilePath,Index` to `STDOUT` and exits.
- **Advanced Picker Mode (`-pick_advanced`)**: Similar to `-pick` but retains full extraction and context menu features for power users.
- **High-Fidelity Extraction**: Unlike standard extractors, this engine reconstructs the entire `RT_GROUP_ICON` structure, ensuring zero quality loss for high-resolution assets.

## Technical Conventions

- **Low-Level Interop**: Use `IconExplorer.Engine` for any tasks involving PE resource manipulation. Avoid duplicating P/Invoke signatures in the UI layer.
- **Async & Performance**: The UI uses a custom `asyncTimer` for non-blocking icon loading and a `zoomTimer` for debounced scaling to prevent GDI+ memory pressure.
- **Documentation**: Refer to `guide.md` for a deep dive into the PE resource extraction logic and Win32 API usage.


___

# IconExplorer Comprehensive Guide and Technical Documentation

## 1. Introduction

Welcome to the comprehensive guide for **IconExplorer**, a high-performance utility designed for compiling, viewing, extracting, and integrating visual assets (icons) from Windows Executable (PE) files. Originally prototyped as a highly advanced PowerShell script (`Icon_Explorer.PS1`), the project has now been fully ported to a compiled, native C# .NET 8.0 architecture. 

The ultimate goal of IconExplorer is to serve as a vastly superior, standalone replacement for the aging native Windows icon browsing dialog (the property pane you see when changing a shortcut's icon). Not only does it allow you to peer into the hidden binary structures of Windows `.dll` and `.exe` files, but it also allows surgical extraction of high-fidelity icon groups, dynamic UI scaling, multi-file processing, and seamless command-line integration for third-party scripts.

This document serves as both a user manual and a technical deep-dive into the architecture of the application, explaining exactly how it works under the hood, the role of DLL files, and how to leverage its advanced shell integration modes.

---

## 2. Architectural Overview

The IconExplorer solution is divided into two distinct projects to ensure a clean separation of concerns. This modularity makes the codebase easier to maintain, test, and eventually integrate into other applications (such as a Windows Shell Extension).

### 2.1 The Application (`IconExplorer.App.exe`)
This is the front-end Windows Forms (WinForms) application. It handles all user interactions, rendering, scaling, file dialogues, and context menus. It is responsible for parsing command-line arguments and deciding whether the application should run in its standard graphical mode or one of its headless/integration modes (like `-pick`).

### 2.2 The Engine (`IconExplorer.Engine.dll`)
This is the back-end Class Library. It contains absolutely zero User Interface code. Its sole purpose is to communicate directly with the Windows Operating System at a low level using P/Invoke (Platform Invocation Services). It reaches into the binary structure of other files, locates resource sections, and parses raw bytes into usable C# `System.Drawing.Icon` objects. 

Because this logic is housed in a separate DLL, any other .NET application in the future could reference `IconExplorer.Engine.dll` to gain advanced icon extraction capabilities without needing to load the IconExplorer GUI.

---

## 3. Understanding the DLL Files

When you build IconExplorer, you will notice several `.dll` (Dynamic Link Library) files generated in the root directory. To understand how IconExplorer works, you must understand the three different types of DLLs involved in this ecosystem.

### Type 1: The Application Framework DLL (`IconExplorer.App.dll`)
In modern .NET (from .NET Core onwards, including .NET 8), the `.exe` file is actually just a very thin, lightweight "host" or "bootstrapper" executable. The actual compiled code of your application's user interface is stored in `IconExplorer.App.dll`. When you double-click the `.exe`, it spins up the .NET runtime, reads the `IconExplorer.App.runtimeconfig.json` file, and loads `IconExplorer.App.dll` into memory to run your app. 

### Type 2: The Logic Library (`IconExplorer.Engine.dll`)
As mentioned above, this contains the `EliteIconExtractor` class. It is compiled separately. When `IconExplorer.App.dll` needs to extract an icon, it calls a function inside `IconExplorer.Engine.dll`. This is the essence of modular programming.

### Type 3: Target Resource DLLs (`imageres.dll`, `shell32.dll`)
These are the files you are actually *inspecting* with the application. Windows stores the vast majority of its system icons, cursors, bitmaps, and UI strings inside DLL files rather than as loose files on your hard drive. 
- **`shell32.dll`**: Contains classic Windows icons (folders, hard drives, control panel items).
- **`imageres.dll`**: Contains modern, high-resolution Windows icons introduced in Vista/Windows 7/10/11.

IconExplorer's primary job is to open these Target Resource DLLs, read them, and show you what is inside.

---

## 4. How the Extraction Engine Works (Deep Dive)

Windows Executables (`.exe`) and Libraries (`.dll`) are formatted using the **PE (Portable Executable)** format. Inside a PE file, there are various "sections" (`.text` for code, `.data` for variables, etc.). One of these sections is the `.rsrc` (Resource) section.

When you type `C:\Windows\System32\imageres.dll` into IconExplorer and hit Enter, the `EliteIconExtractor` engine springs into action.

1. **`LoadLibraryEx`**: The engine asks the Windows Kernel (`kernel32.dll`) to map the target DLL into our application's memory as a "data file" (meaning it doesn't try to execute any code inside it, it just reads it like a book).
2. **`PrivateExtractIconsW`**: To quickly populate the visual list, the engine uses the Windows `user32.dll` API to rip the cached, standard-sized representations of the icons from the file. This provides the fast, asynchronous loading you see in the UI.
3. **`EnumResourceNames`**: When you actually click "Extract Selected", the engine must do the heavy lifting. A single "Icon" in Windows is rarely just one image; it is an **Icon Group** (`RT_GROUP_ICON`). An Icon Group contains multiple resolutions of the same image (e.g., 16x16, 32x32, 64x64, 256x256) so Windows can display the best one depending on your monitor's DPI scaling.
4. **Binary Reconstruction**: The engine locates the exact memory address of the Icon Group header. It reads the header to find out how many different resolutions exist. It then jumps to the `RT_ICON` memory addresses to grab the raw PNG or BMP pixel data for each resolution. Finally, it uses a `BinaryWriter` to stitch all of this raw data back together, completely from scratch, into a perfectly valid `.ico` file on your Desktop.

This low-level approach ensures that the extracted icons are 100% mathematically identical to what is stored in the DLL, preserving ultra-high-resolution 256x256 PNG-compressed icons perfectly.

---

## 5. Usage Guide: Standard Desktop Mode

If you launch `IconExplorer.App.exe` by double-clicking it, you enter standard desktop mode. 

### Browsing and Loading
- **Path Box**: Type the path to any `.exe`, `.dll`, `.icl`, or `.cpl` file. Press **Enter** to load.
- **Browse Button**: Opens a standard Windows file dialog to locate a file.
- **Asynchronous Loading**: The status bar will show the loading progress. Because it loads asynchronously, the UI will not freeze, even if you load a file with 5,000 icons.

### Interacting with the List
- **Scrolling and Zooming**: Hold the **Control (Ctrl)** key on your keyboard and scroll your mouse wheel up or down. The icons will dynamically scale from 16x16 up to 256x256. The application uses a custom "debouncing" timer and an in-memory image cache. This ensures that scaling is incredibly smooth and prevents the application from crashing due to GDI+ memory leaks (a common issue in standard C# WinForms apps).
- **Toggle IDs**: Clicks this button to overlay the internal Resource ID number underneath every icon. This is crucial if you are a developer referencing icons in code.
- **Multi-Select Mode**: Click to toggle on checkboxes. You can check dozens of icons and hit extract to dump them all at once.
- **Right-Click Context Menu**: Right-clicking an icon opens a custom menu. 
  - **Open**: Extracts the icon to a temporary folder and opens it in your default image viewer (e.g., Windows Photos).
  - **Send To**: Dynamically reads your Windows `SendTo` folder. You can send the icon directly to Photoshop, a batch script, or any other shortcut you have configured.
  - **Show in Explorer**: Opens the temporary folder highlighting the physical `.ico` file.

### Extraction
Select an icon (or multiple) and click **Extract Selected**. The raw `.ico` files will be perfectly reconstructed and saved directly to your Desktop. If you select "Open folder after extract", Windows Explorer will immediately pop open to show you your new files.

---

## 6. Advanced Shell Integration: Picker Modes

The true power of IconExplorer lies in its ability to be integrated into larger workflows, scripts, or future shell extensions. This is achieved via command-line arguments.

If you launch the application via PowerShell, Command Prompt, or from another application, you can pass specialized flags.

### 6.1 Strict Picker Mode (`-pick`)
**Command:** `.\IconExplorer.App.exe -pick "C:\Windows\System32\shell32.dll"`

When launched with `-pick`, IconExplorer transforms from a standalone tool into a localized dialog box.
- The "Extract" and "Multi-Select" buttons are completely hidden to prevent user distraction.
- The "Restore Size" button shifts left to fill the void.
- **Behavior Change**: When the user double-clicks an icon, or selects an icon and clicks the "Done" button, the application **does not extract anything**. Instead, it hooks into the parent console window, prints a comma-separated string containing the file path and the Icon ID, and immediately terminates.

**Example Output:**
`C:\Windows\System32\shell32.dll,42`

**Why is this useful?** 
Imagine you are writing a script that changes the icon of a folder. You can pause your script, launch IconExplorer in `-pick` mode, let the user visually select an icon, and when they click "Done", your script reads the output and applies the icon.

**PowerShell Integration Example:**
```powershell
$targetDll = "C:\Windows\System32\imageres.dll"
# Launch IconExplorer and capture the output string
$selection = .\IconExplorer.App.exe -pick $targetDll

if ($selection) {
    # Split the output "FilePath,Index"
    $parts = $selection.Split(',')
    $file = $parts[0]
    $index = $parts[1]
    
    Write-Host "The user chose Icon Index $index from $file"
    # You can now use these variables to write a desktop.ini file and change a folder icon!
} else {
    Write-Host "The user clicked Cancel or closed the window."
}
```

### 6.2 Advanced Picker Mode (`-pick_advanced`)
**Command:** `.\IconExplorer.App.exe -pick_advanced "C:\Windows\System32\shell32.dll"`

This mode behaves exactly like `-pick` in terms of returning the selected icon's data to the console when "Done" is clicked. However, it **does not hide the extraction features**. 

This is ideal for "Power Users". It allows a user to be using the tool as a picker for a script, but simultaneously retain the ability to extract a high-res `.ico` to their desktop, multi-select items, or use the right-click "Send To" context menu without having to close the app and reopen it in standard mode.

---

## 7. Development and Maintenance

### Building the Project
IconExplorer is a .NET 8.0 Solution (`.slnx`). To compile it from source, simply open a terminal in the project root and run:
`dotnet build IconExplorer.slnx -c Release`

### Automated Backup and Versioning System
Included in the root directory is `backup_and_build.ps1`. This is a critical CI/CD (Continuous Integration / Continuous Deployment) script.

Whenever you make changes to the C# code, do not just build it normally. Run `.\backup_and_build.ps1`.
1. The script will first compile the code. If the code has errors, it safely halts.
2. If successful, it prompts you for a Version Number (e.g., `1.1.0`).
3. It prompts you to write a brief description of what you changed or fixed.
4. It instantly compresses the entire source code directory into a `.zip` file with a timestamp and saves it to the `Backups/` folder.
5. It logs your version number, description, and the exact date into the `CHANGELOG.md` file.

This ensures you never lose progress, and you have a perfect, restorable historical record of every single working version of IconExplorer you have ever created.

---

## 8. Summary

IconExplorer represents a massive leap forward from standard icon viewing utilities. By bridging the gap between low-level Win32 memory manipulation and a modern, debounced, auto-scaling WinForms interface, it provides unparalleled extraction fidelity.

Through its intelligent `-pick` and `-pick_advanced` command-line flags, it is no longer just an isolated utility, but a modular, plug-and-play component ready to be injected into any automated PowerShell workflow, batch script, or future Windows Shell modification you desire to build. 

Enjoy exploring the hidden artistry inside your system's binaries!

---

## 9. Build System and Deployment (backup_and_build.ps1)

The `backup_and_build.ps1` script is the core CI/CD engine of this project. It is heavily customized to not only build the .NET binaries but to manage backups, create a native installer, and provide portable distributions.

### How the Build Script Works

When you run `.\backup_and_build.ps1 [Version] [Changelog_Message]`:
1. **Compilation:** It first compiles the `.slnx` solution using `dotnet build` in `Release` configuration. If the build fails, the script immediately halts to protect your backups.
2. **Aggregation:** It copies the compiled `.exe` and `.dll` artifacts from both the Application and Shell Extension projects into a unified `COMPLETE_BUILD` folder.
3. **CAB Backups:** The script reads all source files (excluding output directories and zip files) and dynamically generates a MakeCab Data Directive File (`backup.ddf`). It then runs `makecab.exe` to compress your raw source code into an ultra-dense, timestamped `.cab` file stored in the `Backups/` directory. CAB files provide vastly superior compression ratios compared to standard ZIPs when archiving text and source code. This is the ultimate, restorable snapshot of your work.
4. **Changelog:** It automatically appends to `CHANGELOG.md` with the new version, your commit message, and the specific `.cab` backup file name associated with that release.
5. **Inno Setup Installer Generation:** It invokes the Inno Setup Command-Line Compiler (`iscc.exe`) to read the `IconExplorer.iss` script. It compiles the `COMPLETE_BUILD` contents into a highly polished, standalone, native setup executable (e.g., `IconExplorer_Setup_vX.X.X.exe`) inside the `COMPLETE_BUILD_INSTALLER` folder. The installer features Admin elevation requests, Desktop/Start Menu shortcut creation, and automatic background COM Registration of the `.NET` Shell Extension.
6. **Portable ZIP Creation:** Finally, the script takes the `COMPLETE_BUILD` directory and zips it up as a standalone `.zip` (e.g., `IconExplorer_Portable_vX.X.X.zip`). This provides a "portable" distribution of the `.exe` and `.dll`s for users who wish to use IconExplorer without a formal installation process.

### Missing Features & Future Work
While the architecture is highly capable, some features are currently in development or planned:
- **Full Windows Shell Hook:** While the `IconExplorer.ShellExtension.comhost.dll` is correctly compiled and successfully registered via `regsvr32` during installation, the full registry mapping to inject a new "IconExplorer" tab directly into the native Windows right-click Properties menu (without altering the default "Change Icon" button) requires further development.
- **Format Conversion Exports:** Currently, IconExplorer focuses strictly on extracting mathematically exact `.ico` group reconstructions. The ability to easily extract individual resolutions as raw `.png` images is a future feature.
- **Search & Filter:** While the asynchronous loader handles massive files like `imageres.dll` easily, adding a real-time search box to quickly filter the view by Resource ID is a priority for a future update.

# Build Script Template Baseline Features

$ErrorActionPreference = "Stop"

$slnDir = "Z:\IcoHolder-master-2\IconExplorer(SCRIPT_VERSION)"
$backupDir = Join-Path $slnDir "Backups"
$changelogPath = Join-Path $slnDir "CHANGELOG.md"
$completeBuildDir = Join-Path $slnDir "COMPLETE_BUILD"

if (!(Test-Path $backupDir)) { New-Item -ItemType Directory -Path $backupDir | Out-Null }
if (!(Test-Path $completeBuildDir)) { New-Item -ItemType Directory -Path $completeBuildDir | Out-Null }
if (!(Test-Path $changelogPath)) { Set-Content -Path $changelogPath -Value "# Changelog`n" }

Write-Host "Building the ICON_EXPLORER_APP_SOLUTION solution..." -ForegroundColor Cyan
Push-Location $slnDir
dotnet build ICON_EXPLORER_APP_SOLUTION.slnx -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting backup."
}
Pop-Location

Write-Host "Copying files to COMPLETE_BUILD directory..." -ForegroundColor Cyan
Remove-Item -Path "$completeBuildDir\*" -Recurse -Force -ErrorAction SilentlyContinue

Copy-Item "$slnDir\ICON_EXPLORER_APP\bin\Release\net8.0-windows\*" -Destination $completeBuildDir -Recurse -Force
Copy-Item "$slnDir\ICON_EXPLORER_SHELL_EXTENSION\bin\Release\net8.0-windows\*" -Destination $completeBuildDir -Recurse -Force
# Ensure the engine is also copied if it outputs to its own folder, but they usually output to App folder as a dependency
Copy-Item "$slnDir\ICON_EXPLORER_ENGINE\bin\Release\net8.0\*" -Destination $completeBuildDir -Recurse -Force -ErrorAction SilentlyContinue

$exePath = Join-Path $completeBuildDir "ICON_EXPLORER_APP.exe"
$lnkPath = Join-Path $slnDir "ICON_EXPLORER_APP_LATEST.lnk"
if (Test-Path $lnkPath) { Remove-Item $lnkPath -Force }
$symlinkPath = Join-Path $slnDir "ICON_EXPLORER_APP_LATEST.exe"
if (Test-Path $symlinkPath) { Remove-Item $symlinkPath -Force }

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

$versionInput = "1.0.0"
if ($args.Count -gt 0) { $versionInput = $args[0] }
$changes = "Automated backup build"
if ($args.Count -gt 1) { $changes = $args[1] }

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupName = "ICON_EXPLORER_APP_v${versionInput}_${timestamp}.cab"
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

$filesToBackup = Get-ChildItem -Path $slnDir -Recurse -File | Where-Object { $_.FullName -notmatch "\\Backups\\" -and $_.FullName -notmatch "backup\.ddf$" -and $_.FullName -notmatch "\.zip$" }

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

$isccPath = "C:\Users\zwhiteman\AppData\Local\Programs\Inno Setup 6\iscc.exe"
if (Test-Path $isccPath) {
    Write-Host "Compiling Inno Setup Installer..." -ForegroundColor Cyan
    $issPath = Join-Path $slnDir "IconExplorer.iss"
    $installerOutDir = Join-Path $slnDir "COMPLETE_BUILD_INSTALLER"
    $previousInstallersDir = Join-Path $installerOutDir "PREVIOUS_INSTALLERS_VERSIONS"
    
    if (!(Test-Path $installerOutDir)) {
        New-Item -ItemType Directory -Path $installerOutDir | Out-Null
    }
    if (!(Test-Path $previousInstallersDir)) {
        New-Item -ItemType Directory -Path $previousInstallersDir | Out-Null
    }
    
    # Move existing installers to PREVIOUS_INSTALLERS_VERSIONS
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
