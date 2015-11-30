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
task ci -depends clean, release, local

task clean {
	rd "$source_dir\artifacts" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

task release {
    $global:config = "release"
}

task compile -depends clean {
	$env:DNX_BUILD_VERSION=$env:build_number

    exec { dnu restore }
    exec { dnu build $source_dir\AutoMapper --configuration $config}
    exec { & $base_dir\.nuget\Nuget.exe restore $source_dir\AutoMapper.NoProjectJson.sln }
    exec { msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\AutoMapper.sln }
}

task test {
	create_directory "$build_dir\results"
    exec { & $source_dir\packages\Fixie.1.0.0.33\lib\Net45\Fixie.Console.exe --xUnitXml $result_dir\AutoMapper.UnitTests.Net4.xml $source_dir/UnitTests/bin/$config/AutoMapper.UnitTests.Net4.dll }
    exec { & $source_dir\packages\Fixie.1.0.0.33\lib\Net45\Fixie.Console.exe --xUnitXml $result_dir\AutoMapper.IntegrationTests.Net4.xml $source_dir/IntegrationTests.Net4/bin/$config/AutoMapper.IntegrationTests.Net4.dll }
}

function global:create_directory($directory_name)
{
	mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}
