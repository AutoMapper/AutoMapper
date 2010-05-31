Properties {
	$x = 1
	$y = 2
}

FormatTaskName "[{0}]"

Task default -Depends Verify 

Task Verify -Description "This task verifies psake's variables" {	
	
	$assertions = @( 
		((Test-Path 'variable:\psake'), "'psake' variable was not exported from module"),
		(($variable:psake.ContainsKey("build_success")), "psake variable does not contain 'build_success'"),
		(($variable:psake.ContainsKey("use_exit_on_error")), "psake variable does not contain 'use_exit_on_error'"),
		(($variable:psake.ContainsKey("log_error")), "psake variable does not contain 'log_error'"),
		(($variable:psake.ContainsKey("version")), "psake variable does not contain 'version'"),
		(($variable:psake.ContainsKey("build_script_file")), "psake variable does not contain 'build_script_file'"),
		(($variable:psake.ContainsKey("framework_version")), "psake variable does not contain 'framework_version'"),		
		((!$variable:psake.build_success), 'psake.build_success should be $false'),
		((!$variable:psake.use_exit_on_error), 'psake.use_exit_on_error should be $false'),
		((!$variable:psake.log_error), 'psake.log_error should be $false'),
		((![string]::IsNullOrEmpty($variable:psake.version)), 'psake.version was null or empty'),
		(($variable:psake.build_script_file -ne $null), '$psake.build_script_file was null'), 
		(($variable:psake.build_script_file.Name -eq "checkvariables.ps1"), ("psake variable: {0} was not equal to 'VerifyVariables.ps1'" -f $psake.build_script_file.Name)),
		((![string]::IsNullOrEmpty($variable:psake.framework_version)), 'psake variable: $psake.framework_version was null or empty'),
		(($variable:context.Peek().tasks.Count -ne 0), 'psake variable: $tasks had length zero'),
		(($variable:context.Peek().properties.Count -ne 0), 'psake variable: $properties had length zero'),
		(($variable:context.Peek().includes.Count -eq 0), 'psake variable: $includes should have had length zero'),
		(($variable:context.Peek().formatTaskNameString -eq "[{0}]"), 'psake variable: $formatTaskNameString was not set correctly'),
		(($variable:context.Peek().currentTaskName -eq "Verify"), 'psake variable: $currentTaskName was not set correctly')		
	)
	
	foreach ($assertion in $assertions)
	{	
		Assert ( $assertion[0] ) $assertion[1]		
	}	
}