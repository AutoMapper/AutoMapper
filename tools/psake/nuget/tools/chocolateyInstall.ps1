try { 
  $nugetPath = $env:ChocolateyInstall
  $nugetExePath = Join-Path $nuGetPath 'bin'
  $packageBatchFileName = Join-Path $nugetExePath "psake.bat"

  $psakeDir = (Split-Path -parent $MyInvocation.MyCommand.Definition)
  #$path = ($psakeDir | Split-Path | Join-Path -ChildPath  'psake.cmd')
  $path = Join-Path $psakeDir  'psake.cmd'
  Write-Host "Adding $packageBatchFileName and pointing to $path"
  "@echo off
  ""$path"" %*" | Out-File $packageBatchFileName -encoding ASCII 

  write-host "PSake is now ready. You can type 'psake' from any command line at any path. Get started by typing 'psake /?'"

  Write-ChocolateySuccess 'psake'
} catch {
  Write-ChocolateyFailure 'psake' "$($_.Exception.Message)"
  throw 
}