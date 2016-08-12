Framework '4.5.1x86'

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$source_dir = "$base_dir\src"
	$result_dir = "$build_dir\results"
	$global:config = "debug"
}


task default -depends local
task local -depends init, compile, test
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
	$version = if ($env:APPVEYOR_BUILD_NUMBER -ne $NULL) { $env:APPVEYOR_BUILD_NUMBER } else { '0' }
	$version = "{0:D5}" -f [convert]::ToInt32($version, 10)
	
    exec { & $source_dir\.nuget\Nuget.exe restore $source_dir\AutoMapper.sln }

    exec { msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\AutoMapper.sln }

	exec { dotnet pack $source_dir\AutoMapper -c $config --version-suffix $version}
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

function Install-Dotnet
{
    $dotnetcli = where-is('dotnet')
	
    if($dotnetcli -eq $null)
    {
		$dotnetPath = "$pwd\.dotnet"
		$dotnetCliVersion = if ($env:DOTNET_CLI_VERSION -eq $null) { 'Latest' } else { $env:DOTNET_CLI_VERSION }
		$dotnetInstallScriptUrl = 'https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/install.ps1'
		$dotnetInstallScriptPath = '.\scripts\obtain\install.ps1'

		md -Force ".\scripts\obtain\" | Out-Null
		curl $dotnetInstallScriptUrl -OutFile $dotnetInstallScriptPath
		& .\scripts\obtain\install.ps1 -Channel "preview" -version $dotnetCliVersion -InstallDir $dotnetPath -NoPath
		$env:Path = "$dotnetPath;$env:Path"
	}
}

function where-is($command) {
    (ls env:\path).Value.split(';') | `
        where { $_ } | `
        %{ [System.Environment]::ExpandEnvironmentVariables($_) } | `
        where { test-path $_ } |`
        %{ ls "$_\*" -include *.bat,*.exe,*cmd } | `
        %{  $file = $_.Name; `
            if($file -and ($file -eq $command -or `
			   $file -eq ($command + '.exe') -or  `
			   $file -eq ($command + '.bat') -or  `
			   $file -eq ($command + '.cmd'))) `
            { `
                $_.FullName `
            } `
        } | `
        select -unique
}