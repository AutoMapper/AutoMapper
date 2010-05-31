Properties {
	$x = 1
}

Task default -Depends RunNested1, RunNested2, CheckX

Task RunNested1 {
	Invoke-psake .\nested\nested1.ps1
}

Task RunNested2 {
	Invoke-psake .\nested\nested2.ps1
}

Task CheckX{
	Assert ($x -eq 1) '$x was not 1' 
}