echo off
@set ACSI="C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\bin\x86\Debug\AcsDataImporter.exe"
@set JOBS="C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Jobs"

@cd C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\bin\x86\Debug\

@echo "Block Groups:"
@echo "ralphvars1"
call %ACSI% %JOBS%\job_ralphBG1.txt
@echo "ralphvars2"
call %ACSI% %JOBS%\job_ralphBG2.txt
@echo "ralphvars3"
call %ACSI% %JOBS%\job_ralphBG3.txt

@echo "Grids:"
@echo "ralphvars1"
call %ACSI% %JOBS%\job_ralphGrid1.txt
@echo "ralphvars2"
call %ACSI% %JOBS%\job_ralphGrid2.txt
@echo "ralphvars3"
call %ACSI% %JOBS%\job_ralphGrid3.txt