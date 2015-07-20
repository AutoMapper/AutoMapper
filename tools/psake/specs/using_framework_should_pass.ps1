task default -depends FrameworkFunction 

task FrameworkFunction  {
	AssertFramework '2.0'
	AssertFramework '3.5'
	AssertFramework '4.0'
}

function AssertFramework{
	param(
		[string]$framework
	)
	Framework $framework
	$msBuildVersion = msbuild /version
	Assert ($msBuildVersion[0].ToLower().StartsWith("microsoft (r) build engine version $framework")) '$msBuildVersion does not start with "$framework"'
}