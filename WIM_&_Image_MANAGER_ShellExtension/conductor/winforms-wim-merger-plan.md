# WinForms WIM & Installer Manager (Native Legacy)

## Objective
Create a .NET Framework 4.8 WinForms application that allows users to merge multiple Windows ISO files, slipstream drivers into both boot and install WIMs, and heavily customize the installer (EULA, wallpaper, signature enforcement, boot branding, and drive icon). The application will use native CLI tools (`7z.exe`, `dism.exe`, `oscdimg.exe`, `bcdedit.exe`) and retain a standard, un-themed OS-native appearance with Visual Styles enabled.

## Key Files & Context
- `Z:\WIM_AND_INSTALLER_MANAGER\WimMergeApp\WimMergeApp.sln`: Visual Studio Solution
- `Z:\WIM_AND_INSTALLER_MANAGER\WimMergeApp\MainForm.cs`: Main UI containing configuration tabs.
- `Z:\WIM_AND_INSTALLER_MANAGER\WimMergeApp\Core\IsoExtractor.cs`: Wrapper for `7z.exe` to extract ISO contents.
- `Z:\WIM_AND_INSTALLER_MANAGER\WimMergeApp\Core\DismManager.cs`: Wrapper for `dism.exe` to mount WIMs, export/merge editions, and inject drivers.
- `Z:\WIM_AND_INSTALLER_MANAGER\WimMergeApp\Core\CustomizationEngine.cs`: Handles EULA replacement, BCD boot menu branding, Setup Wallpaper patching, and Unattend.xml generation for signature enforcement.
- `Z:\WIM_AND_INSTALLER_MANAGER\WimMergeApp\Core\IsoBuilder.cs`: Wrapper for `oscdimg.exe` to compile the customized installation files back into a bootable ISO.

## Implementation Steps
1. **Initialize Solution & UI**:
   - Create a C# WinForms .NET Framework 4.8 project.
   - Configure `Program.cs` with `Application.EnableVisualStyles();`.
   - Design `MainForm.cs` using native WinForms controls (TabControl, ListBox, TextBoxes, Buttons) with `UseVisualStyleBackColor = true`.
   - Tabs: 
     - **ISO Inputs**: Add multiple ISOs to merge (reads from `ISO_INPUT` or browse).
     - **Drivers**: Browse for driver root directory.
     - **Customization**: Boot Menu Branding, Drive Icon, Wallpaper selection, EULA (RTF format).
     - **Settings**: Disable Driver Signature Enforcement checkbox.
     - **Build**: Output ISO path and Build Log console.
2. **CLI Tools Integration**:
   - Download or instruct placement of `7z.exe`, `7z.dll`, and `oscdimg.exe` in a `Tools` folder.
   - Write helper classes to execute these tools asynchronously, redirecting standard output to the UI log window.
3. **Core Logic Implementation**:
   - **Extraction**: Extract the "Base" ISO (the first one) to a working directory. Extract additional ISOs to temporary directories.
   - **WIM Merging**:
     - Use `dism /Get-WimInfo` to list editions in the extra ISOs.
     - Use `dism /Export-Image` to append each edition's `install.wim` to the Base ISO's `sources\install.wim`.
     - Update the Edition ID / Name during export to user-defined names (e.g., "Tiny 7").
   - **Driver Slipstreaming**:
     - Mount `boot.wim` (both indexes) and `install.wim` (all indexes) using `dism /Mount-Wim`.
     - Use `dism /Add-Driver /recurse` to inject drivers from the selected directory.
     - Unmount and commit changes.
   - **Customizations**:
     - **Drive Icon**: Generate `autorun.inf` and copy the selected `.ico` to the Base ISO root.
     - **Boot Menu Branding**: Use `bcdedit /store <path_to_iso_boot_bcd> /set {default} description "Custom Setup"` to modify boot titles.
     - **EULA**: Overwrite `sources\license.rtf` with the user's custom RTF.
     - **Wallpaper**: Replace `setup.bmp` in the mounted `boot.wim` `sources\` directory.
     - **Signature Enforcement**: Modify WinPE BCD to set `testsigning on` and `nointegritychecks on`. Generate an `$OEM$\\\$\\Setup\\Scripts\\SetupComplete.cmd` file in the Base ISO to run `bcdedit` commands on the installed OS.
4. **ISO Compilation**:
   - Use `oscdimg.exe -m -o -u2 -udfver102 -bootdata:2#p0,e,b<path_to_etfsboot.com>#pEF,e,b<path_to_efisys.bin> -l"<ISOLabel>" <working_dir> <output.iso>` to create a UEFI/BIOS compatible hybrid bootable ISO.

## Verification & Testing
- Build the app and run as Administrator.
- Provide two different Windows ISOs (e.g., Win10 and Win11) and a Driver folder.
- Configure customizations (Wallpaper, EULA, Boot Title).
- Execute the build process.
- Boot the resulting ISO in a Virtual Machine (Hyper-V or VirtualBox).
- Verify: Boot menu reflects custom branding, Setup shows custom wallpaper/EULA, Drive icon works, both OS editions are available in the setup menu, and Driver Signature Enforcement is disabled in the newly installed OS.
