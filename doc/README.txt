-------------------------------------------------------------------------------
ACS Alchemist Readme
-------------------------------------------------------------------------------

Copyright (c) 2011-2012 Azavea Inc.


Introduction
-------------------------------------
The ACS Alchemist is a tool that can help you extract specific portions of the American Community Survey (ACS).  Specifically this version supports:
 * ACS 2005-2009 (released in December 2010)
 * ACS 2006-2010 (released in December 2011)


About
-------------------------------------
This tool was developed by Azavea in collaboration with Jerry Ratcliffe and Ralph Taylor of Temple University Center for Security and Crime Science.

This project was supported by Award No. 2010-DE-BX-K004, awarded by the National Institute of Justice, Office of Justice Programs, U.S. Department of Justice. The opinions, findings, and conclusions or recommendations expressed in this software are those of the author(s) and do not necessarily reflect those of the Department of Justice. The source code is released under a GPLv3 license and is available at:  http://github.com/azavea/ACSAlchemist/


Disclaimer / License
-------------------------------------
This program comes with ABSOLUTELY NO WARRANTY; This is free software, and you are welcome to redistribute it under the terms of the GNU General Public License.


Getting Started
-------------------------------------
To install the ACS Alchemist, run the ACSAlchemist.exe installer, and select an installation folder.  You can launch the application from the Start Menu as follows:

The installer provides both a command line application, as well as a more friendly graphical interface:

Running the GUI:
 1.) Click on [Start]->[Programs]->[Azavea]->[ACS Alchemist GUI]
 2.) Fill out the form, select a variables file, and click export!  You can also save and load your job at any time!  These job files can be used interchangeably with the command line app.

Running the Command Line Interface:
 1.) Click on [Start]->[Programs]->[Azavea]->[ACS Alchemist] 
 2.) Type AcsAlchemist.exe in the command prompt to list options


The importer will download files to an output directory so you will need to have write permissions for this directory. If you receive permission errors when attempting to run the application in Windows Vista or Windows 7, please try the following steps:

On Windows 7 or Windows Vista:

1. Right->Click on [Start]->[Programs]->[Azavea]->[ACS Alchemist]
2. Select Properties -> Advanced -> Run As Administrator
3. Click on [Start]->[Programs]->[Azavea]->[ACS Alchemist]

Getting help
-------------------------------------
If you need a list of all available commands, just enter the program name without any arguments.

C:\...> ACSAlchemist.exe



Getting the data you want - State and Year
-------------------------------------
The American Community Survey has more than 20,000 variables for 50 states, the District of Columbia, and Puerto Rico all for multiple geographic summary levels.  This tool is designed to enable you to extract up to 100 variables for a single geographic level and a single state in each run.  The first step is to decide what data you want to extract, and the first decision is to pick a state and an ACS year.

This version of the application supports two years of ACS data:
 * ACS 2005-2009 (released in December 2010)
 * ACS 2006-2010 (released in December 2011)


1. To see what states are available, use the following command.

C:\...> ACSAlchemist.exe -listStateCodes
    
2. To see what years are available, use the following command.

C:\...> ACSAlchemist.exe -listYears
    
3. When you know which state and year you'd like to use, you can pre-download the unfiltered state data files by using:

C:\...> ACSAlchemist.exe -s Wyoming –y 2009


Getting the data you want - Summary Level
-------------------------------------------
Variable summaries are grouped into multiple geographic levels of aggregation - tracts, blockgroups, etc. The utility requires you specify a summary level using its code.  You can get a list of these at any time by typing: 

C:\...> ACSAlchemist.exe -listSummaryLevels


Getting the data you want - Variables
-------------------------------------------

Now that you've chosen a State, Summary Level, and Year, you can assemble a list of variables you'd like to retrieve.

Each data point for the ACS is represented by a unique variable ID.  These are available at: 
 * http://www2.census.gov/acs2009_5yr/summaryfile/ACS2009_5-Year_TableShells.xls
 * http://www2.census.gov/acs2010_5yr/summaryfile/ACS2010_5-Year_TableShells.xls

Copies of these spreadsheets were included in the \docs\ directory along with the default installation.  For any Acs Alchemist job, you can select up to 100 Unique ID's from one of these files for a given year, and save these in a "variables file" in plain text format.  The utility will read this file to determine which variables that should be included in your output. 

