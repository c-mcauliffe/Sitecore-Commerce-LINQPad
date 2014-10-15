# https://powershellzip.codeplex.com/releases

Param (
	[switch]$Publish
)

$ErrorActionPreference = "Stop"
$global:ExitCode = 1

function Write-Log {

	#region Parameters
	
		[cmdletbinding()]
		Param(
			[Parameter(ValueFromPipeline=$true)]
			[array] $Messages,

			[Parameter()] [ValidateSet("Error", "Warn", "Info")]
			[string] $Level = "Info",
			
			[Parameter()]
			[Switch] $NoConsoleOut = $false,
			
			[Parameter()]
			[String] $ForegroundColor = 'White',
			
			[Parameter()] [ValidateRange(1,30)]
			[Int16] $Indent = 0,

			[Parameter()]
			[IO.FileInfo] $Path = ".\NuGet.log",
			
			[Parameter()]
			[Switch] $Clobber,
			
			[Parameter()]
			[String] $EventLogName,
			
			[Parameter()]
			[String] $EventSource,
			
			[Parameter()]
			[Int32] $EventID = 1
			
		)
		
	#endregion

	Begin {}

	Process {
		
		$ErrorActionPreference = "Continue"

		if ($Messages.Length -gt 0) {
			try {			
				foreach($m in $Messages) {			
					if ($NoConsoleOut -eq $false) {
						switch ($Level) {
							'Error' { 
								Write-Error $m -ErrorAction SilentlyContinue
								Write-Host ('{0}{1}' -f (" " * $Indent), $m) -ForegroundColor Red
							}
							'Warn' { 
								Write-Warning $m 
							}
							'Info' { 
								Write-Host ('{0}{1}' -f (" " * $Indent), $m) -ForegroundColor $ForegroundColor
							}
						}
					}

					if ($m.Trim().Length -gt 0) {
						$msg = '{0}{1} [{2}] : {3}' -f (" " * $Indent), (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $Level.ToUpper(), $m
	
						if ($Clobber) {
							$msg | Out-File -FilePath $Path -Force
						} else {
							$msg | Out-File -FilePath $Path -Append
						}
					}
			
					if ($EventLogName) {
			
						if (-not $EventSource) {
							$EventSource = ([IO.FileInfo] $MyInvocation.ScriptName).Name
						}
			
						if(-not [Diagnostics.EventLog]::SourceExists($EventSource)) { 
							[Diagnostics.EventLog]::CreateEventSource($EventSource, $EventLogName) 
						} 

						$log = New-Object System.Diagnostics.EventLog  
						$log.set_log($EventLogName)  
						$log.set_source($EventSource) 
				
						switch ($Level) {
							"Error" { $log.WriteEntry($Message, 'Error', $EventID) }
							"Warn"  { $log.WriteEntry($Message, 'Warning', $EventID) }
							"Info"  { $log.WriteEntry($Message, 'Information', $EventID) }
						}
					}
				}
			} 
			catch {
				throw "Failed to create log entry in: '$Path'. The error was: '$_'."
			}
		}
	}

	End {}

	<#
		.SYNOPSIS
			Writes logging information to screen and log file simultaneously.

		.DESCRIPTION
			Writes logging information to screen and log file simultaneously. Supports multiple log levels.

		.PARAMETER Messages
			The messages to be logged.

		.PARAMETER Level
			The type of message to be logged.
			
		.PARAMETER NoConsoleOut
			Specifies to not display the message to the console.
			
		.PARAMETER ConsoleForeground
			Specifies what color the text should be be displayed on the console. Ignored when switch 'NoConsoleOut' is specified.
		
		.PARAMETER Indent
			The number of spaces to indent the line in the log file.

		.PARAMETER Path
			The log file path.
		
		.PARAMETER Clobber
			Existing log file is deleted when this is specified.
		
		.PARAMETER EventLogName
			The name of the system event log, e.g. 'Application'.
		
		.PARAMETER EventSource
			The name to appear as the source attribute for the system event log entry. This is ignored unless 'EventLogName' is specified.
		
		.PARAMETER EventID
			The ID to appear as the event ID attribute for the system event log entry. This is ignored unless 'EventLogName' is specified.

		.EXAMPLE
			PS C:\> Write-Log -Message "It's all good!" -Path C:\MyLog.log -Clobber -EventLogName 'Application'

		.EXAMPLE
			PS C:\> Write-Log -Message "Oops, not so good!" -Level Error -EventID 3 -Indent 2 -EventLogName 'Application' -EventSource "My Script"

		.INPUTS
			System.String

		.OUTPUTS
			No output.
			
		.NOTES
			Revision History:
				2011-03-10 : Andy Arismendi - Created.
	#>
}

function Create-Process() {
	param([string] $fileName, [string] $arguments)

	$pinfo = New-Object System.Diagnostics.ProcessStartInfo
	$pinfo.RedirectStandardError = $true
	$pinfo.RedirectStandardOutput = $true
	$pinfo.UseShellExecute = $false
	$pinfo.FileName = $fileName
	$pinfo.Arguments = $arguments

	$p = New-Object System.Diagnostics.Process
	$p.StartInfo = $pinfo

	return $p
}

Write-Log " "
Write-Log "Starting Sitecore Package Creation" -ForegroundColor Yellow

# https://gallery.technet.microsoft.com/scriptcenter/PowerShell-and-7Zip-83020e74

[xml]$packageConfig = Get-Content .\Package.Config
[xml]$versioningConfig = Get-Content ..\PackagingSettings.config

$zipExe = $packageConfig.configuration.zip;

$insidePackage = "package.zip";
$verionFile = ".\package\metadata\sc_version.txt";
$version = "";

if($versioningConfig.configuration.versioning.source -eq "assembly"){
	$version = Get-ChildItem -Filter Sitecore.Commerce.LINQPad.dll -Recurse | Select-Object -First 1 -ExpandProperty VersionInfo | % {$_.ProductVersion}
}
else{
	$version = $versioningConfig.configuration.versioning.explicit;
}

$packageName = $packageConfig.configuration.name + " rev. " + $version + ".zip";

set-content $verionFile $version;

#### http://mats.gardstad.se/matscodemix/2009/02/05/calling-7-zip-from-powershell/  
# Alias for 7-zip 
if (-not (test-path "$($zipExe)")) {throw "$($zipExe) needed"} 
set-alias sz "$($zipExe)"

$filePath = ".\package\*";
 
# delete all existing zips
rm *.zip;

# create the inside package zip
sz a -tzip ".\$($insidePackage)" "$filePath";

# create the wrapper zip
sz a -tzip ".\$($packageName)" ".\$($insidePackage)";
 
Write-Log " "
Write-Log "Finished..." -ForegroundColor Green

Write-Log " "
Write-Log "Exit Code: $global:ExitCode" -ForegroundColor Gray

#$host.SetShouldExit($global:ExitCode)
#Exit $global:ExitCode