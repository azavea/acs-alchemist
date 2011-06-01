ralph.bat runs everything
The requested variables were split into 3 different files to deal with the 254 column limit of shapefiles (the AcsImporter does not yet automatically deal with this limitation and will probably explode if you give it too many)
There are 6 different jobs to run, one each for the three different files, and one each for block groups (BG) and fishnets (Grid)

Variable name changes (to avoid column name collisions):
"B05002002","nnatbrn" --> "B05002002","nnatbrn1"
"B05012002","nnatbrn" --> "B05012002","nnatbrn2"
"B01001D001","nasian" --> "B01001D001","nasian1"
"B02001005","nasian" --> "B02001005","nasian2"