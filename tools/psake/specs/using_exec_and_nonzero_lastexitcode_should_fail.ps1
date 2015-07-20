task default -depends MSBuildWithError

task MSBuildWithError {
  exec { msbuild ThisFileDoesNotExist.sln }
}