TaskSetup {
	"executing task setup"
}

Task default -depends Compile, Test, Deploy

Task Compile {
	"Compiling"
}

Task Test -depends Compile {
	"Testing"
}

Task Deploy -depends Test {
	"Deploying"
}