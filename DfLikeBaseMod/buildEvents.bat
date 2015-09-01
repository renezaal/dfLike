SETLOCAL ENABLEEXTENSIONS
SET vModsFolder="%~2\Debug\Mods"
MKDIR %vModsFolder%
ROBOCOPY "%~1" %vModsFolder% *.dll /E
IF %ERRORLEVEL% GEQ 8 GOTO failed


SET vModsFolder="%~2\Release\Mods"
MKDIR %vModsFolder%
ROBOCOPY "%~1" %vModsFolder% *.dll /E
IF %ERRORLEVEL% GEQ 8 GOTO failed

:: end of batch file
GOTO success

:failed
:: do not pause as it will pause msbuild.
EXIT

:success
EXIT 0