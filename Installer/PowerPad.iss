[Setup]
AppName=Mi Aplicacion
AppVersion=1.0.0
DefaultDirName={pf}\MiAplicacion
DefaultGroupName=Mi Aplicacion
OutputBaseFilename=MiAplicacion-Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
; Ruta relativa desde donde está el archivo ISS hasta tu carpeta publish
Source: "../TuProyectoWinUI3/publish/*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Mi Aplicacion"; Filename: "{app}\TuProyectoWinUI3.exe"
Name: "{userdesktop}\Mi Aplicacion"; Filename: "{app}\TuProyectoWinUI3.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\TuProyectoWinUI3.exe"; Description: "{cm:LaunchProgram,Mi Aplicacion}"; Flags: nowait postinstall skipifsilent