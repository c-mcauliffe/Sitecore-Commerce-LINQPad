SET dll=..\SitecoreCommerceLinqPadExtensions\bin\debug\SitecoreCommerceLinqPadExtensions.dll
SET pdb=..\SitecoreCommerceLinqPadExtensions\bin\debug\SitecoreCommerceLinqPadExtensions.pdb
SET destination=C:\ProgramData\LINQPad\Drivers\DataContext\4.0\Sitecore.Linqpad (1ff490f7181746b3)\
SET destinationSite=C:\inetpub\CSSitecore\Website\bin\

XCOPY /S /R /Y "%dll%" "%destination%"
XCOPY /S /R /Y "%pdb%" "%destination%"
XCOPY /S /R /Y "%dll%" "%destinationSite%"
XCOPY /S /R /Y "%pdb%" "%destinationSite%"