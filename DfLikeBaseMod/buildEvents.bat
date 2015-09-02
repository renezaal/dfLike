:: Copies the mod library and its resources to the correct place relative to the main executable
SETLOCAL ENABLEEXTENSIONS

:: Name the variables
SET vTargetFolder="%~dp1\"
SET vMainExeBinFolder=%~dp2
SET vProjectName=%~3

:: Get the folder
SET vModFolder="%vMainExeBinFolder%Debug\Mods\%vProjectName%"
MKDIR %vModFolder%
:: Copy the contents of the target folder to the target folder of the main executable (Debug)
ROBOCOPY %vTargetFolder% %vModFolder% /MIR
IF %ERRORLEVEL% GEQ 8 GOTO failed



:: Get the folder
SET vModFolder="%vMainExeBinFolder%Release\Mods\%vProjectName%"
MKDIR %vModFolder%
:: Copy the contents of the target folder to the target folder of the main executable (Release)
ROBOCOPY %vTargetFolder% %vModFolder% /MIR
IF %ERRORLEVEL% GEQ 8 GOTO failed

:: end of batch file
GOTO success

:failed
:: do not pause as it will pause msbuild.
ENDLOCAL
EXIT

:success
ENDLOCAL
EXIT 0