#define MyAppName "IconExplorer"
#ifndef MyAppVersion
#define MyAppVersion "1.3.3.0"
#endif
#define MyAppPublisher "EliteSoftwareTech Co."
#define MyAppAuthor "Zachary Whiteman"
#define MyAppCopyright "© 2026 EliteSoftwareTech Co. (April 2026)"
#define MyAppSlogan "Bringing 2006 to 2026 one line of code at a time."
#define MyAppExeName "ICON_EXPLORER_APP.exe"
#define MyShellDll "ICON_EXPLORER_SHELL_EXTENSION.dll"

; --- BRANDING ASSETS ---
#define MyLogo "LOGO_FOR_ELITE_SOFTWARE.png"

[Setup]
AppId={{912B5A0C-6C1D-4E7F-B21A-1C4F8D9E3A5B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppCopyright={#MyAppCopyright}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=COMPLETE_BUILD_INSTALLER
OutputBaseFilename=IconExplorer_Setup_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
SetupIconFile=ICON_FOR_ICON_EXPLORER.ico

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
Source: "COMPLETE_BUILD\ICON_EXPLORER_APP.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "COMPLETE_BUILD\ICON_EXPLORER_ENGINE.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "COMPLETE_BUILD\x64\*"; DestDir: "{app}"; Check: Is64BitInstallMode; Flags: ignoreversion
Source: "COMPLETE_BUILD\x86\*"; DestDir: "{app}"; Check: "not Is64BitInstallMode"; Flags: ignoreversion
Source: "COMPLETE_BUILD\x86\*"; DestDir: "{app}\x86"; Check: Is64BitInstallMode; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppSlogan}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\IconExplorer"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\IconExplorer"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\IconExplorer"; ValueType: string; ValueName: "Author"; ValueData: "{#MyAppAuthor}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\IconExplorer"; ValueType: string; ValueName: "MainExe"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\EliteSoftwareTech\IconExplorer"; ValueType: string; ValueName: "ShellExtensionDll"; ValueData: "{app}\{#MyShellDll}"; Flags: uninsdeletekey

; --- Shell Extension Handlers ---
Root: HKCR; Subkey: "*\shellex\PropertySheetHandlers\IconExplorer"; ValueType: string; ValueData: "{{7E1A63DF-014F-4C2E-A528-9DB33A4E7A68}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\shellex\PropertySheetHandlers\IconExplorer"; ValueType: string; ValueData: "{{7E1A63DF-014F-4C2E-A528-9DB33A4E7A68}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Drive\shellex\PropertySheetHandlers\IconExplorer"; ValueType: string; ValueData: "{{7E1A63DF-014F-4C2E-A528-9DB33A4E7A68}}"; Flags: uninsdeletekey

; --- NEW: Context Menu Handler ---
Root: HKCR; Subkey: "*\shellex\ContextMenuHandlers\IconExplorer"; ValueType: string; ValueData: "{{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\shellex\ContextMenuHandlers\IconExplorer"; ValueType: string; ValueData: "{{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}}"; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure KillProcesses();
var ErrorCode: Integer;
begin
  Log('Killing processes...');
  Exec('taskkill.exe', '/F /IM ICON_EXPLORER_APP.exe', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
  Exec('taskkill.exe', '/F /IM explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
  Exec('taskkill.exe', '/F /IM win32explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
  Sleep(500);
end;

procedure StartExplorer();
var ErrorCode: Integer;
begin
  Log('Restarting explorer.exe...');
  Exec(ExpandConstant('{win}\explorer.exe'), '', '', SW_SHOW, ewNoWait, ErrorCode);
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
  Guid1 := '{7E1A63DF-014F-4C2E-A528-9DB33A4E7A68}';
  Guid2 := '{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}';
  RegDeleteValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved', Guid1);
  RegDeleteValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved', Guid2);
end;

function GetUninstallString(): String;
var s: String;
begin
  Result := '';
  s := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{912B5A0C-6C1D-4E7F-B21A-1C4F8D9E3A5B}_is1';
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
    if MsgBox('An existing version of IconExplorer was detected. It must be uninstalled. Continue?', mbInformation, MB_YESNO) = IDYES then
    begin
      KillProcesses();
      UninstallString := RemoveQuotes(UninstallString);
      Exec(UninstallString, '/VERYSILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
      StartExplorer();
    end else Result := False;
  end;
  if Result then CleanTestMode();
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then KillProcesses();
  if CurStep = ssPostInstall then
  begin
    if Is64BitInstallMode then begin
      RegisterShellExt(ExpandConstant('{app}\{#MyShellDll}'), True);
      RegisterShellExt(ExpandConstant('{app}\x86\{#MyShellDll}'), False);
    end else RegisterShellExt(ExpandConstant('{app}\{#MyShellDll}'), False);
    StartExplorer();
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
    
    // Purge old IconExtractor folder if it exists
    AppDir := ExpandConstant('{autopf}\IconExtractor');
    if DirExists(AppDir) then DelTree(AppDir, True, True, True);

    RegDeleteKeyIncludingSubkeys(HKEY_LOCAL_MACHINE, 'SOFTWARE\EliteSoftwareTech\IconExplorer');
    StartExplorer();
  end;
end;
