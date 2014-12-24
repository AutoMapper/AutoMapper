<#
-------------------------------------------------------------------
Defaults
-------------------------------------------------------------------
$config.buildFileName="default.ps1"
$config.framework = "4.0"
$config.taskNameFormat="Executing {0}"
$config.verboseError=$false
$config.coloredOutput = $true
$config.modules=$null

-------------------------------------------------------------------
Load modules from .\modules folder and from file my_module.psm1
-------------------------------------------------------------------
$config.modules=(".\modules\*.psm1",".\my_module.psm1")

-------------------------------------------------------------------
Use scriptblock for taskNameFormat
-------------------------------------------------------------------
$config.taskNameFormat= { param($taskName) "Executing $taskName at $(get-date)" }
#>
