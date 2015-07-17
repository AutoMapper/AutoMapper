task default -depends Task_With_Alias

task Task_With_Alias -depends Task_Dependency -alias twa {}

task Task_Dependency {}