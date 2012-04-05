@echo on
REM Meant to be called by someone who sets these vars for us.  See Local_PartCover_Vars.bat for an example

%OPENCOVER_PATH% -filter:%FILTER% -target:%NUNIT% -register:user -targetargs:%DLLPATH% -output:%OPENCOVER_XML%

REM %PARTCOVER_PATH%\partcover.exe --target %NUNIT% --target-args "..\..\csharp\Azavea.HunchLab.Test\bin\x86\Debug\Azavea.HunchLab.Test.dll" --include %INCLUDE% --exclude [Azavea.HunchLab.Test]* --exclude [Azavea.HunchLab.Instancer]* --output %PARTCOVER_XML%