@ECHO OFF

ECHO.
ECHO.     This build script requires the following software to be installed:
ECHO.       - Microsoft Visual Studio 2008 or newer
ECHO.

SET STUDIODIR=c:\Program Files (x86)\Microsoft Visual Studio 9.0

CALL "%STUDIODIR%\Common7\Tools\vsvars32.bat"

DEL /F /Q "Build\Plugins\*.pdb" > NUL

ECHO.
ECHO Compiling GZDoom Builder core...
ECHO.
IF EXIST "Build\Builder.exe" DEL /F /Q "Build\Builder.exe" > NUL
IF EXIST "Source\Core\obj" RD /S /Q "Source\Core\obj"
msbuild "Source\Core\Builder.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Builder.exe" GOTO FILEFAIL

ECHO.
ECHO Compiling Automap Mode plugin...
ECHO.
IF EXIST "Build\Plugins\AutomapMode.dll" DEL /F /Q "Build\Plugins\AutomapMode.dll" > NUL
IF EXIST "Source\Plugins\AutomapMode\obj" RD /S /Q "Source\Plugins\AutomapMode\obj"
msbuild "Source\Plugins\AutomapMode\AutomapMode.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\AutomapMode.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Builder Effects plugin...
ECHO.
IF EXIST "Build\Plugins\BuilderEffects.dll" DEL /F /Q "Build\Plugins\BuilderEffects.dll" > NUL
IF EXIST "Source\Plugins\BuilderEffects\obj" RD /S /Q "Source\Plugins\BuilderEffects\obj"
msbuild "Source\Plugins\BuilderEffects\BuilderEffects.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\BuilderEffects.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Builder Modes plugin...
ECHO.
IF EXIST "Build\Plugins\BuilderModes.dll" DEL /F /Q "Build\Plugins\BuilderModes.dll" > NUL
IF EXIST "Source\Plugins\BuilderModes\obj" RD /S /Q "Source\Plugins\BuilderModes\obj"
msbuild "Source\Plugins\BuilderModes\BuilderModes.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\BuilderModes.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Color Picker plugin...
ECHO.
IF EXIST "Build\Plugins\ColorPicker.dll" DEL /F /Q "Build\Plugins\ColorPicker.dll" > NUL
IF EXIST "Source\Plugins\ColorPicker\obj" RD /S /Q "Source\Plugins\ColorPicker\obj"
msbuild "Source\Plugins\ColorPicker\ColorPicker.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\ColorPicker.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Comments Panel plugin...
ECHO.
IF EXIST "Build\Plugins\CommentsPanel.dll" DEL /F /Q "Build\Plugins\CommentsPanel.dll" > NUL
IF EXIST "Source\Plugins\CommentsPanel\obj" RD /S /Q "Source\Plugins\CommentsPanel\obj"
msbuild "Source\Plugins\CommentsPanel\CommentsPanel.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\CommentsPanel.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Nodes Viewer plugin...
ECHO.
IF EXIST "Build\Plugins\NodesViewer.dll" DEL /F /Q "Build\Plugins\NodesViewer.dll" > NUL
IF EXIST "Source\Plugins\NodesViewer\obj" RD /S /Q "Source\Plugins\NodesViewer\obj"
msbuild "Source\Plugins\NodesViewer\NodesViewer.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\NodesViewer.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Sound Propagation Mode plugin...
ECHO.
IF EXIST "Build\Plugins\SoundPropagationMode.dll" DEL /F /Q "Build\Plugins\SoundPropagationMode.dll" > NUL
IF EXIST "Source\Plugins\SoundPropagationMode\obj" RD /S /Q "Source\Plugins\SoundPropagationMode\obj"
msbuild "Source\Plugins\SoundPropagationMode\SoundPropagation.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\SoundPropagationMode.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Stair Sector Builder plugin...
ECHO.
IF EXIST "Build\Plugins\StairSectorBuilder.dll" DEL /F /Q "Build\Plugins\StairSectorBuilder.dll" > NUL
IF EXIST "Source\Plugins\StairSectorBuilder\obj" RD /S /Q "Source\Plugins\StairSectorBuilder\obj"
msbuild "Source\Plugins\StairSectorBuilder\StairSectorBuilder.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\StairSectorBuilder.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Tag Explorer plugin...
ECHO.
IF EXIST "Build\Plugins\TagExplorer.dll" DEL /F /Q "Build\Plugins\TagExplorer.dll" > NUL
IF EXIST "Source\Plugins\TagExplorer\obj" RD /S /Q "Source\Plugins\TagExplorer\obj"
msbuild "Source\Plugins\TagExplorer\TagExplorer.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\TagExplorer.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Tag Range plugin...
ECHO.
IF EXIST "Build\Plugins\TagRange.dll" DEL /F /Q "Build\Plugins\TagRange.dll" > NUL
IF EXIST "Source\Plugins\TagRange\obj" RD /S /Q "Source\Plugins\TagRange\obj"
msbuild "Source\Plugins\TagRange\TagRange.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\TagRange.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Visplane Explorer plugin...
ECHO.
IF EXIST "Build\Plugins\VisplaneExplorer.dll" DEL /F /Q "Build\Plugins\VisplaneExplorer.dll" > NUL
IF EXIST "Source\Plugins\VisplaneExplorer\obj" RD /S /Q "Source\Plugins\VisplaneExplorer\obj"
msbuild "Source\Plugins\VisplaneExplorer\VisplaneExplorer.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\VisplaneExplorer.dll" GOTO FILEFAIL

ECHO.
ECHO.     BUILD DONE !
ECHO.
PAUSE > NUL
GOTO LEAVE

:ERRORFAIL
ECHO.
ECHO.     BUILD FAILED (Tool returned error %ERRORLEVEL%)
ECHO.
PAUSE > NUL
GOTO LEAVE

:FILEFAIL
ECHO.
ECHO.     BUILD FAILED (Output file was not built)
ECHO.
PAUSE > NUL
GOTO LEAVE

:LEAVE
