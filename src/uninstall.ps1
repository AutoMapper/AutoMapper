param($installPath, $toolsPath, $package, $project)

	$platformSpecificRef = $project.ProjectItems | Where-Object { $_.Name.StartsWith("AutoMapper.") -and $_.Name.EndsWith(".dll") -and $_.Properties.Item("ItemType").Value -eq "Content" }

	if ($platformSpecificRef)
	{
		$platformSpecificRef.Remove()
	}
