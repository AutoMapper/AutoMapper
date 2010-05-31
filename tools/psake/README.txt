
How to get started.

- Download and extract the project
- Open up a PowerShell V2 console window
- CD into the directory where you extracted the project (where the psake.psm1 file is)
> Import-Module .\psake.psm1
> Get-Help Invoke-psake -Full
   - this will show you help and examples of how to use psake


> CD .\examples
> Invoke-psake 
   - This will execute the "default" task in the "default.ps1"
> Invoke-psake .\default.ps1 Clean
   - will execute the single task in the default.ps1 script
