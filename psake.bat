powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& {Import-Module '.\tools\psake\psake.psm1'; invoke-psake .\default.ps1 %1; if ($lastexitcode -ne 0) {write-host "ERROR: $lastexitcode" -fore RED; exit $lastexitcode} }" 



