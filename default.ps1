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

task release {
    $global:config = "release"
}

task compile -depends clean {

	$tag = $(git tag -l --points-at HEAD)
	$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
	$suffix = @{ $true = ""; $false = "ci-$revision"}[$tag -ne $NULL -and $revision -ne "local"]
	$commitHash = $(git rev-parse --short HEAD)
	$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]
    $versionSuffix = @{ $true = "--version-suffix=$($suffix)"; $false = ""}[$suffix -ne ""]

	echo "build: Tag is $tag"
	echo "build: Package version suffix is $suffix"
	echo "build: Build version suffix is $buildSuffix" 
	
	exec { dotnet --version }
	exec { dotnet --info }

	exec { .\nuget.exe restore $base_dir\AutoMapper.sln }
	
    exec { dotnet build -c $config --version-suffix=$buildSuffix }

	exec { dotnet pack $source_dir\AutoMapper\AutoMapper.csproj -c $config --include-symbols --no-build $versionSuffix }
}

task benchmark {
    exec { & $source_dir\Benchmark\bin\$config\net452\Benchmark.exe }
}

task test {
    Push-Location -Path $source_dir\UnitTests

    try {
        exec { & dotnet xunit -configuration Release }
    } finally {
        Pop-Location
    }

    Push-Location -Path $source_dir\IntegrationTests

    try {
        exec { & dotnet xunit -configuration Release }
    } finally {
        Pop-Location
    }
}