Each line in the variables file corresponds to a variable ID from an ACS spreadsheet (see above) and an optional label separated by a comma.  If no label is provided, the variable ID will be used instead.  Please note that the optional label will be truncated to 10 characters due to limitations of the DBF file used in the shapefile format. In addition, you should avoid using spaces or special characters (other than ~,-, and _). If you provide  column names that will result in duplicates, the utility will throw an error.  An example of the variables file would be: 


#SAMPLE Variables File
# total population / male / female
#Variable,Label
B01001001,TOTALPOP
B01001002,TOTALMALE
B01001026,TOTALFEMALE

Assuming this is saved in myVariables.txt in your working directory, you can specify this variable file with the following command: 

C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt


Specify an output directory
-------------------------------------
By default, the utility will create a folder in your Windows user directory under "AppData" to store intermediate census data files.  For example, on Windows 7, this might be something like: C:\Users\{YourUserID}\AppData\Local\ACSAlchemist\Data.  If you want to change this default, you can specify an output directory using the "-outputFolder" switch as follows:

-outputFolder c:\Sandbox\ACS


Naming your Job
-------------------------------------
Each run of the utility is called a "job".  The software will create a job name based on the state, the year and the date, unless you specify a job name.  By default these are overwritten every run, but they can be reused if you also specify "-reuseJob".  This can save time during multiple exports.

-jobName DesiredNameHere


Putting it all together
-------------------------------------
Here is an example using what was discussed so far:
  
C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt -jobName Test01 -outputFolder c:\Sandbox\ACS

This command will make sure all necessary files are downloaded for Wyoming and import the selected variables for 2009 into an internal database (using SQLLite) named "Test01".  The variable values were at the blockgroup summary level.  From here you can also export your results to a shapefile using either the summary geography or using a 'fishnet' grid of polygon cells.


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
The geographic data associated with the ACS variables (tract or blockroup boundaries) are downloaded from the Census Bureau in GRS80NAD83 decimal degrees (EPSG:4269).  If you wish to specify a different projection, you can either add an EPSG spatial reference ID (SRID) or specify a filename containing the WKT of the desired projection.  When an output projection is provided, the ACS Alchemist will transform the census boundaries into the requested projection, and provide a corresponding .PRJ file with the export.  The utility uses the PROJ.4 and ProjNET libraries and the supported SRIDs are listed in the "SRID.csv" file in the installation directory.  Here’s an example that uses the PRJ file approach:

C:\...> ACSAlchemist.exe -s Wyoming -e 150 –y 2009 -v myVariablesFile.txt -jobName Test01 -exportToShape -outputProjection myproj.prj


Saving Job Files
-------------------------------------
The ACS Alchemist has lots of options (there are more listed in the UserManual.pdf), and it is easy to make mistakes when using command line flags.  We have therefore provided a mechanism for using saved arguments from a "job file".  A "job file" is simply a list of all the arguments you would normally specify on the command line in a text file. The syntax is simple:

 - Blank lines and lines that start with "#", are ignored
 - All other lines should be a command line flag starting with -, followed by an argument (if required for the particular flag)

To use a job file simply provide the filename as the only argument to the utility. Note that file paths specified in the job file should be relative to the directory the importer will be run in, not to the location of the job file.  An example job file named "myJob.txt" for the previous command would look something like this: 

#
#Sample Job File - myJob.txt
#       Block Group Summaries
#       Using variables myVariablesFile.txt
#       Wyoming 2009
#
 -s Wyoming						#specify Wyoming as the State
 –y 2009						#specify Year
 -e 150							#extract Blockgroups
 -v myVariablesFile.txt			#extract the Variables in myVariablesFile.txt
 -jobName Test01				#use "Test01" as a Job Name
 -exportToShape					#Export results to a Shapefile
 -outputProjection myproj.prj	#convert the output to projection specified in myproj.prj rather than using WGS84
 -outputFolder C:\sandbox\ACS\	#save the data to a directory


And could be run using this command:

C:\...> ACSAlchemist.exe myJob.txt


Frequently Asked Questions
-------------------------------------

Send us questions!  acs-alchemist@azavea.com


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
