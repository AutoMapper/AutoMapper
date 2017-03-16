Framework '4.5.1x86'

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$source_dir = "$base_dir\src"
	$result_dir = "$build_dir\results"
	$global:config = "debug"
}


task default -depends local
task local -depends compile, test
task ci -depends clean, release, local, benchmark

task clean {
	rd "$source_dir\artifacts" -recurse -force  -ErrorAction SilentlyContinue | out-null
	rd "$base_dir\build" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

task init {
	# Make sure per-user dotnet is installed
	Install-Dotnet
}

task release {
    $global:config = "release"
}

task compile -depends clean {

	$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
	$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
	$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and $revision -ne "local"]
	$commitHash = $(git rev-parse --short HEAD)
	$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]

	echo "build: Package version suffix is $suffix"
	echo "build: Build version suffix is $buildSuffix" 
	
	exec { .\src\.nuget\nuget.exe restore $source_dir\AutoMapper.sln }

	exec { dotnet restore $source_dir\AutoMapper.sln }

    exec { dotnet build $source_dir\AutoMapper.sln -c $config --version-suffix=$buildSuffix -v q /nologo }

	exec { dotnet pack $source_dir\AutoMapper -c $config --include-symbols --no-build --version-suffix=$suffix }
}

task benchmark {
    exec { & $source_dir\Benchmark\bin\$config\Benchmark.exe }
}

task test {
    $testRunners = @(gci $source_dir\packages -rec -filter Fixie.Console.exe)

    if ($testRunners.Length -ne 1)
    {
        throw "Expected to find 1 Fixie.Console.exe, but found $($testRunners.Length)."
    }

    $testRunner = $testRunners[0].FullName

    exec { & $testRunner $source_dir/UnitTests/bin/$config/AutoMapper.UnitTests.Net4.dll }
    exec { & $testRunner $source_dir/IntegrationTests.Net4/bin/$config/AutoMapper.IntegrationTests.Net4.dll }
}
