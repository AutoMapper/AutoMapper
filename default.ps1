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
task ci -depends clean, release, local

task clean {
	rd "$source_dir\artifacts" -recurse -force  -ErrorAction SilentlyContinue | out-null
	rd "$base_dir\build" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

task init {
	$dnxVersion = Get-DnxVersion
	
	# Make sure per-user DNVM is installed
	Install-Dnvm

	# Install DNX
	dnvm install $dnxVersion -r CoreCLR -NoNative
	dnvm install $dnxVersion -r CLR -NoNative
	dnvm use $dnxVersion -r CLR
}

task release {
    $global:config = "release"
}

task compile -depends clean {
	$env:DNX_BUILD_VERSION=$env:build_number

    exec { dnu restore }
    exec { dnu pack $source_dir\AutoMapper --configuration $config}
    exec { & $base_dir\.nuget\Nuget.exe restore $source_dir\AutoMapper.NoProjectJson.sln }
    exec { msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\AutoMapper.sln }
}

task test {
	mkdir $result_dir
    exec { & $source_dir\packages\Fixie.1.0.0.33\lib\Net45\Fixie.Console.exe --xUnitXml $result_dir\AutoMapper.UnitTests.Net4.xml $source_dir/UnitTests/bin/$config/AutoMapper.UnitTests.Net4.dll }
    exec { & $source_dir\packages\Fixie.1.0.0.33\lib\Net45\Fixie.Console.exe --xUnitXml $result_dir\AutoMapper.IntegrationTests.Net4.xml $source_dir/IntegrationTests.Net4/bin/$config/AutoMapper.IntegrationTests.Net4.dll }
}

function Install-Dnvm
{
    & where.exe dnvm 2>&1 | Out-Null
    if(($LASTEXITCODE -ne 0) -Or ((Test-Path Env:\TEAMCITY_VERSION) -eq $true))
    {
        Write-Host "DNVM not found"
        &{$Branch='dev';iex ((New-Object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}

        if($env:DNX_HOME -eq $NULL)
        {
            Write-Host "Initial DNVM environment setup failed; running manual setup"
            $tempDnvmPath = Join-Path $env:TEMP "dnvminstall"
            $dnvmSetupCmdPath = Join-Path $tempDnvmPath "dnvm.ps1"
            & $dnvmSetupCmdPath setup
        }
    }
}

function Get-DnxVersion
{
    $globalJson = Join-Path $PSScriptRoot "global.json"
    $jsonData = Get-Content -Path $globalJson -Raw | ConvertFrom-JSON
    return $jsonData.sdk.version
}
