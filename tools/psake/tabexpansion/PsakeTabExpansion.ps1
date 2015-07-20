$global:psakeSwitches = @('-docs', '-task', '-properties', '-parameters')

function script:psakeSwitches($filter) {  
  $psakeSwitches | where { $_ -like "$filter*" }
}

function script:psakeDocs($filter, $file) {
  if ($file -eq $null -or $file -eq '') { $file = 'default.ps1' }
  psake $file -docs | out-string -Stream |% { if ($_ -match "^[^ ]*") { $matches[0]} } |? { $_ -ne "Name" -and $_ -ne "----" -and $_ -like "$filter*" }
}

function script:psakeFiles($filter) {
    ls "$filter*.ps1" |% { $_.Name }
}

function PsakeTabExpansion($lastBlock) {
  switch -regex ($lastBlock) {
    '(invoke-psake|psake) ([^\.]*\.ps1)? ?.* ?\-ta?s?k? (\S*)$' { # tasks only
      psakeDocs $matches[3] $matches[2] | sort
    } 
    '(invoke-psake|psake) ([^\.]*\.ps1)? ?.* ?(\-\S*)$' { # switches only
      psakeSwitches $matches[3] | sort
    } 
    '(invoke-psake|psake) ([^\.]*\.ps1) ?.* ?(\S*)$' { # switches or tasks
      @(psakeDocs $matches[3] $matches[2]) + @(psakeSwitches $matches[3]) | sort
    }
    '(invoke-psake|psake) (\S*)$' {
      @(psakeFiles $matches[2]) + @(psakeDocs $matches[2] 'default.ps1') + @(psakeSwitches $matches[2]) | sort
    }
  }
}
