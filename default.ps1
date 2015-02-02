$framework = '4.5.1x86'

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$dist_dir = "$base_dir\release"
	$source_dir = "$base_dir\src"
	$tools_dir = "$base_dir\tools"
	$test_dir = "$build_dir\test"
	$result_dir = "$build_dir\results"
	$lib_dir = "$base_dir\lib"
	$pkgVersion = if ($env:build_number -ne $NULL) { $env:build_number } else { '4.0.0' }
	$assemblyVersion = $pkgVersion -replace "\-.*$", ".0"
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
    exec { & $source_dir\.nuget\Nuget.exe restore $source_dir }
    exec { msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\AutoMapper.sln }
    exec { msbuild /t:Clean /t:Build /p:Configuration=ReleaseWin8 /v:q /p:NoWarn=1591 /nologo $source_dir\AutoMapper.sln }
    exec { kpm build $source_dir\AutoMapper }
}

task commonAssemblyInfo {
    $commit = if ($env:BUILD_VCS_NUMBER -ne $NULL) { $env:BUILD_VCS_NUMBER } else { git log -1 --pretty=format:%H }
    create-commonAssemblyInfo "$commit" "$source_dir\CommonAssemblyInfo.cs"
}

task test {
	create_directory "$build_dir\results"
    exec { & $source_dir\packages\Fixie.1.0.0.3\lib\Net45\Fixie.Console.exe --xUnitXml $result_dir\AutoMapper.UnitTests.Net4.xml $source_dir/UnitTests/bin/NET4/$config/AutoMapper.UnitTests.Net4.dll }
    exec { & $tools_dir\statlight\statlight.exe -x $source_dir/UnitTests/bin/SL5/$config/AutoMapper.UnitTests.xap -d $source_dir/UnitTests/bin/SL5/$config/AutoMapper.UnitTests.SL5.dll --ReportOutputFile=$result_dir\AutoMapper.UnitTests.SL5.xml --ReportOutputFileType=NUnit }
    exec { & $source_dir\packages\xunit.runners.2.0.0-beta5-build2785\tools\xunit.console.x86.exe $source_dir/UnitTests/bin/WinRT/$config/AutoMapper.UnitTests.WinRT.dll -xml $result_dir\AutoMapper.UnitTests.WinRT.xml -parallel none }
    exec { & $source_dir\packages\xunit.runners.2.0.0-beta5-build2785\tools\xunit.console.x86.exe $source_dir/UnitTests/bin/WP8/$config/AutoMapper.UnitTests.WP8.dll -xml $result_dir\AutoMapper.UnitTests.WP8.xml -parallel none }
}

