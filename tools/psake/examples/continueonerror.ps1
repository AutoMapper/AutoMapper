Task default -Depends TaskA

Task TaskA -Depends TaskB {
	"Task - A"
}

Task TaskB -Depends TaskC -ContinueOnError {
	"Task - B"
	throw "I failed on purpose!"
}

Task TaskC {
	"Task - C"
}