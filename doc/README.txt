-------------------------------------------------------------------------------
ACS Alchemist Readme
-------------------------------------------------------------------------------

Copyright 2011-2012 Azavea Inc.


Introduction
-------------------------------------
The ACS Alchemist is a tool that can help you extract specific portions of the
American Community Survey (ACS).  Specifically this version supports:
 * ACS 2005-2009 (released in December 2010)
 * ACS 2006-2010 (released in December 2011)

This tool was developed by Azavea in collaboration with Jerry Ratcliffe and Ralph Taylor of Temple University and partially funded by a Predictive Policing grant from the National Institute of Justice (Award # 2010-DE-BX-K004).  The source code is released under a GPLv3 license.


Getting Started
-------------------------------------
To install the ACS Alchemist, run the ACSAlchemist.exe file, and select an installation folder.  You can launch the application from the Start Menu as follows:

1. Click on [Start]->[Programs]->[Azavea]->[ACS Alchemist]


The importer will download files to an output directory so you will need to have write permissions for this directory. If you receive permission errors when attempting to run the application in Windows Vista or Windows 7, please try the following steps:

On Windows 7 or Windows Vista:

1. Right->Click on [Start]->[Programs]->[Azavea]->[ACS Alchemist]
2. Select Properties -> Advanced -> Run As Administrator
1. Click on [Start]->[Programs]->[Acs Importer]->[Acs Importer]

Getting help
-------------------------------------
If you need a list of all available commands, just enter the program name without any arguments.

C:\...> ACSAlchemist.exe



Getting the data you want - State and Year
-------------------------------------
The American Community Survey has more than 20,000 variables for 50 states, District of Columbia and Puerto Rico and for multiple geographic levels.  This tool is designed to enable you to extract up to 100 variables for a single geographic level and a single state in each run.  The first step is to decide what data you want to extract.  And the first decision is to pick a state and an ACS year.

This version of the application supports two years of ACS data:
 * ACS 2005-2009 (released in December 2010)
 * ACS 2006-2010 (released in December 2011).


1. To see what states are available, use the following command.

C:\...> ACSAlchemist.exe -listStateCodes
    
2. To see what years are available, use the following command.

C:\...> ACSAlchemist.exe -listYears
    
3. When you know which state and year you'd like to use, you can pre-download the unfiltered state data files by using:

C:\...> ACSAlchemist.exe -s Wyoming –y 2009


Getting the data you want - Summary Level
-------------------------------------------
Variable summaries are grouped into multiple geographic levels aggregation - tracts, blockgroups, etc. The Data Ermine requires a summary level specified by code.  These can be listed by typing: 

C:\...> ACSAlchemist.exe -listSummaryLevels


Getting the data you want - Variables
-------------------------------------------
Each data point for the ACS is represented by a unique variable ID.  These are available at: 
 * http://www2.census.gov/acs2009_5yr/summaryfile/ACS2009_5-Year_TableShells.xls
 * http://www2.census.gov/acs2010_5yr/summaryfile/ACS2010_5-Year_TableShells.xls

These files are also copied to the \docs\ directory with in the Data Ermine installation.  From this file, select up to 100 Unique ID's per job, and save these variable names to a “variable file” in text format.  This file is used to determine the variables that should be included in your output. Each line corresponds to a variable and consists of two parts separated by a comma.  The first is an ID from the "Table ID" column in the ACS Excel file, the second is an optional name for the column in the output shapefile (if no name is specified, the ID will be used instead).  Note that column names are truncated to 10 characters due to limitations of the DBF file used in the shapefile format.  In addition, you should avoid using spaces or special characters (other than ~,-, and _) If you specify column names that will result in duplicates, the Data Ermine will throw an error.  An example of the variables file would be: 

B01001001,TOTALPOP
B01001002,TOTALMALE
B01001026,TOTALFEMALE

Assuming this is saved in myVariables.txt in your working directory, you can specify this variable file with the following command: 

C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt


Specify an output directory
-------------------------------------
By default, the Data Ermine will use the “AppData” directory in your Windows user directory.  For example, on Windows 7, this might be something like: C:\Users\YourUserID\AppData\Local\ACSImporter\Data.  If you want to change this default, you can specify an output directory using the “-outputFolder” switch as follows:

-outputFolder c:\Sandbox\ACS


Naming your Job
-------------------------------------
Each run of the Data Ermine is called a “job”.  The software will create a job name based on the state, the year and the date, unless you specify a job name.

-jobName DesiredNameHere


Putting it all together
-------------------------------------
Here is an example import using what was discussed so far:
  
C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt -jobName Test01 -outputFolder c:\Sandbox\ACS

This command will make sure all necessary files are downloaded for Wyoming and import the selected variables for 2009 into an internal database (using SQLLite) named "Test01".  The variable values were at the blockgroup summary level.  From here you can also generate your results as a shapefile using either the summary geography or using a 'fishnet' grid of polygon cells.


Exporting to a Shapefile
-------------------------------------
When you're ready to run your full export, you can convert the data to a Shapefile.  "-exportToShape" will cause your data to be saved to a shapefile in the given output projection (see below).	
  
C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt -jobName Test01 -exportToShape
C:\...> dir Test01.*

Test01.dbf
Test01.prj
Test01.shp
Test01.shx


Setting an Output Projection
-------------------------------------
The geographic data associated with the ACS variables (tract or blockroup boundaries) are downloaded from the Census Bureau in GRS80NAD83 decimal degrees (EPSG:4269).  If you wish to specify a different projection, you can either add an EPSG spatial reference ID (SRID) or specify a filename containing the WKT of the desired projection.  When a projection is listed, the Data Ermine will re-project when generating the shapefile.  The Data Ermine uses the PROJ.4 and ProjNET libraries and the supported SRIDs are listed in the “SRID.csv” file in the installation directory.  Here’s an example that uses the PRJ file approach:

C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt -jobName Test01 -exportToShape -outputProjection myproj.prj


Saving Job Files
-------------------------------------
The Data Ermine has a lot of options (there are more listed in the UserManual.pdf), it is easy to make mistakes with the command line flags.  We have therefore provided a mechanism for saving the variables as a “job file”.  A job file contains all the arguments you would normally specify on the command line in a text file. The syntax is simple:

 - Blank lines and lines that start with “#”, are ignored
 - All other lines should be a command line flag starting with -, followed by an argument (if required for the particular flag)

In order to use a job file, simply specify its path as the only argument to the Data Ermine. Note that file paths specified in the job file should be relative to the directory the importer will be run in, not to the location of the job file.  An example job file named "myJob.txt" for the previous command would look something like this: 

# myJob.txt - gets some data about wyoming and puts it in a shapefile
# specify Wyoming as the state
-s Wyoming
# specify year
–y 2009
# extract blockgroups
-e 150
# extract the variables in myVariablesFile.txt
-v myVariablesFile.txt
# save the data to a directory
-outputFolder C:\sandbox\ACS\
# use "Test01" as a job name
-jobName Test01
# export results to a shapefile
-exportToShape
# convert the output to projection specified in myproj.prj rather than using WGS84
-outputProjection myproj.prj

And could be run using this command:

C:\...> ACSAlchemist.exe myJob.txt


Frequently Asked Questions
-------------------------------------

Send us questions!  acs-data-ermine@azavea.com


Open Source Libraries Used
-------------------------------------

Special thanks to several open source libraries that made this project possible. Please see the /licenses/ folder for the licenses for each respective project, and see below for links to each project:

 * ExcelBinaryReader - http://exceldatareader.codeplex.com/
 * GeoApi - http://geoapi.codeplex.com/
 * GEOS - http://trac.osgeo.org/geos/
 * log4net - http://logging.apache.org/log4net/
 * NetTopologySuite - http://code.google.com/p/nettopologysuite/
 * PowerCollections - http://powercollections.codeplex.com/
 * ProjNET - http://projnet.codeplex.com/
 * Proj.4 - http://trac.osgeo.org/proj/
 * Ionic.Zip - http://dotnetzip.codeplex.com/
 * Newtonsoft.Json - http://james.newtonking.com/pages/json-net.aspx
 * System.Data.SQLite - http://system.data.sqlite.org
 * Spatialite - http://www.gaia-gis.it/spatialite/
 * Sqlite - http://www.sqlite.org/
