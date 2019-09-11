@ECHO OFF

ECHO.
ECHO.     This build script requires the following software to be installed:
ECHO.       - Git command-line client
ECHO.       - Microsoft Visual Studio 2008 or newer
ECHO.       - Microsoft HTML Help compiler
ECHO.       - 7zip
ECHO.
ECHO.     You have to commit your work before using this script.
ECHO.     Results will be in the 'GIT_Build' directory.
ECHO.     Files in the 'GIT_Build' directory may be overwritten.
ECHO.
ECHO.

SET STUDIODIR=c:\Program Files (x86)\Microsoft Visual Studio 14.0
SET HHWDIR=c:\Program Files (x86)\HTML Help Workshop
SET SEVENZIPDIR=c:\Program Files\7-Zip

IF NOT DEFINED PLATFORM SET PLATFORM=x86

CALL "%STUDIODIR%\Common7\Tools\vsdevcmd.bat" %PLATFORM%
ECHO.
ECHO Building for platform %PLATFORM%
ECHO.

MKDIR "GIT_Build"

git.exe checkout "Source/Core/Properties/AssemblyInfo.cs" > NUL
git.exe checkout "Source/Plugins/BuilderModes/Properties/AssemblyInfo.cs" > NUL

ECHO.
ECHO Writing GIT log file...
ECHO.
IF EXIST "GIT_Build\Changelog.xml" DEL /F /Q "GIT_Build\Changelog.xml" > NUL
(
echo [OB]?xml version="1.0" encoding="UTF-8"?[CB]
echo [OB]log[CB]
git.exe log master --since=2012-04-17 --pretty=format:"[OB]logentry commit=\"%%h\"[CB]%%n[OB]author[CB]%%an[OB]/author[CB]%%n[OB]date[CB]%%aI[OB]/date[CB]%%n[OB]msg[CB]%%B[OB]/msg[CB]%%n[OB]/logentry[CB]"
echo [OB]/log[CB]
) >"GIT_Build\Changelog.xml"
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "GIT_Build\Changelog.xml" GOTO FILEFAIL

ECHO.
ECHO Compiling HTML Help file...
ECHO.
IF EXIST "Build\Refmanual.chm" DEL /F /Q "Build\Refmanual.chm" > NUL
"%HHWDIR%\hhc" Help\Refmanual.hhp
IF %ERRORLEVEL% NEQ 1 GOTO ERRORFAIL
IF NOT EXIST "Build\Refmanual.chm" GOTO FILEFAIL

ECHO.
ECHO Looking up current repository revision numbers...
ECHO.
IF EXIST "setenv.bat" DEL /F /Q "setenv.bat" > NUL
VersionFromGIT.exe "Source\Core\Properties\AssemblyInfo.cs" "Source\Plugins\BuilderModes\Properties\AssemblyInfo.cs" -O "setenv.bat"
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "setenv.bat" GOTO FILEFAIL

CALL "setenv.bat"
DEL /F /Q "setenv.bat"

VersionFromEXE.exe "Build\Updater.exe" "setenv.bat"
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "setenv.bat" GOTO FILEFAIL

CALL "setenv.bat"
DEL /F /Q "setenv.bat"

ECHO.
ECHO Cleaning solution...
ECHO.
msbuild.exe Builder.sln /t:Clean

ECHO.
ECHO Compiling GZDoom Builder core...
ECHO.
IF EXIST "Build\Builder.exe" DEL /F /Q "Build\Builder.exe" > NUL
IF EXIST "Source\Core\obj" RD /S /Q "Source\Core\obj"
msbuild "Source\Core\Builder.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Builder.exe" GOTO FILEFAIL

