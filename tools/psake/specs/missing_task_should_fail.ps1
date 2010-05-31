task default -depends Test

task Test -depends Compile, Clean { 
  Write-Host "Running PSake"
}
