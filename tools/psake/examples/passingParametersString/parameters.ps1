properties {
	$buildOutputPath = ".\bin\$buildConfiguration"
}

task default -depends DoRelease

task DoRelease {
	Assert ("$buildConfiguration" -ne $null) "buildConfiguration should not have been null"
	Assert ("$buildConfiguration" -eq 'Release') "buildConfiguration=[$buildConfiguration] should have been 'Release'"
	
	Write-Host ""
	Write-Host ""
	Write-Host ""
	Write-Host -NoNewline "Would build output into path "
	Write-Host -NoNewline -ForegroundColor Green "$buildOutputPath"
	Write-Host -NoNewline " for build configuration "
	Write-Host -ForegroundColor Green "$buildConfiguration"
	Write-Host -NoNewline "."
	Write-Host ""
	Write-Host ""
	Write-Host ""
}
