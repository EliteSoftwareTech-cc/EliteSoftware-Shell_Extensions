# EliteUserRightsManager (User Rights Manager)

"Bringing 2006 to 2026 one line of code at a time."

EliteUserRightsManager is a professional Windows Control Panel Applet (.cpl) designed for surgical manipulation of Local Security Authority (LSA) policies. It targets .NET Framework 4.6 to ensure native compatibility with Windows Vista, 7, 8, 10, and 11.

## 🏛️ Project Overview
- **Project Name:** EliteUserRightsManager
- **Developer:** Zachary Whiteman
- **Organization:** EliteSoftwareTech Co.
- **Target Framework:** .NET Framework 4.6 (Strictly enforced for Windows Vista compatibility)
- **Output Type:** Control Panel Applet (.cpl)
- **Platform:** Windows x64 (Native PE Resource Integration)

## 🛠️ Key Features
- **LSA Policy Integration:** Deep access to `advapi32.dll` for system security database management.
- **Staff Token Injection:** Programmatic conversion of `Elite Software-Staff` group names to SIDs.
- **NT Privilege Overdrive:** "Apply All" capability to loop through and assign every privilege string available in the NT Kernel.
- **EliteSoftware UI Parity:** Centered status boxes with teal/gray transitions, Win32 Common Controls, and signature high-density layouts.

## 🚀 Build & Deployment (Golden Standard)
Builds **MUST** be executed strictly via `backup_and_build.ps1`. Manual builds are prohibited.
1. **CAB Archival:** Automatic `makecab.exe` backup of the entire root before every build.
2. **Lock Release:** Forceful termination of `explorer.exe` and `Win32Explorer.exe`.
3. **Deployment:** Automatic renaming to `.cpl` and deployment to `System32` via `psexec64`.

## 🆔 Static GUID/CLSID Registry (Troubleshooting)
To prevent shell caching issues, use these unique identifiers for major build iterations:

- **Build v1.0.0.X:** `{7F3A4B5C-6D7E-8F9A-0B1C-2D3E4F5A6B7C}`
- **Build v1.1.0.X:** `{8E4B5C6D-7F8A-9B0C-1D2E-3F4A5B6C7D8E}`
- **Build v1.2.0.X:** `{9F5C6D7E-8A9B-0C1D-2E3F-4A5B6C7D8E9F}`
- **Build v1.3.0.X:** `{A06D7E8F-9B0C-1D2E-3F4A5B6C7D8E9F0A}`
- **Build v1.4.0.X:** `{B17E8F9A-0C1D-2E3F-4A5B6C7D8E9F0A1B}`

---
© 2026 EliteSoftwareTech Co.  
*Part of the EliteSoftware Shell Extensions & CPL Suite.*
