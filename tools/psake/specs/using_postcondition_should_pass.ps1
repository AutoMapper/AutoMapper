task default -depends A,B,C

task A {
	"TaskA"
}

task B -postcondition { return $true } {
	"TaskB"
}

task C {
	"TaskC"
}