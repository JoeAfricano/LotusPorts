; Lotus Ports installer (Inno Setup 6). Built by tools\Build-Installer.ps1 from the self-contained publish output.
; Per-user install, no administrator: %LOCALAPPDATA%\Programs\Lotus Ports.
; Player data (%LOCALAPPDATA%\LotusPorts: installed ports, settings, journal) is never touched, not even on uninstall.

#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\..\artifacts\dist\LotusPorts"
#endif
#ifndef NumericVersion
  #define NumericVersion "0.1.0.0"
#endif
#ifndef OutputDir
  #define OutputDir "..\..\artifacts\dist"
#endif

[Setup]
AppId={{4C2D8E61-93A7-4B5F-8E1D-6A0B7C3F2E94}
AppName=Lotus Ports
AppVersion={#AppVersion}
AppVerName=Lotus Ports {#AppVersion}
AppPublisher=Joeafricano
AppCopyright=Lotus Ports · feito por Joeafricano com o Claude
AppComments=Ports nativos de jogos clássicos para PC, num clique.
DefaultDirName={localappdata}\Programs\Lotus Ports
DisableDirPage=yes
DefaultGroupName=Lotus Ports
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir={#OutputDir}
OutputBaseFilename=LotusPorts-Setup-{#AppVersion}
SetupIconFile=..\..\src\LotusPorts.App\Assets\Art\Brand\app.ico
UninstallDisplayIcon={app}\LotusPorts.exe
UninstallDisplayName=Lotus Ports
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
WizardStyle=modern dark hidebevels
WizardBackColor=#120A10
WizardSizePercent=110
DisableWelcomePage=no
CloseApplications=force
RestartApplications=no
ShowLanguageDialog=auto
VersionInfoVersion={#NumericVersion}
VersionInfoProductVersion={#NumericVersion}
VersionInfoDescription=Instalador do Lotus Ports

[Languages]
Name: "pt"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
pt.DesktopIcon=Criar atalho na Área de Trabalho
en.DesktopIcon=Create a desktop shortcut
pt.Launch=Abrir o Lotus Ports agora
en.Launch=Open Lotus Ports now

[Messages]
pt.WelcomeLabel1=Bem-vindo ao Lotus Ports
en.WelcomeLabel1=Welcome to Lotus Ports
pt.WelcomeLabel2=Este assistente vai instalar o [name/ver] neste PC.%n%nPorts nativos de jogos clássicos, num clique. Nada é baixado nem alterado sem você confirmar, e o Lotus Ports nunca fornece arquivos de jogos.
en.WelcomeLabel2=This will install [name/ver] on this PC.%n%nNative ports of classic games, in one click. Nothing is downloaded or changed without your confirmation, and Lotus Ports never provides game files.
pt.FinishedHeadingLabel=Tudo pronto. Bom jogo!
en.FinishedHeadingLabel=All set. Have a great game!

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIcon}"; Flags: checkedonce

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{userprograms}\Lotus Ports"; Filename: "{app}\LotusPorts.exe"; WorkingDir: "{app}"
Name: "{userdesktop}\Lotus Ports"; Filename: "{app}\LotusPorts.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\LotusPorts.exe"; Description: "{cm:Launch}"; Flags: nowait postinstall skipifsilent
; Update started from inside Lotus Ports (/relaunch=1): open it again once the files are in place.
Filename: "{app}\LotusPorts.exe"; Flags: nowait; Check: RelaunchAfterUpdate

[UninstallRun]
; The lotusports:// links and the integration entry that Release builds write for Lotus (HKCU only).
Filename: "{sys}\reg.exe"; Parameters: "delete HKCU\Software\Classes\lotusports /f"; Flags: runhidden; RunOnceId: "LotusPortsLinks"
Filename: "{sys}\reg.exe"; Parameters: "delete HKCU\Software\LotusPorts\Integration /f"; Flags: runhidden; RunOnceId: "LotusPortsIntegration"

[Code]
function RelaunchAfterUpdate(): Boolean;
begin
  Result := WizardSilent() and (ExpandConstant('{param:relaunch|0}') = '1');
end;

procedure CloseLotusPorts();
var Code: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/IM LotusPorts.exe /F', '', SW_HIDE, ewWaitUntilTerminated, Code);
  Sleep(600);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  CloseLotusPorts();
  Result := '';
end;

function InitializeUninstall(): Boolean;
begin
  CloseLotusPorts();
  Result := True;
end;
