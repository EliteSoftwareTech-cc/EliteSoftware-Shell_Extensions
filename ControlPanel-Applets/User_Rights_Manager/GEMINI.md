# EliteUserRightsManager AI Engineering Mandates (GEMINI.md)

This document takes absolute precedence over general workflows for the **EliteUserRightsManager** project.

## 🏛️ Foundational Mandates
- **Target Framework:** Strictly **.NET Framework 4.6**. This is a hard requirement for Windows Vista compatibility. Do NOT upgrade to 4.8 or .NET 6/7/8.
- **Output:** The final artifact must be renamed from `.dll` to `.cpl` and registered as a Control Panel Applet.
- **Administrative Context:** This tool requires `requireAdministrator` in its manifest and `System` level permissions (via `psexec64`) for deployment to `System32`.

## 🛠️ Build Automation (The Golden Standard)
- **Zero-Manual Builds:** Never use `dotnet build` directly. ALWAYS use `backup_and_build.ps1`.
- **Shell Locking:** `explorer.exe` and `Win32Explorer.exe` must be terminated before any file operations in `System32` and restarted only after verification.
- **CAB Archival:** A full source code backup using `makecab.exe` must precede every build.
- **Always on Top:** The build script and installer windows must be forced to the foreground/always-on-top to prevent user distraction and ensure visibility.

## 🎨 UI & Aesthetic Standards
- **EliteSoftware Vibe:** Centered status boxes, Teal/Gray gradients, Monsterrat typography.
- **Density:** High-density, command-center style layouts. No excessive white space.

## 🆔 Unique Identity
- **GUIDs:** Reference the static list in `README.md`. Generate a new one for every major build to avoid shell caching.

## 📋 Tracking
- **FIX_LOG.md:** Log every file-lock failure or architectural pivot here before fixing.
- **CHANGELOG.md:** Maintain 4-digit versioning (`1.0.0.0`) and link to the specific CAB backup filename.

"Bringing 2006 to 2026 one line of code at a time."
