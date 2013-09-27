$framework = '4.0x86'

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$dist_dir = "$base_dir\release"
	$source_dir = "$base_dir\src"
	$tools_dir = "$base_dir\tools"
	$test_dir = "$build_dir\test"
	$result_dir = "$build_dir\results"
	$lib_dir = "$base_dir\lib"
	$pkgVersion = if ($env:build_number -ne $NULL) { $env:build_number } else { '2.3.0' }
	$assemblyVersion = $pkgVersion -replace "\.[0-9]*-.*$", ".0.0"
	$assemblyFileVersion = $pkgVersion -replace "-[^0-9]*", "."
	$global:config = "debug"
	$framework_dir = Get-FrameworkDirectory
}


task default -depends local
task local -depends compile, test
task full -depends local, dist
task ci -depends clean, release, commonAssemblyInfo, local, dist

task clean {
	delete_directory "$build_dir"
	delete_directory "$dist_dir"
}

task release {
    $global:config = "release"
}

task compile -depends clean { 
    exec { msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\AutoMapper.sln }
}

task commonAssemblyInfo {
    $commit = if ($env:BUILD_VCS_NUMBER -ne $NULL) { $env:BUILD_VCS_NUMBER } else { git log -1 --pretty=format:%H }
    create-commonAssemblyInfo "$commit" "$source_dir\CommonAssemblyInfo.cs"
}

task test {
	create_directory "$build_dir\results"
    exec { & $lib_dir\xunit.net\xunit.console.clr4.x86.exe $source_dir/UnitTests/bin/NET4/$config/AutoMapper.UnitTests.Net4.dll /xml $result_dir\AutoMapper.UnitTests.Net4.xml }
    exec { & $tools_dir\statlight\statlight.exe -x $source_dir/UnitTests/bin/SL4/$config/AutoMapper.UnitTests.xap -d $source_dir/UnitTests/bin/SL4/$config/AutoMapper.UnitTests.SL4.dll --ReportOutputFile=$result_dir\AutoMapper.UnitTests.SL4.xml --ReportOutputFileType=NUnit }
    exec { & $lib_dir\xunit.net\xunit.console.clr4.x86.exe $source_dir/UnitTests/bin/WinRT/$config/AutoMapper.UnitTests.WinRT.dll /xml $result_dir\AutoMapper.UnitTests.WinRT.xml }
    exec { & $lib_dir\xunit.net\xunit.console.clr4.x86.exe $source_dir/UnitTests/bin/WP8/$config/AutoMapper.UnitTests.WP8.dll /xml $result_dir\AutoMapper.UnitTests.WP8.xml }
}

task dist {
	create_directory $dist_dir
	copy_files "$source_dir\AutoMapper\bin\Net4\$config" "$dist_dir\net40-un"
	copy_files "$source_dir\AutoMapper\bin\sl4\$config" "$dist_dir\sl4-un"
	copy_files "$source_dir\AutoMapper\bin\wp75\$config" "$dist_dir\wp71"
	copy_files "$source_dir\AutoMapper\bin\WinRT\$config" "$dist_dir\windows8-un"
	copy_files "$source_dir\AutoMapper\bin\Android\$config" "$dist_dir\MonoAndroid22"
    create_directory "$dist_dir\net40"
    create_directory "$dist_dir\sl4"
    create_directory "$dist_dir\windows8"
    exec { & $tools_dir\ildasm.exe /NOBAR /OUT="$dist_dir\net40\AutoMapper.il" "$dist_dir\net40-un\AutoMapper.dll" }
    exec { & $framework_dir\ilasm.exe /QUIET /KEY="$source_dir\AutoMapper.snk" /DLL="$dist_dir\net40\AutoMapper.dll" /RESOURCE="$dist_dir\net40\AutoMapper.res" "$dist_dir\net40\AutoMapper.il" }
    exec { & $tools_dir\ildasm.exe /NOBAR /OUT="$dist_dir\net40\AutoMapper.Net4.il" "$dist_dir\net40-un\AutoMapper.Net4.dll" }
    exec { & $framework_dir\ilasm.exe /QUIET /KEY="$source_dir\AutoMapper.snk" /DLL="$dist_dir\net40\AutoMapper.Net4.dll" /RESOURCE="$dist_dir\net40\AutoMapper.Net4.res" "$dist_dir\net40\AutoMapper.Net4.il" }
    exec { & $tools_dir\ildasm.exe /NOBAR /OUT="$dist_dir\sl4\AutoMapper.SL4.il" "$dist_dir\sl4-un\AutoMapper.SL4.dll" }
    exec { & $framework_dir\ilasm.exe /QUIET /KEY="$source_dir\AutoMapper.snk" /DLL="$dist_dir\sl4\AutoMapper.SL4.dll" /RESOURCE="$dist_dir\sl4\AutoMapper.SL4.res" "$dist_dir\sl4\AutoMapper.SL4.il" }
    exec { & $tools_dir\ildasm.exe /NOBAR /OUT="$dist_dir\windows8\AutoMapper.WinRT.il" "$dist_dir\windows8-un\AutoMapper.WinRT.dll" }
    exec { & $framework_dir\ilasm.exe /QUIET /KEY="$source_dir\AutoMapper.snk" /DLL="$dist_dir\windows8\AutoMapper.WinRT.dll" /RESOURCE="$dist_dir\windows8\AutoMapper.WinRT.res" "$dist_dir\windows8\AutoMapper.WinRT.il" }
    create-nuspec "$pkgVersion" "AutoMapper.nuspec"
    create-un-nuspec "$pkgVersion" "AutoMapper.Unsigned.nuspec"
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions 
# --------------------------------------------------------------------------------------------------------------
function Get-FrameworkDirectory()
{
    $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory().Replace("v2.0.50727", "v4.0.30319"))
}

