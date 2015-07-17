Framework '3.5'

task default -depends AspNetCompiler

task AspNetCompiler {
  aspnet_compiler
  if ($LastExitCode -ne 1) {
    throw 'Error: Could not execute aspnet_compiler'
  }
  $global:LastExitCode = 0
}
