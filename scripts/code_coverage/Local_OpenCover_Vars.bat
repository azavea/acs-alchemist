@echo off
REM Sets local partcover variables, and then calls the partcover command (assumed to be %1)

set OPENCOVER_PATH="C:\software\OpenCover\OpenCover.Console.exe"
set NUNIT="C:\Program Files\NUnit 2.5.2\bin\net-2.0\nunit-console-x86.exe"
set FILTER="+[Azavea.NijPredictivePolicing.*]* -[Azavea.NijPredictivePolicing.Test]*"
set OPENCOVER_XML="results.xml"
set DLLPATH="..\..\csharp\Azavea.NijPredictivePolicing.Test\bin\x86\Debug\Azavea.NijPredictivePolicing.Test.dll"

call %1