ECHO.
ECHO Compiling Automap Mode plugin...
ECHO.
IF EXIST "Build\Plugins\AutomapMode.dll" DEL /F /Q "Build\Plugins\AutomapMode.dll" > NUL
IF EXIST "Source\Plugins\AutomapMode\obj" RD /S /Q "Source\Plugins\AutomapMode\obj"
msbuild "Source\Plugins\AutomapMode\AutomapMode.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\AutomapMode.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Builder Effects plugin...
ECHO.
IF EXIST "Build\Plugins\BuilderEffects.dll" DEL /F /Q "Build\Plugins\BuilderEffects.dll" > NUL
IF EXIST "Source\Plugins\BuilderEffects\obj" RD /S /Q "Source\Plugins\BuilderEffects\obj"
msbuild "Source\Plugins\BuilderEffects\BuilderEffects.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\BuilderEffects.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Builder Modes plugin...
ECHO.
IF EXIST "Build\Plugins\BuilderModes.dll" DEL /F /Q "Build\Plugins\BuilderModes.dll" > NUL
IF EXIST "Source\Plugins\BuilderModes\obj" RD /S /Q "Source\Plugins\BuilderModes\obj"
msbuild "Source\Plugins\BuilderModes\BuilderModes.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\BuilderModes.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Color Picker plugin...
ECHO.
IF EXIST "Build\Plugins\ColorPicker.dll" DEL /F /Q "Build\Plugins\ColorPicker.dll" > NUL
IF EXIST "Source\Plugins\ColorPicker\obj" RD /S /Q "Source\Plugins\ColorPicker\obj"
msbuild "Source\Plugins\ColorPicker\ColorPicker.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\ColorPicker.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Comments Panel plugin...
ECHO.
IF EXIST "Build\Plugins\CommentsPanel.dll" DEL /F /Q "Build\Plugins\CommentsPanel.dll" > NUL
IF EXIST "Source\Plugins\CommentsPanel\obj" RD /S /Q "Source\Plugins\CommentsPanel\obj"
msbuild "Source\Plugins\CommentsPanel\CommentsPanel.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\CommentsPanel.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Nodes Viewer plugin...
ECHO.
IF EXIST "Build\Plugins\NodesViewer.dll" DEL /F /Q "Build\Plugins\NodesViewer.dll" > NUL
IF EXIST "Source\Plugins\NodesViewer\obj" RD /S /Q "Source\Plugins\NodesViewer\obj"
msbuild "Source\Plugins\NodesViewer\NodesViewer.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\NodesViewer.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Sound Propagation Mode plugin...
ECHO.
IF EXIST "Build\Plugins\SoundPropagationMode.dll" DEL /F /Q "Build\Plugins\SoundPropagationMode.dll" > NUL
IF EXIST "Source\Plugins\SoundPropagationMode\obj" RD /S /Q "Source\Plugins\SoundPropagationMode\obj"
msbuild "Source\Plugins\SoundPropagationMode\SoundPropagation.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\SoundPropagationMode.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Stair Sector Builder plugin...
ECHO.
IF EXIST "Build\Plugins\StairSectorBuilder.dll" DEL /F /Q "Build\Plugins\StairSectorBuilder.dll" > NUL
IF EXIST "Source\Plugins\StairSectorBuilder\obj" RD /S /Q "Source\Plugins\StairSectorBuilder\obj"
msbuild "Source\Plugins\StairSectorBuilder\StairSectorBuilder.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\StairSectorBuilder.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Tag Explorer plugin...
ECHO.
IF EXIST "Build\Plugins\TagExplorer.dll" DEL /F /Q "Build\Plugins\TagExplorer.dll" > NUL
IF EXIST "Source\Plugins\TagExplorer\obj" RD /S /Q "Source\Plugins\TagExplorer\obj"
msbuild "Source\Plugins\TagExplorer\TagExplorer.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\TagExplorer.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling Tag Range plugin...
ECHO.
IF EXIST "Build\Plugins\TagRange.dll" DEL /F /Q "Build\Plugins\TagRange.dll" > NUL
IF EXIST "Source\Plugins\TagRange\obj" RD /S /Q "Source\Plugins\TagRange\obj"
msbuild "Source\Plugins\TagRange\TagRange.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\TagRange.dll" GOTO FILEFAIL

ECHO.
ECHO Compiling vpo_dll...
ECHO.
IF EXIST "Source\Plugins\VisplaneExplorer\Resources\vpo.dll" DEL /F /Q "Source\Plugins\VisplaneExplorer\Resources\vpo.dll" > NUL
IF EXIST "Source\Plugins\vpo_dll\Release" RD /S /Q "Source\Plugins\vpo_dll\Release"
msbuild "Source\Plugins\vpo_dll\vpo_dll.vcxproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Source\Plugins\VisplaneExplorer\Resources\vpo.dll" GOTO FILEFAIL


