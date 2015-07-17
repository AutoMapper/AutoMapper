properties {
	$x = $null
	$y = $null
}

task default -depends TestRequired

task TestRequired -requiredVariables x, y {
}
