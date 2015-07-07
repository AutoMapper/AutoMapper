function Main()
{
	write-host "Running psake build tests" -ForeGroundColor GREEN
	remove-module psake -ea SilentlyContinue
	import-module .\psake.psm1
	$psake.run_by_psake_build_tester = $true
	$results = runBuilds
	remove-module psake

	""
	$results | Sort 'Name' | % { if ($_.Result -eq "Passed") { write-host ($_.Name + " (Passed)") -ForeGroundColor 'GREEN'} else { write-host ($_.Name + " (Failed)") -ForeGroundColor 'RED'}} 
	""

	$failed = $results | ? { $_.Result -eq "Failed" }
	if ($failed) 
	{	
		write-host "One or more of the build files failed" -ForeGroundColor RED
		exit 1
	} 
	else 
	{
		write-host "All Builds Passed" -ForeGroundColor GREEN
		exit 0
	}
}

function runBuilds()
{
	$buildFiles = ls specs\*.ps1
	$testResults = @()

	#Add a fake build file to the $buildFiles array so that we can verify
	#that Invoke-psake fails
	$non_existant_buildfile = "" | select Name, FullName
	$non_existant_buildfile.Name = "specifying_a_non_existant_buildfile_should_fail.ps1"
	$non_existant_buildfile.FullName = "c:\specifying_a_non_existant_buildfile_should_fail.ps1"
	$buildFiles += $non_existant_buildfile

	foreach($buildFile in $buildFiles) 
	{		
		$testResult = "" | select Name, Result
		$testResult.Name = $buildFile.Name
		invoke-psake $buildFile.FullName -Parameters @{'p1'='v1'; 'p2'='v2'} -Properties @{'x'='1'; 'y'='2'} -Initialization { if(!$container) { $container = @{}; } $container.bar = "bar"; $container.baz = "baz"; $bar = 2; $baz = 3 } | Out-Null
		$testResult.Result = (getResult $buildFile.Name $psake.build_success)
		$testResults += $testResult
		if ($testResult.Result -eq "Passed")
		{
			write-host "." -ForeGroundColor GREEN -NoNewLine
		}
		else
		{
			write-host "F" -ForeGroundColor RED -NoNewLine
		}
	}

  return $testResults
}

function getResult([string]$fileName, [bool]$buildSucceeded) 
{    
	$shouldSucceed = $null
	if ($fileName.EndsWith("_should_pass.ps1")) 
	{
		$shouldSucceed = $true
	} 
	elseif ($fileName.EndsWith("_should_fail.ps1")) 
	{
		$shouldSucceed = $false
	} 
	else 
	{
		throw "Invalid specification syntax. Specs should end with _should_pass or _should_fail. $fileName"
	}
	if ($buildSucceeded -eq $shouldSucceed) 
	{
		"Passed"
	} 
	else 
	{
		"Failed"
	}
}

main