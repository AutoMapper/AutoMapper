function TeamCity-TestSuiteStarted([string]$name) {
	Write-Output "##teamcity[testSuiteStarted name='$name']"
}

function TeamCity-TestSuiteFinished([string]$name) {
	Write-Output "##teamcity[testSuiteFinished name='$name']"
}

function TeamCity-TestStarted([string]$name) {
	Write-Output "##teamcity[testStarted name='$name']"
}

function TeamCity-TestFinished([string]$name) {
	Write-Output "##teamcity[testFinished name='$name']"
}

function TeamCity-TestIgnored([string]$name, [string]$message='') {
	Write-Output "##teamcity[testIgnored name='$name' message='$message']"
}

function TeamCity-TestOutput([string]$name, [string]$output) {
	Write-Output "##teamcity[testStdOut name='$name' out='$output']"
}

function TeamCity-TestError([string]$name, [string]$output) {
	Write-Output "##teamcity[testStdErr name='$name' out='$output']"
}

function TeamCity-TestFailed([string]$name, [string]$message, [string]$details='', [string]$type='', [string]$expected='', [string]$actual='') {
	$output="##teamcity[testFailed ";
	if (![string]::IsNullOrEmpty($type)) {
		$output += " type='$type'"
	}
	
	$output += " name='$name' message='$message' details='$details'"
	
	if (![string]::IsNullOrEmpty($expected)) {
		$output += " expected='$expected'"
	}
	if (![string]::IsNullOrEmpty($actual)) {
		$output += " actual='$actual'"
	}
	
	$output += ']'
	Write-Output $output
}

function TeamCity-PublishArtifact([string]$path) {
	Write-Output "##teamcity[publishArtifacts '$path']"
}

function TeamCity-ReportBuildStart([string]$message) {
	Write-Output "##teamcity[progessStart '$message']"
}

function TeamCity-ReportBuildProgress([string]$message) {
	Write-Output "##teamcity[progessMessage '$message']"
}

function TeamCity-ReportBuildFinish([string]$message) {
	Write-Output "##teamcity[progessFinish '$message']"
}

function TeamCity-ReportBuildStatus([string]$status, [string]$text='') {
	Write-Output "##teamcity[buildStatus '$status' text='$text']"
}

function TeamCity-SetBuildNumber([string]$buildNumber) {
	Write-Output "##teamcity[buildNumber '$buildNumber']"
}

function TeamCity-SetBuildStatistic([string]$key, [string]$value) {
	Write-Output "##teamcity[buildStatisticValue key='$key' value='$value']"
}

function TeamCity-CreateInfoDocument([string]$buildNumber='', [boolean]$status=$true, [string[]]$statusText=$null, [System.Collections.IDictionary]$statistics=$null) {
	$doc=New-Object xml;
	$buildEl=$doc.CreateElement('build');
	
	if (![string]::IsNullOrEmpty($buildNumber)) {
		$buildEl.SetAttribute('number', $buildNumber);
	}
	
	$buildEl=$doc.AppendChild($buildEl);
	
	$statusEl=$doc.CreateElement('statusInfo');
	if ($status) {
		$statusEl.SetAttribute('status', 'SUCCESS');
	} else {
		$statusEl.SetAttribute('status', 'FAILURE');
	}
	
	if ($statusText -ne $null) {
		foreach ($text in $statusText) {
			$textEl=$doc.CreateElement('text');
			$textEl.SetAttribute('action', 'append');
			$textEl.set_InnerText($text);
			$textEl=$statusEl.AppendChild($textEl);
		}
	}	
	
	$statusEl=$buildEl.AppendChild($statusEl);
	
	if ($statistics -ne $null) {
		foreach ($key in $statistics.Keys) {
			$val=$statistics.$key
			if ($val -eq $null) {
				$val=''
			}
			
			$statEl=$doc.CreateElement('statisticsValue');
			$statEl.SetAttribute('key', $key);
			$statEl.SetAttribute('value', $val.ToString());
			$statEl=$buildEl.AppendChild($statEl);
		}
	}
	
	return $doc;
}

function TeamCity-WriteInfoDocument([xml]$doc) {
	$dir=(Split-Path $buildFile)
	$path=(Join-Path $dir 'teamcity-info.xml')
	
	$doc.Save($path);
}