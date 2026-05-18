; Скрипт для создания установщика SoundProject
     #define MyAppName "SoundProject"
     #define MyAppVersion "1.0"
     #define MyAppPublisher "SoundProject Team"
     #define MyAppExeName "project.exe"
     ; УКАЖИТЕ НИЖЕ ПУТЬ К ВАШЕЙ ПАПКЕ PUBLISH
     #define MyPublishPath
      "C:\Users\Slabu\Desktop\OOP-course_project\project\project\bin\Release\net10.0-windows\win-x64\publish"
    
     [Setup]
    AppId={{SoundProject-OOP-Course-Project}}
    AppName={#MyAppName}
    AppVersion={#MyAppVersion}
    AppPublisher={#MyAppPublisher}
   DefaultDirName={autopf}\{#MyAppName}
    DefaultGroupName={#MyAppName}
    AllowNoIcons=yes
    OutputDir={userdesktop}
    OutputBaseFilename=SoundProject_Setup
    Compression=lzma
    SolidCompression=yes
    WizardStyle=modern
   
    [Languages]
   Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
    Name: "english"; MessagesFile: "compiler:Default.isl"
   
    [Tasks]
    Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags:
      unchecked
   
    [Files]
    ; Копируем сам экзешник
    Source: "{#MyPublishPath}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
    ; Копируем ВСЕ остальные файлы из папки publish (библиотеки, базу данных, картинки)
    Source: "{#MyPublishPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
   
    [Icons]
    Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
    Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
   
    [Run]
    Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags:
      nowait postinstall skipifsilent