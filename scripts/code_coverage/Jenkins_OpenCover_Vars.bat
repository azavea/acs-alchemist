@echo off
REM Sets Jenkins partcover variables, and then calls the partcover command (assumed to be %1)

set OPENCOVER_PATH="C:\software\OpenCover\OpenCover.Console.exe"
set NUNIT=%nunit%
set FILTER="+[Azavea.HunchLab.*]* -[Azavea.HunchLab.Test]* -[Azavea.HunchLab.Instancer]*"
set OPENCOVER_XML="results.xml"
set DLLPATH="..\..\csharp\Azavea.HunchLab.Test\bin\Release\Azavea.HunchLab.Test.dll"

call %1