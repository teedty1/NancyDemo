#############################################
# am I running in 32 bit shell?
#############################################
if ($pshome -like "*syswow64*") {
  
  # relaunch this script under 64 bit shell
  & (join-path ($pshome -replace "syswow64", "sysnative") powershell.exe) -file `
    (join-path $psscriptroot $myinvocation.mycommand) @args
 
  # exit 32 bit script
  exit
}

Import-Module -Name ServerManager

Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestMonitor
Write-Output "IIS Request Monitor Installed"

$webclient = new-object System.Net.WebClient 
$document = $webclient.DownloadString("http://169.254.169.254/latest/dynamic/instance-identity/document") | ConvertFrom-Json
$instanceId = $document.instanceId

iisreset /stop
Write-Output "IIS Stopped";

c:\windows\System32\inetsrv\appcmd.exe set apppool /apppool.name DefaultAppPool /enable32BitAppOnWin64:true

Import-Module WebAdministration
$iisAppName = "NancyDemo"
$iisHostname = "nd.bill2paytest.com"
$directoryPath = "c:\inetpub\wwwRoot\NancyDemo"

Get-ChildItem -Path $directoryPath -Recurse | Remove-Item -Force -Recurse

#navigate to the sites root
cd IIS:\Sites\

#create the site
New-WebSite -Name $iisAppName -Port 80 -HostHeader $iisHostname -PhysicalPath $directoryPath -Force
New-WebBinding -Name $iisAppName -Port 80 -HostHeader "www.$iisHostname"

iisreset /start
Write-Output "IIS Start"