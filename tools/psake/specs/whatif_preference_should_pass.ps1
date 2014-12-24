Task default -Depends RunWhatIf

Task RunWhatIf {
	try {
		## Setup the -whatif flag globally
		$global:WhatIfPreference = $true
		
		## Ensure that psake ends up by calling something with -whatif e.g. Set-Item
		$parameters = @{p1='whatifcheck';}
		
		Invoke-psake .\nested\whatifpreference.ps1 -parameters $parameters
	} finally {
		$global:WhatIfPreference = $false
	}
}
