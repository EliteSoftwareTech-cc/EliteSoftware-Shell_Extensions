# MetadataEditor Project Context (GEMINI.md)

MetadataEditor is a professional Windows system utility for viewing and editing file metadata (Windows Property System) and Alternative Data Streams (ADS). It is designed to bridge the gap between low-level Win32 APIs and a modern, high-performance UI.

---

## 1. Project Directives

- **Maintain FIX_LOG.md**: Record all critical fixes and technical decisions to prevent regression.
- **Version Numbering**: Always update the 4-digit version number (`A.B.C.D`) in `CHANGELOG.md` and `MetadataEditor.iss`.
- **Reference Documentation**: Consult `guide.md` and `README.md` for architectural context.
- **Mandatory Backups**: Use `backup_and_build.ps1` to perform CAB-compressed source backups before major changes.

---

## 2. Architecture & Tech Stack

- **Target Framework**: .NET Framework 4.8 (required for deep shell compatibility).
- **Metadata Editor App**: WinForms-based main interface.
- **Metadata Editor Engine**: Class library for low-level resource extraction and metadata/ADS manipulation.
- **Metadata Editor Shell Extension**: SharpShell-powered library for Explorer integration.

---

## 3. Key Technical Conventions

### 3.1 Metadata & Property System
- **API**: Uses `IPropertyStore` (propsys.dll) for native property access.
- **Discovery**: Performed asynchronously (up to 800 fields) and cached at `%AppData%\MetadataEditor\property_cache.txt`.
- **Asynchronous Load**: UI should populate instantly from cache and "fill in" discovered fields via the `SchemaUpdated` event.

### 3.2 Alternative Data Streams (ADS)
- **API**: Uses `FindFirstStreamW` and `:streamname` syntax for NTFS manipulation.
- **Feature**: Supports "Add Custom ADS Field" to attach invisible string data to files.

### 3.3 UI Styling (Teal Structural Theme)
- **Grid Categories**: White text on Teal background (`CategorySplitterColor` + `CategoryForeColor`).
- **Help/Commands**: Teal background with White text (`HelpBackColor` / `CommandsBackColor`).
- **Grid Lines**: DimGray (`LineColor`).
- **Form Design**: Use OS-native painting for form/control backgrounds.

---

## 4. Operational Workflows

### 4.1 Development Cycle
1. **Implementation**: Maintain surgical updates across the engine, app, and shell extension.
2. **Verification**: Use `TEST_ENVIRONMENT\Test_Registration.ps1` to validate shell tab visibility.
3. **Backup and Build**: Execute `.\backup_and_build.ps1` to compile, backup, and generate the installer.

### 4.2 Shell Integration (GUIDs)
- **Property Sheet**: `{6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}`
- **Context Menu**: `{9A8B7C6D-5E4F-3D2C-1B0A-9F8E7D6C5B4A}`

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman
