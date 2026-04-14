#define MyAppName "MetadataEditor"
#ifndef MyAppVersion
#define MyAppVersion "1.3.3.0"
#endif
#define MyAppPublisher "EliteSoftwareTech Co."
#define MyAppAuthor "Zachary Whiteman"
#define MyAppCopyright "© 2026 EliteSoftwareTech Co. (April 2026)"
#define MyAppSlogan "Bringing 2006 to 2026 one line of code at a time."
#define MyAppExeName "METADATA_EDITOR_APP.exe"
#define MyShellDll "METADATA_EDITOR_SHELL_EXTENSION.dll"

; --- BRANDING ASSETS ---
#define MyLogo "LOGO_FOR_ELITE_SOFTWARE.png"

[Setup]
AppId={{160160B0-3DEB-465F-995B-2ED3E2DD937C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppCopyright={#MyAppCopyright}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=COMPLETE_BUILD_INSTALLER
OutputBaseFilename=MetadataEditor_Setup_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
SetupIconFile=MetadataEditor.ico

#ifdef MyLogo
WizardImageFile={#MyLogo}
WizardSmallImageFile={#MyLogo}
#endif

CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[InstallDelete]
Type: filesandordirs; Name: "{app}\*"

[Files]
Source: "COMPLETE_BUILD\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "COMPLETE_BUILD\METADATA_EDITOR_APP.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "COMPLETE_BUILD\METADATA_EDITOR_ENGINE.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "COMPLETE_BUILD\x64\*"; DestDir: "{app}"; Check: Is64BitInstallMode; Flags: ignoreversion
Source: "COMPLETE_BUILD\x86\*"; DestDir: "{app}"; Check: "not Is64BitInstallMode"; Flags: ignoreversion
Source: "COMPLETE_BUILD\x86\*"; DestDir: "{app}\x86"; Check: Is64BitInstallMode; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppSlogan}"
Name: "{group}\{cm:UninstallProgram,ளையும்{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\MetadataEditor"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\MetadataEditor"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\MetadataEditor"; ValueType: string; ValueName: "Author"; ValueData: "{#MyAppAuthor}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\MetadataEditor"; ValueType: string; ValueName: "MainExe"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\MetadataEditor"; ValueType: string; ValueName: "ShellExtensionDll"; ValueData: "{app}\{#MyShellDll}"; Flags: uninsdeletekey

; --- Shell Extension Handlers ---
Root: HKCR; Subkey: "*\shellex\PropertySheetHandlers\MetadataEditor"; ValueType: string; ValueData: "{{6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\shellex\PropertySheetHandlers\MetadataEditor"; ValueType: string; ValueData: "{{6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Drive\shellex\PropertySheetHandlers\MetadataEditor"; ValueType: string; ValueData: "{{6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Folder\shellex\PropertySheetHandlers\MetadataEditor"; ValueType: string; ValueData: "{{6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}}"; Flags: uninsdeletekey

; --- NEW: Context Menu Handler ---
Root: HKCR; Subkey: "*\shellex\ContextMenuHandlers\MetadataEditor"; ValueType: string; ValueData: "{{9A8B7C6D-5E4F-3D2C-1B0A-9F8E7D6C5B4A}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\shellex\ContextMenuHandlers\MetadataEditor"; ValueType: string; ValueData: "{{9A8B7C6D-5E4F-3D2C-1B0A-9F8E7D6C5B4A}}"; Flags: uninsdeletekey

; --- Approved Shell Extensions ---
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"; ValueType: string; ValueName: "{{6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C}}"; ValueData: "Metadata Editor Property Sheet Extension"; Flags: uninsdeletevalue
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"; ValueType: string; ValueName: "{{9A8B7C6D-5E4F-3D2C-1B0A-9F8E7D6C5B4A}}"; ValueData: "Metadata Editor Context Menu Extension"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure KillProcesses();
var ErrorCode: Integer;
begin
  Log('Killing processes and locking shell...');
  Exec('taskkill.exe', '/F /IM METADATA_EDITOR_APP.exe', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
  Exec('taskkill.exe', '/F /IM explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
  Exec('taskkill.exe', '/F /IM win32explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
  Sleep(500);
end;

procedure StartExplorer();
var ErrorCode: Integer;
begin
  Log('Restarting shell (unlocking)...');
  Exec(ExpandConstant('{win}\explorer.exe'), '', '', SW_SHOW, ewNoWait, ErrorCode);
  if FileExists('D:\Projects\Explorer++_FORK\Win32Explorer_1.4.X\Win32Explorer.exe') then
    Exec('D:\Projects\Explorer++_FORK\Win32Explorer_1.4.X\Win32Explorer.exe', '', '', SW_SHOW, ewNoWait, ErrorCode);
end;

procedure UnregisterShellExt(Path: String; Is64Bit: Boolean);
var ErrorCode: Integer; RegPath: String;
begin
  if not FileExists(Path) then Exit;
  if Is64Bit then RegPath := ExpandConstant('{dotnet4064}\regasm.exe') else RegPath := ExpandConstant('{dotnet4032}\regasm.exe');
  Exec(RegPath, ExpandConstant('/u /s "' + Path + '"'), '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
end;

procedure RegisterShellExt(Path: String; Is64Bit: Boolean);
var ErrorCode: Integer; RegPath: String;
begin
  if not FileExists(Path) then Exit;
  if Is64Bit then RegPath := ExpandConstant('{dotnet4064}\regasm.exe') else RegPath := ExpandConstant('{dotnet4032}\regasm.exe');
  Exec(RegPath, ExpandConstant('/codebase /s "' + Path + '"'), '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
end;

procedure CleanTestMode();
var Guid1, Guid2: String;
begin
  Guid1 := '{27416A28-B93F-4507-B64A-CDF2AEDA466A}';
  Guid2 := '{EA81E814-10BF-4905-A0C2-0B3D59EF7D03}';
  RegDeleteValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved', Guid1);
  RegDeleteValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved', Guid2);
end;

function GetUninstallString(): String;
var s: String;
begin
  Result := '';
  s := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{160160B0-3DEB-465F-995B-2ED3E2DD937C}_is1';
  if not RegQueryStringValue(HKLM, s, 'UninstallString', Result) then
    RegQueryStringValue(HKCU, s, 'UninstallString', Result);
end;

function InitializeSetup(): Boolean;
var UninstallString: String; ErrorCode: Integer;
begin
  Result := True;
  UninstallString := GetUninstallString();
  if UninstallString <> '' then
  begin
    if MsgBox('An existing version of MetadataEditor was detected. It must be uninstalled. Continue?', mbInformation, MB_YESNO) = IDYES then
    begin
      KillProcesses(); // Shell stays dead during uninstall -> install transition
      UninstallString := RemoveQuotes(UninstallString);
      Exec(UninstallString, '/VERYSILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
      // We DO NOT StartExplorer here to maintain the lock
    end else Result := False;
  end;
  if Result then CleanTestMode();
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then KillProcesses(); // Re-ensure lock if initialization didn't do it or if it was bypassed
  if CurStep = ssPostInstall then
  begin
    if Is64BitInstallMode then begin
      RegisterShellExt(ExpandConstant('{app}\{#MyShellDll}'), True);
      RegisterShellExt(ExpandConstant('{app}\x86\{#MyShellDll}'), False);
    end else RegisterShellExt(ExpandConstant('{app}\{#MyShellDll}'), False);
    StartExplorer(); // Unlock here
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var AppDir: String;
begin
  if CurUninstallStep = usUninstall then KillProcesses();
  if CurUninstallStep = usPostUninstall then
  begin
    AppDir := ExpandConstant('{app}');
    if DirExists(AppDir) then DelTree(AppDir, True, True, True);

    RegDeleteKeyIncludingSubkeys(HKEY_LOCAL_MACHINE, 'SOFTWARE\EliteSoftwareTech\MetadataEditor');
    StartExplorer();
  end;
end;
