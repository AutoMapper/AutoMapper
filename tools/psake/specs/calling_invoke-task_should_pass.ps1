task default -depends A,B

task A {
}

task B {
	"inside task B before calling task C"
	invoke-task C
	"inside task B after calling task C"
}

task C {
}