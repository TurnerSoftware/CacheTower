Write-Host "Initialising AppVeyor build..." -ForegroundColor "Magenta"

$baseBuildVersion = git describe --tags --abbrev=0
if ($baseBuildVersion -contains "fatal") {
	$baseBuildVersion = "0.0.0"
}

$isTagBuild = $False
$buildMetadata = "$($env:APPVEYOR_REPO_COMMIT.substring(0,7))-$($env:APPVEYOR_BUILD_NUMBER)"
$prereleaseVersion = "dev"

if ($env:APPVEYOR_REPO_TAG -ne "false") {
	$baseBuildVersion = $env:APPVEYOR_REPO_TAG_NAME
	$prereleaseVersion = $False
	$isTagBuild = $True
}
elseif ($env:APPVEYOR_PULL_REQUEST_NUMBER) {
	$prereleaseVersion = "PR$($env:APPVEYOR_PULL_REQUEST_NUMBER)"
}

$buildVersion = "$baseBuildVersion+$buildMetadata"
if ($prereleaseVersion) {
	$buildVersion = "$baseBuildVersion-$prereleaseVersion+$buildMetadata"
}

Update-AppveyorBuild -Version $buildVersion

if ($isTagBuild) {
	.\build.ps1 -CreatePackages $True -BuildVersion $buildVersion
}
else {
	.\build.ps1 -CheckCoverage $True -BuildVersion $buildVersion
}

Exit $LastExitCode