ECHO.
ECHO Compiling Visplane Explorer plugin...
ECHO.
IF EXIST "Build\Plugins\VisplaneExplorer.dll" DEL /F /Q "Build\Plugins\VisplaneExplorer.dll" > NUL
IF EXIST "Source\Plugins\VisplaneExplorer\obj" RD /S /Q "Source\Plugins\VisplaneExplorer\obj"
msbuild "Source\Plugins\VisplaneExplorer\VisplaneExplorer.csproj" /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORM% /v:minimal
IF %ERRORLEVEL% NEQ 0 GOTO ERRORFAIL
IF NOT EXIST "Build\Plugins\VisplaneExplorer.dll" GOTO FILEFAIL

ECHO.
ECHO Creating changelog...
ECHO.
ChangelogMaker.exe "GIT_Build\Changelog.xml" "Build" "m-x-d>MaxED" %REVISIONNUMBER%
IF %ERRORLEVEL% NEQ 0 GOTO LOGFAIL

ECHO.
ECHO Packing release...
ECHO.
IF NOT "%PLATFORM%" == "x86" (SET DEL_PATHSPEC="GIT_Build\GZDoom_Builder*-%PLATFORM%.7z") ELSE (SET DEL_PATHSPEC="GIT_Build\GZDoom_Builder*.7z")
IF EXIST %DEL_PATHSPEC% DEL /F /Q %DEL_PATHSPEC% > NUL
IF EXIST "GIT_Build\GZDB_Updater-%PLATFORM%.7z" DEL /F /Q "GIT_Build\GZDB_Updater-%PLATFORM%.7z" > NUL
"%SEVENZIPDIR%\7z" a .\GIT_Build\gzdb.7z .\Build\* -xr!*.xml -xr!JetBrains.Profiler.Core.Api.dll -xr!ScintillaNET.3.5.pdb -x!Setup
"%SEVENZIPDIR%\7z" a .\GIT_Build\GZDB_Updater-%PLATFORM%.7z .\Build\Updater.exe .\Build\Updater.ini
IF %ERRORLEVEL% NEQ 0 GOTO PACKFAIL
IF NOT EXIST .\GIT_Build\gzdb.7z GOTO FILEFAIL
IF NOT EXIST .\GIT_Build\GZDB_Updater-%PLATFORM%.7z GOTO FILEFAIL

IF NOT "%PLATFORM%" == "x86" (REN "GIT_Build\gzdb.7z" GZDoom_Builder_Bugfix-r%REVISIONNUMBER%-%PLATFORM%.7z) ELSE (REN "GIT_Build\gzdb.7z" GZDoom_Builder_Bugfix-r%REVISIONNUMBER%.7z)

IF EXIST "Build\Changelog.txt" DEL /F /Q "Build\Changelog.txt" > NUL

@ECHO %REVISIONNUMBER%> .\GIT_Build\Version.txt
@ (ECHO %REVISIONNUMBER% && ECHO %EXEREVISIONNUMBER%) > .\GIT_Build\Versions.txt

git.exe checkout "Source\Core\Properties\AssemblyInfo.cs" > NUL
git.exe checkout "Source\Plugins\BuilderModes\Properties\AssemblyInfo.cs" > NUL

ECHO.
ECHO.     BUILD DONE !
ECHO.
ECHO.     Revision:  %REVISIONNUMBER% (%REVISIONHASH%)
ECHO.
PAUSE > NUL
GOTO LEAVE

:ERRORFAIL
ECHO.
ECHO.     BUILD FAILED (Tool returned error %ERRORLEVEL%)
ECHO.
PAUSE > NUL
GOTO LEAVE

:PACKFAIL
ECHO.
ECHO.     PACKAGING FAILED (7zip returned error %ERRORLEVEL%)
ECHO.
PAUSE > NUL
GOTO LEAVE

:FILEFAIL
ECHO.
ECHO.     BUILD FAILED (Output file was not built)
ECHO.
PAUSE > NUL
GOTO LEAVE

:LOGFAIL
ECHO.
ECHO.     CHANGELOG GENERATION FAILED (Tool returned error %ERRORLEVEL%)
ECHO.
PAUSE > NUL
GOTO LEAVE

:LEAVE
exit