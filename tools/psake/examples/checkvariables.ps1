Properties {
  $x = 1
  $y = 2
}

FormatTaskName "[{0}]"

Task default -Depends Verify 

Task Verify -Description "This task verifies psake's variables" {

  Assert (Test-Path 'variable:\psake') "psake variable was not exported from module"
  
  Assert ($psake.ContainsKey("version")) "psake variable does not contain 'version'"
  Assert ($psake.ContainsKey("context")) "psake variable does not contain 'context'"
  Assert ($psake.ContainsKey("build_success")) "psake variable does not contain 'build_success'"
  Assert ($psake.ContainsKey("build_script_file")) "psake variable does not contain 'build_script_file'"
  Assert ($psake.ContainsKey("build_script_dir")) "psake variable does not contain 'build_script_dir'"

  Assert (![string]::IsNullOrEmpty($psake.version)) '$psake.version was null or empty'
  Assert ($psake.context -ne $null) '$psake.context was null'
  Assert (!$psake.build_success) '$psake.build_success should be $false'
  Assert ($psake.build_script_file -ne $null) '$psake.build_script_file was null'
  Assert ($psake.build_script_file.Name -eq "checkvariables.ps1") ("psake variable: {0} was not equal to 'checkvariables.ps1'" -f $psake.build_script_file.Name)
  Assert (![string]::IsNullOrEmpty($psake.build_script_dir)) '$psake.build_script_dir was null or empty'

  Assert ($psake.context.Peek().tasks.Count -ne 0) "psake context variable 'tasks' had length zero"
  Assert ($psake.context.Peek().properties.Count -ne 0) "psake context variable 'properties' had length zero"
  Assert ($psake.context.Peek().includes.Count -eq 0) "psake context variable 'includes' should have had length zero"
  Assert ($psake.context.Peek().config -ne $null) "psake context variable 'config' was null"

  Assert ($psake.context.Peek().currentTaskName -eq "Verify") 'psake variable: $currentTaskName was not set correctly'
}