ralph.bat runs everything
The requested variables were split into 3 different files to deal with the 254 column limit of shapefilesn
There are 6 different jobs to run, one each for the three different files, and one each for block groups (BG) and fishnets (Grid).  The fishnet jobs haven't been run yet.

Variable name changes (to avoid column name collisions):
"B05002002","nnatbrn" --> "B05002002","nnatbrn1"
"B05012002","nnatbrn" --> "B05012002","nnatbrn2"
"B01001D001","nasian" --> "B01001D001","nasian1"
"B02001005","nasian" --> "B02001005","nasian2"