REM @echo off

REM Jenkins defines the %WORKSPACE% variable, so we use that to test to see if we should use the jenkins config
IF (%WORKSPACE%)==() (
	call Local_OpenCover_Vars.bat OpenCover_Cmd.bat
) ELSE (
	call Jenkins_OpenCover_Vars.bat OpenCover_Cmd.bat
)

