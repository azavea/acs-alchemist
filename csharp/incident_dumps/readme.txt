This folder contains raw dumps of crime incident data from 2007 up to jan 1st, 2011.
The files are projected in 2272 (Philly State Plane South Feet).
The class numbers are mapped below

ClassID	Name	Label
109	ALL_100	Homicides (100 series)
110	ALL_200	Rapes (200 series)
111	ALL_300	Robberies (300 series)
112	ALL_400	Agg Assaults (400 series)
113	ALL_500	Burglaries (500 Series)
114	ALL_600	Thefts (600 Series)
115	ALL_700	Stolen/Recovered Vehicles (700 series)
106 STOLEN_CAR Stolen Vehicles
116	ALL_800	Simple Assaults (800 series)


The Query was:
SELECT eventx, eventy, eventtime
  FROM [HunchLab_Philadelphia].[dbo].[HL_EventLookup]
  where eventtime >= '2007-01-01'
  and eventtime < '2011-01-01'
  and classid = ****
  order by eventtime asc