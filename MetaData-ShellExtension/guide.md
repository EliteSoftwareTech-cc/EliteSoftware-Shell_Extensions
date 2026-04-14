# MetadataEditor Comprehensive Technical Guide

## 1. Introduction

MetadataEditor is a professional Windows system utility for viewing and editing file metadata (Windows Property System) and Alternative Data Streams (ADS). It is designed to bridge the gap between low-level Win32 APIs and a modern, high-performance UI.

The goal of MetadataEditor is to provide surgical control over file-attached data that is typically hidden or difficult to access through standard Windows Explorer interfaces.

---

## 2. Architectural Overview

The solution is divided into three distinct modules to ensure a clean separation of concerns and maximum performance.

### 2.1 The Application (`MetadataEditor.App.exe`)
The WinForms frontend. It handles user interactions, property editing, and process management. It supports specialized "Picker" modes (`-pick`) for integration with third-party automation scripts.

### 2.2 The Engine (`MetadataEditor.Engine.dll`)
The core logic library. It contains zero UI code and communicates directly with the Windows Operating System using P/Invoke (Platform Invocation Services) and COM Interop.

### 2.3 The Shell Extension (`MetadataEditor.ShellExtension.dll`)
A SharpShell-powered library that injects MetadataEditor functionality directly into the Windows Shell via the Property Sheet ("Metadata" tab) and Global Context Menu.

---

## 3. The Metadata & ADS Engine (Deep Dive)

### 3.1 The Windows Property System (`IPropertyStore`)
MetadataEditor uses the modern Windows Property System (`propsys.dll`). 
1. **Property Discovery**: The engine performs an asynchronous scan of up to 800 shell property indices using `folder.GetDetailsOf`.
2. **Schema Caching**: Discovered property indices are cached at `%AppData%\MetadataEditor\property_cache.txt`. This ensures that subsequent loads are instantaneous while still allowing for background updates if the system schema changes.
3. **Property Writing**: Edits are committed via `IPropertyStore.SetValue` and `IPropertyStore.Commit`, ensuring metadata changes are fully indexed by Windows Search.

### 3.2 Alternative Data Streams (ADS)
NTFS allows files to have multiple data streams beyond the default `$DATA` stream.
1. **Enumeration**: Uses `FindFirstStreamW` to locate all alternate streams attached to a file.
2. **Manipulation**: Uses `CreateFileW` with the `:streamname` suffix to perform standard Read/Write operations on these hidden channels.
3. **Custom Fields**: The "Add Custom ADS Field" feature allows users to attach arbitrary string data to any file without modifying the primary file content.

---

## 4. UI Design and Performance

### 4.1 Asynchronous "Fill-In" Loading
To ensure the UI is never blocked, MetadataEditor uses a two-stage loading process:
1. **Stage 1 (Instant)**: The PropertyGrid is immediately populated with properties found in the persistent schema cache and standard file attributes.
2. **Stage 2 (Background)**: A background thread performs a deep scan of the target file's properties. As new information is found, the `SchemaUpdated` event is fired, and the `PropertyGrid` is refreshed to "fill in" the discovered fields.

### 4.2 Visual Theming
The application uses a "Unified Teal Structural Theme":
- **Category Headers**: White text on a Teal background (via `CategorySplitterColor`).
- **Grid Lines**: Dark Grey (`DimGray`).
- **OS-Native Rendering**: The form itself respects the user's OS theme (Dark/Light mode), while the PropertyGrid retains specialized branding accents for clarity.

---

## 5. Shell Integration & Integration Modes

### 5.1 The Metadata Tab
The Shell Extension injects a custom tab into the standard Windows Properties dialog. This tab uses the same asynchronous engine as the main app, allowing for deep metadata inspection without hanging `explorer.exe`.

### 5.2 Context Menu
The "Open in Metadata Editor" entry allows for instant deep-dive analysis of any file or folder on the system.

### 5.3 Picker Modes
For automation developers, the `-pick` and `-pick_advanced` flags transform the UI into a modal dialog that returns the selected property index to `STDOUT` upon closing.

---

## 6. Build and Deployment

The project uses `backup_and_build.ps1` for CI/CD:
1. **Build**: Compiles both x86 and x64 targets.
2. **Backup**: Creates a timestamped CAB archive of the source code.
3. **Installer**: Generates a native Inno Setup installer that handles COM registration and Explorer restarts.

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman
