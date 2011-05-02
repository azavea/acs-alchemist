@set CDM="C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.CrimeDataMerger\bin\x86\Debug\CrimeDataMerger.exe"

cd C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.CrimeDataMerger\bin\x86\Debug\

echo "bg 2007"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge1.txt
echo "bg 2008\"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge2.txt
echo "bg 2009"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge3.txt
echo "bg 2010"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge3a.txt

echo "cells 2007"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge4.txt
echo "cells 2008"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge5.txt
echo "cells 2009"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge6.txt
echo "cells 2010"
%CDM% C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\philly_merge7.txt