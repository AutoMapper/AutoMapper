properties {
	$my_property = $p1 + $p2
}

task default -depends TestParams

task TestParams { 
	Assert ($my_property -ne $null) "`$my_property should not be null. Run with -parameters @{'p1' = 'v1'; 'p2' = 'v2'}"
}