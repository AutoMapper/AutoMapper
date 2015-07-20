properties {
  $x = 1
}

task default -depends Verify 

task Verify -description "This task verifies psake's variables" {

  #Verify the exported module variables
  cd variable:
  Assert (Test-Path "psake") "variable psake was not exported from module"

  Assert ($psake.ContainsKey("build_success")) "'psake' variable does not contain key 'build_success'"
  Assert ($psake.ContainsKey("version")) "'psake' variable does not contain key 'version'"
  Assert ($psake.ContainsKey("build_script_file")) "'psake' variable does not contain key 'build_script_file'"
  Assert ($psake.ContainsKey("build_script_dir")) "'psake' variable does not contain key 'build_script_dir'"  

  Assert (!$psake.build_success) '$psake.build_success should be $false'
  Assert ($psake.version) '$psake.version was null or empty'
  Assert ($psake.build_script_file) '$psake.build_script_file was null' 
  Assert ($psake.build_script_file.Name -eq "writing_psake_variables_should_pass.ps1") '$psake.build_script_file.Name was not equal to "writing_psake_variables_should_pass.ps1"'
  Assert ($psake.build_script_dir) '$psake.build_script_dir was null or empty'

  Assert ($psake.context.Count -eq 1) '$psake.context should have had a length of one (1) during script execution'

  $config = $psake.context.peek().config
  Assert ($config) '$psake.config is $null'
  Assert ((new-object "System.IO.FileInfo" $config.buildFileName).FullName -eq $psake.build_script_file.FullName) ('$psake.context.peek().config.buildFileName not equal to "{0}"' -f $psake.build_script_file.FullName)
  Assert ($config.framework -eq "4.0") '$psake.context.peek().config.framework not equal to "4.0"'
  Assert ($config.taskNameFormat -eq "Executing {0}") '$psake.context.peek().config.taskNameFormat not equal to "Executing {0}"'
  Assert (!$config.verboseError) '$psake.context.peek().config.verboseError should be $false'
  Assert ($config.coloredOutput) '$psake.context.peek().config.coloredOutput should be $false'
  Assert ($config.modules -eq $null) '$psake.context.peek().config.modules is not $null'
}