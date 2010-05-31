powershell -Command "& {Import-Module .\..\..\psake.psm1; Invoke-psake .\parameters.ps1 -parameters @{"buildConfiguration"='Release';} }"

Pause