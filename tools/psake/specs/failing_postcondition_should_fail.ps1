task default -depends A,B,C

task A {
	"TaskA"
}

task B -postcondition { return $false } {
	"TaskB"
}

task C {
	"TaskC"
}