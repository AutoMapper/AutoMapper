param(
    [int]$buildNumber = 0
    )

if(Test-Path Env:\APPVEYOR_BUILD_NUMBER){
    $buildNumber = [int]$Env:APPVEYOR_BUILD_NUMBER
    Write-Host "Using APPVEYOR_BUILD_NUMBER"
}

"Build number $buildNumber"

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

$psakeVersion = (gc $dir\psake.psm1 | Select-String -Pattern "$psake.Version = " | Select-Object -first 1).Line
$start = $psakeVersion.IndexOf('"') + 1
$end = $psakeVersion.IndexOf('"',$start)
$psakeVersion = $psakeVersion.Substring($start, $end - $start)
$nugetVersion = "$psakeVersion-build" + $buildNumber.ToString().PadLeft(5, '0')

"psake version $psakeVersion"
"nuget version $nugetVersion"

$destDir = "$dir\bin"
if (Test-Path $destDir -PathType container) {
    Remove-Item $destDir -Recurse -Force
}

Copy-Item -Recurse $dir\nuget $destDir
Copy-Item -Recurse $dir\en-US $destDir\tools\en-US
Copy-Item -Recurse $dir\examples $destDir\tools\examples
@( "psake.cmd", "psake.ps1", "psake.psm1", "psake-config.ps1", "README.markdown", "license.txt") |
    % { Copy-Item $dir\$_ $destDir\tools }

.\nuget pack "$destDir\psake.nuspec" -Verbosity quiet -Version $nugetVersion
