task default -depends MSBuildWithError

task MSBuildWithError {
   msbuild ThisFileDoesNotExist.sln
}