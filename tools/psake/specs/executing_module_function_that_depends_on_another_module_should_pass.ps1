task default -depends test

task test {
    invoke-psake modules\default.ps1
}