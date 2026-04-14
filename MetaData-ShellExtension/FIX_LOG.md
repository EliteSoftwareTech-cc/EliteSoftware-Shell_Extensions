# MetadataEditor Fix & Mistake Log

This log tracks technical hurdles, mistakes, and their subsequent fixes to prevent regressions and repeated errors during development. **Always reference this log before making major architectural changes.**

---

## [2026-04-13] - .NET 8 to .NET 4.8 Transition

### Issue: .NET 8 Shell Extension Registration Failure
- **Mistake:** Initially implemented the Shell Extension using .NET 8. While .NET 8 supports COM hosting via `comhost.dll`, the version of SharpShell (2.7.2) and the Windows Shell itself are far more stable when targeting the native .NET Framework. `ServerManager.exe` failed to recognize the .NET 8 assemblies as valid SharpShell servers.
- **Fix:** Ported the entire solution (App, Engine, and Shell Extension) to **.NET Framework 4.8**.
- **Intent:** Achieve 100% compatibility with `regasm.exe`, SharpShell tools, and ensure maximum stability within the `explorer.exe` process.

### Issue: C# Language Version Mismatch
- **Mistake:** Targeting `net48` in SDK-style projects defaults to C# 7.3, which caused build failures because the code used modern features like `Nullable` and `ImplicitUsings`.
- **Fix:** Explicitly added `<LangVersion>latest</LangVersion>` to all `.csproj` files.
- **Intent:** Retain modern C# syntax and safety features while targeting the legacy .NET Framework.

---

## [2026-04-13] - Inno Setup & Registration Fixes

### Issue: Inno Setup Quote Escaping
- **Mistake:** Used `\`"` to escape paths in the `[Run]` section of `MetadataEditor.iss`. Inno Setup does not recognize backslash escaping for parameters; it requires double-double quotes (`""`).
- **Fix:** Updated all `regasm.exe` calls to use `""` for file paths.
- **Intent:** Ensure the installer can handle installation paths that contain spaces.

### Issue: Incorrect Inno Setup Parameter
- **Mistake:** Used `Exclude` in the `[Files]` section to skip architecture folders.
- **Fix:** Corrected the parameter name to `Excludes` (plural).
- **Intent:** Resolve installer compilation errors.

### Issue: Shell Extension Discovery
- **Mistake:** Omitted `[DisplayName]` and `[RegistrationName]` attributes in the `IconPropertySheetExtension` class.
- **Fix:** Added both attributes to `PropertySheetExtension.cs`.
- **Intent:** Improve discovery and professional labeling within the SharpShell Server Manager and Windows Registry.

---

## [2026-04-13] - Build & Asset Management

### Issue: Missing App Icon
- **Mistake:** Pointed to `ICON_FOR_METADATA_EDITOR.ico` in the project folder, but the file was actually in the solution root.
- **Fix:** Updated the relative path in `METADATA_EDITOR_APP.csproj` to `..\ICON_FOR_METADATA_EDITOR.ico`.
- **Intent:** Resolve build failure.

### Issue: Stale Files on Reinstall
- **Mistake:** The installer was overwriting files but not cleaning the directory, leading to potential conflicts with older `.deps.json` or `.pdb` files.
- **Fix:** Added `[InstallDelete]` to wipe the `{app}` directory and implemented a `CurStepChanged` procedure in `[Code]` to unregister the old DLLs *before* the new ones are installed.
- **Intent:** Guarantee a "Clean Reinstall" state for every update.


