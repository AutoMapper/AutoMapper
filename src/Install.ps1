param($installPath, $toolsPath, $package, $project)
 
    # Need to load MSBuild assembly if it's not loaded yet.
    Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

    # Grab the loaded MSBuild project for the project
    $msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

	# Find the platform-specific reference
	$platformSpecificRef = $msbuild.Xml.Items | Where-Object { $_.ItemType -eq "Reference" -and $_.Include.StartsWith("AutoMapper.") } | Select-Object -First 1

	if ($platformSpecificRef)
	{
		$refPath = ($platformSpecificRef.Metadata | Where-Object { $_.Name -eq "HintPath" } | Select-Object -First 1).Value

		$item = $msbuild.Xml.AddItem("Content", $refPath)
		$item.AddMetadata("Link", [System.IO.Path]::GetFileName($refPath))
		$item.AddMetadata("CopyToOutputDirectory", "PreserveNewest")
		$msbuild.Save()
	}