task dist {
	create_directory $build_dir
	create_directory $dist_dir
	copy_files "$source_dir\AutoMapper\bin\Net4\$config" "$dist_dir\net40"
	copy_files "$source_dir\AutoMapper\bin\Profile136\$config" "$dist_dir\Profile136"
	copy_files "$source_dir\AutoMapper\bin\sl5\$config" "$dist_dir\sl5"
	copy_files "$source_dir\AutoMapper\bin\wp8\$config" "$dist_dir\wp8"
	copy_files "$source_dir\AutoMapper\bin\wpa81\$config" "$dist_dir\wpa81"
	copy_files "$source_dir\AutoMapper\bin\WinRT\$config" "$dist_dir\windows81"
	copy_files "$source_dir\AutoMapper\bin\Android\$config" "$dist_dir\MonoAndroid"
	copy_files "$source_dir\AutoMapper\bin\iPhone\$config" "$dist_dir\MonoTouch"
	copy_files "$source_dir\AutoMapper\bin\iPhone10\$config" "$dist_dir\Xamarin.iOS10"
	copy_files "$source_dir\Artifacts\bin\AutoMapper.CoreCLR\$config\aspnet50" "$dist_dir\aspnet50"
	copy_files "$source_dir\Artifacts\bin\AutoMapper.CoreCLR\$config\aspnetcore50" "$dist_dir\aspnetcore50"
    create-nuspec "$pkgVersion" "AutoMapper.nuspec"
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

[assembly: CLSCompliant(true)]
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
    <dependencies>
      <group targetFramework=""Asp.NetCore5.0"">
        <dependency id=""System.Runtime"" version=""4.0.20-beta-22530"" />
        <dependency id=""System.Linq.Expressions"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Linq"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Reflection"" version=""4.0.10-beta-22530"" />
        <dependency id=""System.Text.RegularExpressions"" version=""4.0.10-beta-22530"" />
        <dependency id=""System.Reflection.TypeExtensions"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Reflection.Emit"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Threading"" version=""4.0.10-beta-22530"" />
        <dependency id=""System.Runtime.Extensions"" version=""4.0.10-beta-22530"" />
        <dependency id=""System.Reflection.Extensions"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Collections.Specialized"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Collections.Concurrent"" version=""4.0.10-beta-22530"" />
        <dependency id=""System.ComponentModel.TypeConverter"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Reflection.Primitives"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Linq.Queryable"" version=""4.0.0-beta-22530"" />
        <dependency id=""System.Diagnostics.Debug"" version=""4.0.10-beta-22530"" />
        <dependency id=""System.ObjectModel"" version=""4.0.10-beta-22530"" />
      </group>
    </dependencies>
    <frameworkAssemblies>
      <frameworkAssembly assemblyName=""mscorlib"" targetFramework=""Asp.Net5.0"" />
      <frameworkAssembly assemblyName=""System"" targetFramework=""Asp.Net5.0"" />
      <frameworkAssembly assemblyName=""System.Core"" targetFramework=""Asp.Net5.0"" />
      <frameworkAssembly assemblyName=""Microsoft.CSharp"" targetFramework=""Asp.Net5.0"" />
    </frameworkAssemblies>
  </metadata>
  <files>
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\portable-windows8+net40+wp8+sl5+MonoAndroid+MonoTouch"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\portable-windows8+net40+wp8+sl5+MonoAndroid+MonoTouch"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\portable-windows8+net40+wp8+sl5+MonoAndroid+MonoTouch"" />
    <file src=""$dist_dir\net40\AutoMapper.dll"" target=""lib\portable-windows8+net40+wp8+wpa81+sl5+MonoAndroid+MonoTouch"" />
    <file src=""$dist_dir\net40\AutoMapper.pdb"" target=""lib\portable-windows8+net40+wp8+wpa81+sl5+MonoAndroid+MonoTouch"" />
    <file src=""$dist_dir\net40\AutoMapper.xml"" target=""lib\portable-windows8+net40+wp8+wpa81+sl5+MonoAndroid+MonoTouch"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\net40"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\net40"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\net40"" />
    <file src=""$dist_dir\net40\AutoMapper.Net4.dll"" target=""lib\net40"" />
    <file src=""$dist_dir\net40\AutoMapper.Net4.pdb"" target=""lib\net40"" />
    <file src=""$source_dir\install.ps1"" target=""tools\net40"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\net40"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\sl5"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\sl5"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\sl5"" />
    <file src=""$dist_dir\sl5\AutoMapper.SL5.dll"" target=""lib\sl5"" />
    <file src=""$dist_dir\sl5\AutoMapper.SL5.pdb"" target=""lib\sl5"" />
    <file src=""$source_dir\install.ps1"" target=""tools\sl5"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\sl5"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\wp8"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\wp8"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\wp8"" />
    <file src=""$dist_dir\wp8\AutoMapper.WP8.dll"" target=""lib\wp8"" />
    <file src=""$dist_dir\wp8\AutoMapper.WP8.pdb"" target=""lib\wp8"" />
    <file src=""$source_dir\install.ps1"" target=""tools\wp8"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\wp8"" />
    <file src=""$dist_dir\wpa81\AutoMapper.dll"" target=""lib\wpa81"" />
    <file src=""$dist_dir\wpa81\AutoMapper.pdb"" target=""lib\wpa81"" />
    <file src=""$dist_dir\wpa81\AutoMapper.xml"" target=""lib\wpa81"" />
    <file src=""$dist_dir\wpa81\AutoMapper.WPA81.dll"" target=""lib\wpa81"" />
    <file src=""$dist_dir\wpa81\AutoMapper.WPA81.pdb"" target=""lib\wpa81"" />
    <file src=""$source_dir\install.ps1"" target=""tools\wpa81"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\wpa81"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\windows81"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\windows81"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\windows81"" />
    <file src=""$dist_dir\windows81\AutoMapper.WinRT.dll"" target=""lib\windows81"" />
    <file src=""$dist_dir\windows81\AutoMapper.WinRT.pdb"" target=""lib\windows81"" />
    <file src=""$source_dir\install.ps1"" target=""tools\windows81"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\windows81"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\MonoAndroid"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\MonoAndroid"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\MonoAndroid"" />
    <file src=""$dist_dir\MonoAndroid\AutoMapper.Android.dll"" target=""lib\MonoAndroid"" />
    <file src=""$dist_dir\MonoAndroid\AutoMapper.Android.pdb"" target=""lib\MonoAndroid"" />
    <file src=""$source_dir\install.ps1"" target=""tools\MonoAndroid"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\MonoAndroid"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\MonoTouch"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\MonoTouch"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\MonoTouch"" />
    <file src=""$dist_dir\MonoTouch\AutoMapper.iOS.dll"" target=""lib\MonoTouch"" />
    <file src=""$dist_dir\MonoTouch\AutoMapper.iOS.pdb"" target=""lib\MonoTouch"" />
    <file src=""$source_dir\install.ps1"" target=""tools\MonoTouch"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\MonoTouch"" />
    <file src=""$dist_dir\Profile136\AutoMapper.dll"" target=""lib\Xamarin.iOS10"" />
    <file src=""$dist_dir\Profile136\AutoMapper.pdb"" target=""lib\Xamarin.iOS10"" />
    <file src=""$dist_dir\Profile136\AutoMapper.xml"" target=""lib\Xamarin.iOS10"" />
    <file src=""$dist_dir\Xamarin.iOS10\AutoMapper.iOS10.dll"" target=""lib\Xamarin.iOS10"" />
    <file src=""$dist_dir\Xamarin.iOS10\AutoMapper.iOS10.pdb"" target=""lib\Xamarin.iOS10"" />
    <file src=""$source_dir\install.ps1"" target=""tools\Xamarin.iOS10"" />
    <file src=""$source_dir\uninstall.ps1"" target=""tools\Xamarin.iOS10"" />
    <file src=""$source_dir\AutoMapper.targets"" target=""tools"" />
    <file src=""$dist_dir\aspnet50\AutoMapper.dll"" target=""lib\aspnet50"" />
    <file src=""$dist_dir\aspnet50\AutoMapper.pdb"" target=""lib\aspnet50"" />
    <file src=""$dist_dir\aspnet50\AutoMapper.xml"" target=""lib\aspnet50"" />
    <file src=""$dist_dir\aspnetcore50\AutoMapper.dll"" target=""lib\aspnetcore50"" />
    <file src=""$dist_dir\aspnetcore50\AutoMapper.pdb"" target=""lib\aspnetcore50"" />
    <file src=""$dist_dir\aspnetcore50\AutoMapper.xml"" target=""lib\aspnetcore50"" />
    <file src=""**\*.cs"" target=""src"" />
  </files>
</package>" | out-file $build_dir\$fileName -encoding "ASCII"
}
