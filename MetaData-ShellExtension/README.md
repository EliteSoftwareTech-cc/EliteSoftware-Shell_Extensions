# MetadataEditor (Professional Edition)

**The ultimate command-center for Windows Metadata and Alternative Data Streams (ADS).**  
*Surgical precision for NTFS file properties and hidden system data.*

MetadataEditor is a high-performance system utility designed for power users and developers who need to view, edit, and manage the underlying metadata structures of the Windows file system. It provides a unified interface for the native Windows Property System and hidden NTFS Alternative Data Streams.

## 🚀 Key Features

### 📄 System-Wide Metadata Editing
*   **800+ Field Deep Scan**: Automatically discovers and maps hundreds of native Windows property fields.
*   **Asynchronous "Fill-In" Loading**: The UI loads instantly and populates discovered fields in the background for zero-lag performance.
*   **Native Integration**: Uses the official Windows `IPropertyStore` API to ensure changes are recognized globally by Windows Search and Explorer.
*   **Property Grid Interface**: A professional, categorized view with White-on-Teal headers and DimGray grid lines.

### 🕵️ Alternative Data Stream (ADS) Management
*   **Hidden Stream Discovery**: Automatically enumerates hidden NTFS streams that are invisible to standard file managers.
*   **Custom Data Injection**: Add your own custom ADS fields to any file to store sidecar data, audit logs, or project-specific tags without altering the main file content.
*   **Full ADS Editor**: Create, Read, Update, and Delete (CRUD) operations for any alternate stream.

### 🔗 Windows Shell Integration
*   **Surgical Property Sheet**: A custom "Metadata" tab injected directly into the Windows File Properties dialog for instant access.
*   **Global Context Menu**: Right-click any file or folder to "Open in Metadata Editor" for deep inspection.
*   **Smart Pickers**: Specialized CLI flags (`-pick` and `-pick_advanced`) allow other scripts to use MetadataEditor as a visual property selector.

### 🎨 Modern Performance & UI
*   **Persistent Schema Cache**: Discovered property indices are cached at `%AppData%\MetadataEditor\property_cache.txt` for instant subsequent loads.
*   **Thread-Safe Engine**: High-performance, non-blocking metadata extraction ensures the UI stays responsive even when parsing massive system directories.
*   **OS-Native Design**: Reverted custom window painting to respect system themes while preserving specialized PropertyGrid accents.

## 🏗️ Architectural Overview

### 1. The Application (`MetadataEditor.App.exe`)
The main WinForms interface for property editing, ADS management, and standalone file inspection.

### 2. The Engine (`MetadataEditor.Engine.dll`)
The heart of the project. It handles low-level COM Interop with `propsys.dll` and P/Invoke calls to the Windows Kernel for NTFS stream manipulation.

### 3. The Shell Extension (`MetadataEditor.ShellExtension.dll`)
A SharpShell-powered extension that handles the deep integration with `explorer.exe`, providing the property sheet and context menu handlers.

## ⚙️ Technical Deep-Dive

### NTFS Alternative Data Streams
MetadataEditor leverages the `FindFirstStreamW` and `GetFileInformationByHandleEx` APIs to peer into the NTFS Master File Table (MFT) and extract data stored in the `:streamname` format. This allows for persistent, hidden data storage that travels with the file across NTFS drives.

### The Windows Property System
By using the `SHGetPropertyStoreFromParsingName` API, the application accesses the unified property system used by Windows 10 and 11, ensuring that metadata edits are compliant with modern shell standards and indexing services.

## 📥 Installation

1.  Download **MetadataEditor_Setup_vX.X.X.exe**.
2.  Run the installer as Administrator.
3.  The installer will register the COM servers and restart Explorer to activate the "Metadata" tab.

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman
