Task default -Depends AlwaysFail

Task AlwaysFail {
	Assert $false "This should always fail."
}