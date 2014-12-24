Task default -Depends RunAlwaysFail

Task RunAlwaysFail {
	Invoke-psake .\nested\always_fail.ps1
}
