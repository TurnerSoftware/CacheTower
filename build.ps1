[CmdletBinding(PositionalBinding=$false)]
param(
	[bool] $RunTests = $true,
	[bool] $CheckCoverage,
	[bool] $CreatePackages,
	[string] $BuildVersion
)

$packageOutputFolder = "$PSScriptRoot\build-artifacts"
mkdir -Force $packageOutputFolder | Out-Null

$config = Get-Content "buildconfig.json" | ConvertFrom-Json

if (-not $BuildVersion) {
	$lastTaggedVersion = git describe --tags --abbrev=0
	if ($lastTaggedVersion -contains "fatal") {
		$lastTaggedVersion = "0.0.0"
	}

	$BuildVersion = $lastTaggedVersion
}

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "  RunTests: $RunTests"
Write-Host "  CheckCoverage: $CheckCoverage"
Write-Host "  CreatePackages: $CreatePackages"
Write-Host "  BuildVersion: $BuildVersion"
Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  TestProject: $($config.TestProject)"
Write-Host "  TestCoverageFilter: $($config.TestCoverageFilter)"
Write-Host "Environment:" -ForegroundColor Cyan
Write-Host "  .NET Version:" (dotnet --version)
Write-Host "  Artifact Path: $packageOutputFolder"

Write-Host "Building solution..." -ForegroundColor "Magenta"
dotnet build -c Release /p:Version=$BuildVersion
if ($LastExitCode -ne 0) {
	Write-Host "Build failed, aborting!" -Foreground "Red"
	Exit 1
}
Write-Host "Solution built!" -ForegroundColor "Green"

if ($RunTests) {
	if (-Not $CheckCoverage) {
		Write-Host "Running tests without coverage..." -ForegroundColor "Magenta"
		dotnet test $config.TestProject
		if ($LastExitCode -ne 0) {
			Write-Host "Tests failed, aborting build!" -Foreground "Red"
			Exit 1
		}
		Write-Host "Tests passed!" -ForegroundColor "Green"
	}
	else {
		Write-Host "Running tests with coverage..." -ForegroundColor "Magenta"
		OpenCover.Console.exe -register:user -target:"%LocalAppData%\Microsoft\dotnet\dotnet.exe" -targetargs:"test $($config.TestProject) /p:DebugType=Full" -filter:"$($config.TestCoverageFilter)" -output:"$packageOutputFolder\coverage.xml" -oldstyle
		if ($LastExitCode -ne 0 -Or -Not $?) {
			Write-Host "Failure performing tests with coverage, aborting!" -Foreground "Red"
			Exit 1
		}
		else {
			Write-Host "Tests passed!" -ForegroundColor "Green"
			Write-Host "Saving code coverage..." -ForegroundColor "Magenta"
			codecov -f "$packageOutputFolder\coverage.xml"
			if ($LastExitCode -ne 0 -Or -Not $?) {
				Write-Host "Failure saving code coverage!" -Foreground "Red"
			}
			else {
				Write-Host "Coverage saved!" -ForegroundColor "Green"
			}
		}
	}
}

if ($CreatePackages) {
	Write-Host "Clearing existing $packageOutputFolder... " -NoNewline
	Get-ChildItem $packageOutputFolder | Remove-Item
	Write-Host "Packages cleared!" -ForegroundColor "Green"
	
	Write-Host "Packing..." -ForegroundColor "Magenta"
	dotnet pack --no-build -c Release /p:Version=$BuildVersion /p:PackageOutputPath=$packageOutputFolder
	Write-Host "Packing complete!" -ForegroundColor "Green"
}