function global:zip_directory($directory, $file)
{
    delete_file $file
    cd $directory
    exec { & "$tools_dir\7-zip\7za.exe" a $file *.* }
    cd $base_dir
}

function global:delete_directory($directory_name)
{
  rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:delete_file($file)
{
    if($file) {
        remove-item $file  -force  -ErrorAction SilentlyContinue | out-null} 
}

function global:create_directory($directory_name)
{
  mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:copy_files($source, $destination, $exclude = @()) {
    create_directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
}

function global:create-commonAssemblyInfo($commit, $filename)
{
	$date = Get-Date
    "using System;
using System.Reflection;
using System.Runtime.InteropServices;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: AssemblyVersionAttribute(""$assemblyVersion"")]
[assembly: AssemblyFileVersionAttribute(""$assemblyFileVersion"")]
[assembly: AssemblyCopyrightAttribute(""Copyright Jimmy Bogard 2008-" + $date.Year + """)]
[assembly: AssemblyProductAttribute(""AutoMapper"")]
[assembly: AssemblyTrademarkAttribute(""AutoMapper"")]
[assembly: AssemblyCompanyAttribute("""")]
[assembly: AssemblyConfigurationAttribute(""release"")]
[assembly: AssemblyInformationalVersionAttribute(""$commit"")]"  | out-file $filename -encoding "ASCII"    
}

function global:create-nuspec($version, $fileName)
{
    "<?xml version=""1.0""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>AutoMapper</id>
    <version>$version</version>
    <authors>Jimmy Bogard</authors>
    <owners>Jimmy Bogard</owners>
    <licenseUrl>https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>http://automapper.org</projectUrl>
    <iconUrl>https://s3.amazonaws.com/automapper/icon.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <summary>A convention-based object-object mapper</summary>
    <description>A convention-based object-object mapper. AutoMapper uses a fluent configuration API to define an object-object mapping strategy. AutoMapper uses a convention-based matching algorithm to match up source to destination values. Currently, AutoMapper is geared towards model projection scenarios to flatten complex object models to DTOs and other simple objects, whose design is better suited for serialization, communication, messaging, or simply an anti-corruption layer between the domain and application layer.</description>
  </metadata>
  <files>
    <file src=""$dist_dir\net40\AutoMapper.dll"" target=""lib\portable-windows8+net40+wp71+sl4+MonoAndroid22"" />
    <file src=""$dist_dir\net40-un\AutoMapper.pdb"" target=""lib\portable-windows8+net40+wp71+sl4+MonoAndroid22"" />
    <file src=""$dist_dir\net40-un\AutoMapper.xml"" target=""lib\portable-windows8+net40+wp71+sl4+MonoAndroid22"" />
    <file src=""$dist_dir\net40\AutoMapper.dll"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.pdb"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.xml"" target=""lib\net40"" />
    <file src=""$dist_dir\net40\AutoMapper.Net4.dll"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.Net4.pdb"" target=""lib\net40"" />
    <file src=""$source_dir\install.ps1"" target=""tools\net40"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\net40"" />
    <file src=""$dist_dir\net40\AutoMapper.dll"" target=""lib\sl4"" />
    <file src=""$dist_dir\net40-un\AutoMapper.pdb"" target=""lib\sl4"" />
    <file src=""$dist_dir\net40-un\AutoMapper.xml"" target=""lib\sl4"" />
    <file src=""$dist_dir\sl4\AutoMapper.SL4.dll"" target=""lib\sl4"" />
    <file src=""$dist_dir\sl4-un\AutoMapper.SL4.pdb"" target=""lib\sl4"" />
    <file src=""$source_dir\install.ps1"" target=""tools\sl4"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\sl4"" />
    <file src=""$dist_dir\wp71\AutoMapper.dll"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.pdb"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.xml"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.WP75.dll"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.WP75.pdb"" target=""lib\wp71"" />
    <file src=""$source_dir\install.ps1"" target=""tools\wp71"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\wp71"" />
    <file src=""$dist_dir\net40\AutoMapper.dll"" target=""lib\windows8"" />
    <file src=""$dist_dir\net40-un\AutoMapper.pdb"" target=""lib\windows8"" />
    <file src=""$dist_dir\net40-un\AutoMapper.xml"" target=""lib\windows8"" />
    <file src=""$dist_dir\windows8\AutoMapper.WinRT.dll"" target=""lib\windows8"" />
    <file src=""$dist_dir\windows8-un\AutoMapper.WinRT.pdb"" target=""lib\windows8"" />
    <file src=""$source_dir\install.ps1"" target=""tools\windows8"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\windows8"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.dll"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.pdb"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.xml"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.Android.dll"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.Android.pdb"" target=""lib\MonoAndroid22"" />
    <file src=""$source_dir\install.ps1"" target=""tools\MonoAndroid22"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\MonoAndroid22"" />
    <file src=""**\*.cs"" target=""src"" />
  </files>
</package>" | out-file $build_dir\$fileName -encoding "ASCII"
}

function global:create-un-nuspec($version, $fileName)
{
    "<?xml version=""1.0""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>AutoMapper.Unsigned</id>
    <version>$version</version>
    <authors>Jimmy Bogard</authors>
    <owners>Jimmy Bogard</owners>
    <licenseUrl>https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>http://automapper.org</projectUrl>
    <iconUrl>https://s3.amazonaws.com/automapper/icon.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <summary>A convention-based object-object mapper (strong named version)</summary>
    <description>A convention-based object-object mapper. AutoMapper uses a fluent configuration API to define an object-object mapping strategy. AutoMapper uses a convention-based matching algorithm to match up source to destination values. Currently, AutoMapper is geared towards model projection scenarios to flatten complex object models to DTOs and other simple objects, whose design is better suited for serialization, communication, messaging, or simply an anti-corruption layer between the domain and application layer.</description>
  </metadata>
  <files>
    <file src=""$dist_dir\net40-un\AutoMapper.dll"" target=""lib\portable-windows8+net40+wp71+sl4+MonoAndroid22"" />
    <file src=""$dist_dir\net40-un\AutoMapper.pdb"" target=""lib\portable-windows8+net40+wp71+sl4+MonoAndroid22"" />
    <file src=""$dist_dir\net40-un\AutoMapper.xml"" target=""lib\portable-windows8+net40+wp71+sl4+MonoAndroid22"" />
    <file src=""$dist_dir\net40-un\AutoMapper.dll"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.pdb"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.xml"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.Net4.dll"" target=""lib\net40"" />
    <file src=""$dist_dir\net40-un\AutoMapper.Net4.pdb"" target=""lib\net40"" />
    <file src=""$source_dir\install.ps1"" target=""tools\net40"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\net40"" />
    <file src=""$dist_dir\sl4-un\AutoMapper.dll"" target=""lib\sl4"" />
    <file src=""$dist_dir\sl4-un\AutoMapper.pdb"" target=""lib\sl4"" />
    <file src=""$dist_dir\sl4-un\AutoMapper.xml"" target=""lib\sl4"" />
    <file src=""$dist_dir\sl4-un\AutoMapper.SL4.dll"" target=""lib\sl4"" />
    <file src=""$dist_dir\sl4-un\AutoMapper.SL4.pdb"" target=""lib\sl4"" />
    <file src=""$source_dir\install.ps1"" target=""tools\sl4"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\sl4"" />
    <file src=""$dist_dir\wp71\AutoMapper.dll"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.pdb"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.xml"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.WP75.dll"" target=""lib\wp71"" />
    <file src=""$dist_dir\wp71\AutoMapper.WP75.pdb"" target=""lib\wp71"" />
    <file src=""$source_dir\install.ps1"" target=""tools\wp71"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\wp71"" />
    <file src=""$dist_dir\windows8-un\AutoMapper.dll"" target=""lib\windows8"" />
    <file src=""$dist_dir\windows8-un\AutoMapper.pdb"" target=""lib\windows8"" />
    <file src=""$dist_dir\windows8-un\AutoMapper.xml"" target=""lib\windows8"" />
    <file src=""$dist_dir\windows8-un\AutoMapper.WinRT.dll"" target=""lib\windows8"" />
    <file src=""$dist_dir\windows8-un\AutoMapper.WinRT.pdb"" target=""lib\windows8"" />
    <file src=""$source_dir\install.ps1"" target=""tools\windows8"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\windows8"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.dll"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.pdb"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.xml"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.Android.dll"" target=""lib\MonoAndroid22"" />
    <file src=""$dist_dir\MonoAndroid22\AutoMapper.Android.pdb"" target=""lib\MonoAndroid22"" />
    <file src=""$source_dir\install.ps1"" target=""tools\MonoAndroid22"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\MonoAndroid22"" />
    <file src=""**\*.cs"" target=""src"" />
  </files>
</package>" | out-file $build_dir\$fileName -encoding "ASCII"
}

