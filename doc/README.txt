-------------------------------------------------------------------------------
ACS Importer Readme
-------------------------------------------------------------------------------


Introduction
-------------------------------------

 The ACS Importer is a tool that can help you extract specific portions of the
American Community Survey.  Specifically this version targets the 2005-2009
ACS 5-year data release at the block group level.

 This tool benefits from the use of several other open source projects, please
jump to the bottom of this readme for more information on those projects.
  
Getting Started
-------------------------------------

  Installing the ACS Importer from the install file should be relatively
quick.  Just run the ACSImporter.exe file, and select the location where
you'd like the program to be.  From there you'll want to open a command
prompt to the install location.  The importer will be downloading files
to the working directory or output directory, so make sure you're
running it as an Administrator.  If you're not, you'll see errors if it
will be a problem.

On Windows 7 or Windows Vista:
1. Right->Click on [Start]->[Programs]->[Acs Importer]->[Acs Importer]
2. Select Properties -> Advanced -> Run As Administrator
3. Click on [Start]->[Programs]->[Acs Importer]->[Acs Importer]

You should now have a command prompt in the same folder as the importer.

On Windows XP (no run as admin):
1. Click on [Start]->[Programs]->[Acs Importer]->[Acs Importer]


Getting help
-------------------------------------

  If you need a quick list of available commands, just enter the program name
without any arguments.

C:\...> AcsDataImporter.exe


Getting the data you Want
-------------------------------------

  The American Community Survey probably has more data points than you are 
interested in all at once.  The first step is starting to get the data you
are interested in.

1. To see what states are available, use the following command.

C:\...> AcsDataImporter.exe -listStateCodes
    
2. When you know which state you'd like to use, you can pre-download the
unfiltered state data files by using:

C:\...> AcsDataImporter.exe -s Wyoming


Summary Levels
-------------------------------------

  Variable summaries are grouped into a number of levels.  The importer
will require a summary level specified by code, These can be listed by typing:

C:\...> AcsDataImporter.exe -listSummaryLevels


Variable Files
-------------------------------------

  Each data point for the ACS is represented by a unique variable ID.
These are available here: 
http://www2.census.gov/acs2009_5yr/summaryfile/ACS2009_5-Year_TableShells.xls
From this file, select up to 100 Unique ID's per export job you'd like,
and save these to your variable file in the following format.  For example:

B01001001,TOTALPOP
B01001002,TOTALMALE
B01001026,TOTALFEMALE

C:\...> AcsDataImporter.exe -s Wyoming -e 150 -v myVariablesFile.txt

Naming your export
-------------------------------------

  The importer utility will name jobs based on the state, and date, unless
you provide a specific name you'd like.

-jobName DesiredNameHere

A more thorough example
-------------------------------------

  Here is an example export using what was discussed so far:
  
C:\...> AcsDataImporter.exe -s Wyoming -e 150 -v myVariablesFile.txt -jobName Test01

This command will make sure all necessary files are downloaded for Wyoming,
and import the selected variables into an internal database named "Test01".
The variable values were at the block group summary level.  From here you 
can export your results into a shapefile using the summary geography, or
using a grid 'fishnet.'

Exporting to a Shapefile
-------------------------------------

  When you're ready to run your full export, you can send everything to
a Shapefile.  "-exportToShape" will cause your data to be saved to a
shapefile in the given output projection (see below) with the name provided
by 'jobName.'
  
C:\...> AcsDataImporter.exe -s Wyoming -e 150 -v myVariablesFile.txt -jobName Test01 -exportToShape
C:\...> dir Test01.*

Test01.dbf
Test01.prj
Test01.shp
Test01.shx

Setting an Output Projection
-------------------------------------

  The ACS data and summary levels are all in WGS84NAD83 degrees.
If you know the projection you'd like to be working in, you can specify
either a SRID, or a filename containing the WKT of your desired projection,
and the importer will reproject the data into your projection when exporting.

C:\...> AcsDataImporter.exe -s Wyoming -e 150 -v myVariablesFile.txt -jobName Test01 -exportToShape -outputProjection myproj.prj


Frequently Asked Questions
-------------------------------------

 Send us questions!  info@azavea.com

Open Source Libraries Used
-------------------------------------

 Special Thanks to those amazing open source libraries that are so useful!
Please see the /Licenses folder for the licenses for each respective project,
and see below for links to each project.  Apologies if these links are 
incorrect or out-of-date.

 * log4net!
   * http://logging.apache.org/log4net/
 * GeoApi!
   * http://geoapi.codeplex.com/
 * NetTopologySuite!
   * http://code.google.com/p/nettopologysuite/
 * PowerCollections!
   * http://powercollections.codeplex.com/
 * ProjNET!
   * http://projnet.codeplex.com/
 * Ionic.Zip!
   * http://dotnetzip.codeplex.com/
 * Newtonsoft.Json!
   * http://james.newtonking.com/pages/json-net.aspx
 * System.Data.SQLite!
   * http://system.data.sqlite.org
 * Spatialite!
   * http://www.gaia-gis.it/spatialite/
 * Sqlite!
   * http://www.sqlite.org/
 * ExcelBinaryReader!
   * http://exceldatareader.codeplex.com/
 