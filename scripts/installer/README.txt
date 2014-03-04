1.) Switch to 'Debug' mode, with CPU type set to 'x86' (Important!)
2.) Manually delete the data folder if you'be been debugging:
C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\bin\x86\Debug\Data

3.) Rebuild
4.) Install the NSIS plugins in the lib/nsis folder if you haven't already (see http://nsis.sourceforge.net/How_can_I_install_a_plugin for instructions)
4.) compile installer
5.) install on non-developer machine 32bit, and 64 bit
6.) test basic download, import (this will test spatialite, which is the most nitpicky)