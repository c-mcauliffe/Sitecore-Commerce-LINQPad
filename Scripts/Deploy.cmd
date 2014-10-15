SET dll=..\Sitecore.Commerce.LinqPad\bin\debug\Sitecore.Commerce.LINQPad.dll
SET pdb=..\Sitecore.Commerce.LinqPad\bin\debug\Sitecore.Commerce.LINQPad.pdb
SET destinationSite=C:\inetpub\CSSitecore\Website\bin\

XCOPY /S /R /Y "%dll%" "%destinationSite%"
XCOPY /S /R /Y "%pdb%" "%destinationSite%"