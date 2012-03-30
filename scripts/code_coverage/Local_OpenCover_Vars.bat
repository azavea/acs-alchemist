@echo off
REM Sets local partcover variables, and then calls the partcover command (assumed to be %1)

set OPENCOVER_PATH="C:\software\OpenCover\OpenCover.Console.exe"
set NUNIT="C:\Program Files\NUnit 2.5.2\bin\net-2.0\nunit-console.exe"
set FILTER="+[Azavea.HunchLab.*]* -[Azavea.HunchLab.Test]* -[Azavea.HunchLab.Instancer]*"
set OPENCOVER_XML="results.xml"
set DLLPATH="..\..\csharp\Azavea.HunchLab.Test\bin\Debug\Azavea.HunchLab.Test.dll"

call %1