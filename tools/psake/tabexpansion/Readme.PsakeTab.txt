_________________________________________________________
A powershell script for tab completion of psake module build commands
 - tab completion for file name: psake d<tab> -> psake .\default.ps1
 - tab completion for parameters (docs,task,parameters,properties): psake -t<tab> -> psake -task
 - tab completion for task: psake -t<tab> c -> psake -task Clean
---------------------------------------------------------

_________________________________________________________
Profile example
---------------------------------------------------------
Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
. ./PsakeTabExpansion.ps1
Pop-Location

if((Test-Path Function:\TabExpansion) -and (-not (Test-Path Function:\DefaultTabExpansion))) {
    Rename-Item Function:\TabExpansion DefaultTabExpansion
}

# Set up tab expansion and include psake expansion
function TabExpansion($line, $lastWord) {
    $lastBlock = [regex]::Split($line, '[|;]')[-1]
    
    switch -regex ($lastBlock) {
        # Execute psake tab completion for all psake-related commands
        '(Invoke-psake|psake) (.*)' { PsakeTabExpansion $lastBlock }
        # Fall back on existing tab expansion
        default { DefaultTabExpansion $line $lastWord }
    }
}
---------------------------------------------------------

_________________________________________________________
Based on work by:

 - Keith Dahlby, http://solutionizing.net/
 - Mark Embling, http://www.markembling.info/
 - Jeremy Skinner, http://www.jeremyskinner.co.uk/
 - Dusty Candland, http://www.candland.net/blog
---------------------------------------------------------
