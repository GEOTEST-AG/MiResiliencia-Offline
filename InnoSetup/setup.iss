; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "MiResiliencia Honduras"
#define MyAppVersion "0.3.1"
#define MyAppPublisher "GEOTEST AG"
#define MyAppURL "https://www.resilience-toolbox.org"
#define MyAppExeName "ResTBDesktop.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{944657E9-B7F5-4970-916B-FC798BAADE13}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=..\InnoSetup\Output
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

;[Files]
;Source: "..\GUI\VCREDIST\VC_redist.x86.exe"; DestDir: {tmp}; Flags: deleteafterinstall
;[Run]
;Filename: "{tmp}\VC_redist.x86.exe"; StatusMsg: Installing Microsoft Visual C++ 2015-2019 Redistributable (x86) - 14.27.29112

[Files]
Source: "..\GUI\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion  recursesubdirs createallsubdirs
Source: "..\GUI\bin\Release\ResTBDesktop.exe.config"; DestDir: "{app}"; Flags: ignoreversion  recursesubdirs createallsubdirs; AfterInstall: ChangeAppSettings();
Source: "C:\dev\MapWinGIS\*"; DestDir: "{app}\MapWinGIS"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\dev\MapWinGIS\MapWinGIS.ocx"; DestDir: "{app}\MapWinGIS"; Flags: restartreplace sharedfile regserver
Source: "..\PrintTemplates\*.pdf"; DestDir: "{code:GetAppData}\ResTBDesktop\PrintTemplates\";  Flags: recursesubdirs createallsubdirs
Source: "..\Kernel\Scripts\*.min.js"; DestDir: "{code:GetAppData}\ResTBDesktop\Script\";  Flags: recursesubdirs createallsubdirs
Source: "..\Kernel\Content\*.min.css"; DestDir: "{code:GetAppData}\ResTBDesktop\Script\";  Flags: recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


[Code]

const
  ConfigEndpointPath = '//add[@key="UseOfflineDB"]';

var
  AppDataPath: string;

function GetAppData(Param: string): string;
begin
  Result := AppDataPath;
end;  

function SaveValueToXML(AFileName: string): Boolean;
var
  XMLNode: Variant;
  XMLDocument: Variant;  
begin
  XMLDocument := CreateOleObject('Msxml2.DOMDocument.6.0');
  try
    XMLDocument.async := False;
    XMLDocument.load(AFileName);
    if (XMLDocument.parseError.errorCode <> 0) then
      MsgBox('The XML file could not be parsed. ' + 
        XMLDocument.parseError.reason, mbError, MB_OK)
    else
    begin
      XMLDocument.setProperty('SelectionLanguage', 'XPath');  
      XMLNode := XMLDocument.selectSingleNode(ConfigEndpointPath);
      XMLNode.setAttribute('value', 'false');
      XMLDocument.save(AFileName);
      Result := True;
    end;
  except
    MsgBox('An error occured!' + #13#10 + GetExceptionMessage, mbError, MB_OK); 
  Result := False;
  end;
end;


function InitializeSetup(): Boolean;
var
  Uniq: string;
  TempFileName: string;
  Cmd: string;
  Params: string;
  ResultCode: Integer;
  Buf: AnsiString;
begin
  AppDataPath := ExpandConstant('{userappdata}');
  Log(Format('Default/Fallback application data path is %s', [AppDataPath]));
  Uniq := ExtractFileName(ExpandConstant('{tmp}'));
  TempFileName :=
    ExpandConstant(Format('{commondocs}\appdata-%s.txt', [Uniq]));
  Params := Format('/C echo %%APPDATA%% > %s', [TempFileName]);
  Log(Format('Resolving APPDATA using %s', [Params]));
  Cmd := ExpandConstant('{cmd}');
  if ExecAsOriginalUser(Cmd, Params, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and
     (ResultCode = 0) then
  begin
    if LoadStringFromFile(TempFileName, Buf) then
    begin
      AppDataPath := Trim(Buf);
      Log(Format('APPDATA resolved to %s', [AppDataPath]));
    end
      else
    begin
      Log(Format('Error reading %s', [TempFileName]));
    end;
    DeleteFile(TempFileName);
  end
    else
  begin
    Log(Format('Error %d resolving APPDATA', [ResultCode]));
  end;



  Result := True;
end;


procedure ChangeAppSettings();
begin        
  SaveValueToXML(ExpandConstant('{app}\ResTBDesktop.exe.config'));
end;
