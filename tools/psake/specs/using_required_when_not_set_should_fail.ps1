properties {
	$x = $null
	$y = $null
	$z = $null
}

task default -depends TestProperties

task TestProperties -requiredVariables z{
}
