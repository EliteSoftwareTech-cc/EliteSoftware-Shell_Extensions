# IconExplorer

**High-performance icon extraction and management for Windows.**  
*Bringing 2006 to 2026 one line of code at a time.*

IconExplorer is a professional utility designed for surgically extracting, viewing, and integrating high-fidelity visual assets from Windows Executable (PE) files. It bridges the gap between low-level Win32 resource manipulation and a modern, high-performance UI.

## 🚀 Key Features

### 💎 High-Fidelity Extraction
*   **Exact Reconstruction**: Reconstructs entire `RT_GROUP_ICON` structures for mathematically perfect 256x256 PNG-compressed icons.
*   **Universal Support**: Automatically resolves icons from the **Default Handler** for any file type.
*   **Deep Inspection**: Peer into `.dll`, `.exe`, `.mui`, `.mun`, `.cpl`, `.scr`, and `.icl` files.

### 🔗 Shell Integration
*   **Property Sheet Tab**: Inspect icons directly from the Windows file properties dialog.
*   **Global Context Menu**: Right-click any file or folder to "Open in Icon Explorer."
*   **Smart Pickers**: Specialized `-pick` and `-pick_advanced` modes for integration with third-party scripts and workflows.

### 🎨 Modern UI/UX
*   **Dynamic Scaling**: Real-time icon zooming from 16px to 256px with debounced rendering.
*   **Power-User Tools**: "Select All," "Extract All" (with folder organization), and "Toggle IDs."
*   **Zero Lag**: Asynchronous loading and robust image caching handle thousands of icons effortlessly.

## 🏗️ Architectural Overview

The IconExplorer solution is divided into two distinct projects to ensure a clean separation of concerns. This modularity makes the codebase easier to maintain, test, and eventually integrate into other applications.

### 1. The Application (`IconExplorer.App.exe`)
This is the front-end Windows Forms (WinForms) application. It handles all user interactions, rendering, scaling, file dialogues, and context menus. It is responsible for parsing command-line arguments and deciding whether the application should run in its standard graphical mode or one of its headless/integration modes (like `-pick`).

### 2. The Engine (`IconExplorer.Engine.dll`)
This is the back-end Class Library. It contains absolutely zero User Interface code. Its sole purpose is to communicate directly with the Windows Operating System at a low level using P/Invoke (Platform Invocation Services). It reaches into the binary structure of other files, locates resource sections, and parses raw bytes into usable C# `System.Drawing.Icon` objects. 

Because this logic is housed in a separate DLL, any other .NET application in the future could reference `IconExplorer.Engine.dll` to gain advanced icon extraction capabilities without needing to load the IconExplorer GUI.

### 3. The Shell Extension (`IconExplorer.ShellExtension.dll`)
A SharpShell-powered C# library that injects IconExplorer functionality directly into the Windows Shell. This powers both the Property Sheet Tab for easy viewing of icons and the Global Context Menu ("Open in Icon Explorer").

## ⚙️ How the Extraction Engine Works (Deep Dive)

Windows Executables (`.exe`) and Libraries (`.dll`) are formatted using the **PE (Portable Executable)** format. Inside a PE file, there are various "sections" (`.text` for code, `.data` for variables, etc.). One of these sections is the `.rsrc` (Resource) section.

When you type `C:\Windows\System32\imageres.dll` into IconExplorer and hit Enter, the `EliteIconExtractor` engine springs into action.

1. **`LoadLibraryEx`**: The engine asks the Windows Kernel (`kernel32.dll`) to map the target DLL into our application's memory as a "data file" (meaning it doesn't try to execute any code inside it, it just reads it like a book).
2. **`PrivateExtractIconsW`**: To quickly populate the visual list, the engine uses the Windows `user32.dll` API to rip the cached, standard-sized representations of the icons from the file. This provides the fast, asynchronous loading you see in the UI.
3. **`EnumResourceNames`**: When you actually click "Extract Selected", the engine must do the heavy lifting. A single "Icon" in Windows is rarely just one image; it is an **Icon Group** (`RT_GROUP_ICON`). An Icon Group contains multiple resolutions of the same image (e.g., 16x16, 32x32, 64x64, 256x256) so Windows can display the best one depending on your monitor's DPI scaling.
4. **Binary Reconstruction**: The engine locates the exact memory address of the Icon Group header. It reads the header to find out how many different resolutions exist. It then jumps to the `RT_ICON` memory addresses to grab the raw PNG or BMP pixel data for each resolution. Finally, it uses a `BinaryWriter` to stitch all of this raw data back together, completely from scratch, into a perfectly valid `.ico` file on your Desktop.

This low-level approach ensures that the extracted icons are 100% mathematically identical to what is stored in the DLL, preserving ultra-high-resolution 256x256 PNG-compressed icons perfectly.

## 🛠️ Usage Guide

### Standard Desktop Mode
If you launch `IconExplorer.App.exe` by double-clicking it, you enter standard desktop mode.
- **Path Box**: Type the path to any `.exe`, `.dll`, `.icl`, or `.cpl` file. Press **Enter** to load.
- **Browse Button**: Opens a standard Windows file dialog to locate a file.
- **Properties Button**: View the properties of the selected file container directly within the app.
- **Asynchronous Loading**: The status bar will show the loading progress. Because it loads asynchronously, the UI will not freeze, even if you load a file with 5,000 icons.

### Interacting with the List
- **Scrolling and Zooming**: Hold the **Control (Ctrl)** key on your keyboard and scroll your mouse wheel up or down. The icons will dynamically scale from 16x16 up to 256x256. 
- **Toggle IDs**: Clicks this button to overlay the internal Resource ID number underneath every icon. This is crucial if you are a developer referencing icons in code.
- **Multi-Select Mode**: Click to toggle on checkboxes. You can check dozens of icons and hit extract to dump them all at once.
- **Right-Click Context Menu**: Right-clicking an icon opens a custom menu. 
  - **Open**: Extracts the icon to a temporary folder and opens it in your default image viewer (e.g., Windows Photos).
  - **Send To**: Dynamically reads your Windows `SendTo` folder. You can send the icon directly to Photoshop, a batch script, or any other shortcut you have configured.
  - **Show in Explorer**: Opens the temporary folder highlighting the physical `.ico` file.

### Advanced Shell Integration: Picker Modes
If you launch the application via PowerShell, Command Prompt, or from another application, you can pass specialized flags:

- **Strict Picker Mode (`-pick`)**: `.\IconExplorer.App.exe -pick "C:\Windows\System32\shell32.dll"`
  Transforms the UI into a minimal dialog for integration with other scripts. Double-clicking an icon returns `FilePath,Index` to `STDOUT` and exits.

- **Advanced Picker Mode (`-pick_advanced`)**: `.\IconExplorer.App.exe -pick_advanced "C:\Windows\System32\shell32.dll"`
  Similar to `-pick` but retains full extraction and context menu features for power users.

## 📥 Installation

1.  Download the latest **IconExplorer_Setup_vX.X.X.exe** from the Releases section.
2.  Run the installer (Administrator privileges required for shell integration).
3.  The installer will automatically register the shell extensions and restart Explorer for you.

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman