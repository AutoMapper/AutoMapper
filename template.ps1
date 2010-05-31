properties {
  $projectName = "CodeCampServer"
  $base_dir = resolve-path .
  $build_dir = "$base_dir\build"
  $package_dir = "$build_dir\package"
  $package_file = "$base_dir\latestVersion\" + $projectName +"Package.exe"
  $source_dir = "$base_dir\src\"
  $test_dir = "$build_dir\test\"
  $result_dir = "$build_dir\results\"  
  $buildNumber = 99
  
  $databaseName = $projectName
  $databaseServer = ".\SqlExpress"
  $databaseScripts = "$source_dir\Database"
}

task default -depends Clean, CommonAssemblyInfo, Database, Compile
task privateBuild -depends default, Test
task integrationBuild -depends default, Test,Inspection, Package

<# 
Poke HIbernate Config
Create Database Migration
 #>
task CreateSolutionTemplate {
    $newsolution ="CcsArchitecture"
    $templatedir = "..\_CcsTemplate"
    
    .\lib\solutionfactory\SolutionFactory-console.exe export $source_dir$projectName.sln $templatedir
    delete_directory "$templatedir\template\build"
    delete_directory "$templatedir\template\src\_ReSharper.CodeCampServer"
    delete_file "$templatedir\template\latestversion\safesolutionnamepackage.exe"
    delete_file "$templatedir\template\readme.txt"

    copy_files ".\lib\solutionfactory\" "$templatedir\"
    write "Creating $package_dir"
    create_directory $package_dir
    zip_directory $templatedir\template\ $package_dir\VisualStudioTemplate.exe
}

task Database {
    exec { .\lib\tarantino\DatabaseDeployer.exe Rebuild $databaseServer $databaseName  $databaseScripts}
}

task CommonAssemblyInfo {
    $version = "1.0.$buildNumber.0"   
    create-commonAssemblyInfo "$version" $projectName "$source_dir\CommonAssemblyInfo.cs"
}

task Test {  
    copy_all_assemblies_for_test $test_dir
    run_nunit "$projectName.UnitTests.dll"
    run_nunit "$projectName.IntegrationTests.dll"
    load_test_data "$projectName.IntegrationTests.dll"
}

task Compile -depends Clean { 
    exec { msbuild /t:build $source_dir$projectName.sln }
}

task Clean { 
    delete_file $package_file
    delete_directory $build_dir
    create_directory $test_dir 
    create_directory $result_dir
    exec { msbuild /t:clean $source_dir\$projectName.sln }
}

task TestWithCoverage {
    copy_all_assemblies_for_test $test_dir 
    run_nunit_with_coverage "$projectName.UnitTests.dll"
    run_nunit_with_coverage "$projectName.IntegrationTests.dll"
}

task Inspection {
    run_fxcop
    run_source_monitor
}

task Package {
    delete_directory $package_dir    
    copy_website_files "$source_dir\UI" "$package_dir\website" 
    copy_files "$source_dir\Database" "$package_dir\Database" 
    copy_files "$base_dir\lib\tarantino" "$source_dir\Database\Tools" @("*.pdb")
    copy_all_assemblies_for_test "$package_dir\Tests"
    copy_files "$base_dir\lib\cassini" "$package_dir\tests\tools\cassini"
    copy_files "$base_dir\lib\nunit"  "$package_dir\tests\tools\nunit"
    copy_files "$base_dir\lib\gallio" "$package_dir\tests\tools\gallio"
    copy_files "$base_dir\lib\nant" "$package_dir\nant" @( '*.pdb','*.xml')
    copy_files "$base_dir\deployment" "$package_dir"

    $agents_dir = "$package_dir\agents"
    copy_files "$base_dir\lib\tinoBatchJobs" $agents_dir
    copy_files "$source_dir\Ui\bin" $agents_dir
    Copy_and_flatten $source_dir *.config $agents_dir
    
    zip_directory $package_dir $package_file
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions 
# --------------------------------------------------------------------------------------------------------------
function global:zip_directory($directory,$file)
{
    delete_file $file
    cd $directory
    &"$base_dir\lib\7zip\7za.exe" a -mx=9 -r -sfx $file *.*
    cd $base_dir
}

function global:delete_file($file)
{
    if($file) {
        remove-item $file  -force  -ErrorAction SilentlyContinue | out-null} 
}

function global:run_fxcop
{
   & .\lib\FxCop\FxCopCmd.exe /out:$result_dir"FxCopy.xml"  /file:$test_dir$projectname".*.dll" /quiet /d:$test_dir /c /summary | out-file $result_dir"fxcop.log"
}
function global:run_source_monitor
{
$command =  $result_dir + "command.xml"

"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<sourcemonitor_commands>
    <write_log>true</write_log>
    <command>
        <project_file>build\results\sm_project.smp</project_file>
        <project_language>CSharp</project_language>
        <source_directory>src</source_directory>
        <include_subdirectories>true</include_subdirectories>
        <checkpoint_name>0</checkpoint_name>
        <export>
            <export_file>build\results\sm_summary.xml</export_file>
            <export_type>1</export_type>
        </export>
    </command>
    <command>
        <project_file>build\results\sm_project.smp</project_file>
        <checkpoint_name>0</checkpoint_name> 
        <export>
            <export_file>build\results\sm_details.xml</export_file>
            <export_type>2</export_type>
        </export>
    </command> 
</sourcemonitor_commands>" | out-file $command -encoding "ASCII"

	.\lib\sourcemonitor\sourcemonitor.exe /C $command | out-null
    Convert-WithXslt -originalXmlFilePath $result_dir"sm_details.xml"  -xslFilePath  "lib\sourcemonitor\SourceMonitorSummaryGeneration.xsl" -outputFilePath $result_dir"sm_top15.xml" 

}


function global:delete_directory($directory_name)
{
  rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:create_directory($directory_name)
{
  mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:run_nunit ($test_assembly)
{
    exec { & lib\nunit\nunit-console-x86.exe $test_dir$test_assembly /nologo /nodots /xml=$result_dir$test_assembly.xml /exclude=DataLoader}
}

function global:load_test_data ($test_assembly)
{
    exec { & lib\nunit\nunit-console-x86.exe $test_dir$test_assembly /nologo /nodots /include=DataLoader}
}


function global:run_nunit_with_coverage($test_assembly)
{
    exec {  .\lib\ncover\NCover.Console.exe $base_dir\lib\nunit\nunit-console.exe $test_dir$test_assembly /noshadow /nologo /nodots  /xml=$result_dir$test_assembly.xml  //x $result_dir"$test_assembly.Coverage.xml"  //ias $projectName".Core;"$projectName".UI;"$projectName".Infrastructure;"$projectName".DependencyInjection" //w $test_dir //h $result_dir //reg}

}

function global:Copy_and_flatten ($source,$filter,$dest)
{
  ls $source -filter $filter -r | cp -dest $dest
}

function global:copy_all_assemblies_for_test($destination){
  create_directory $destination
  Copy_and_flatten $source_dir *.dll $destination
  Copy_and_flatten $source_dir *.config $destination
  Copy_and_flatten $source_dir *.xml $destination
  Copy_and_flatten $source_dir *.pdb $destination
}

function global:copy_website_files($source,$destination){
    $exclude = @('*.user','*.dtd','*.tt','*.cs','*.csproj') 
    copy_files $source $destination $exclude
}

function global:copy_files($source,$destination,$exclude=@()){    
    create_directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
}

function global:Convert-WithXslt($originalXmlFilePath, $xslFilePath, $outputFilePath) 
{
   ## Simplistic error handling
   $xslFilePath = resolve-path $xslFilePath
   if( -not (test-path $xslFilePath) ) { throw "Can't find the XSL file" } 
   $originalXmlFilePath = resolve-path $originalXmlFilePath
   if( -not (test-path $originalXmlFilePath) ) { throw "Can't find the XML file" } 
   #$outputFilePath = resolve-path $outputFilePath -ErrorAction SilentlyContinue 
   if( -not (test-path (split-path $originalXmlFilePath)) ) { throw "Can't find the output folder" } 

   ## Get an XSL Transform object (try for the new .Net 3.5 version first)
   $EAP = $ErrorActionPreference
   $ErrorActionPreference = "SilentlyContinue"
   $script:xslt = new-object system.xml.xsl.xslcompiledtransform
   trap [System.Management.Automation.PSArgumentException] 
   {  # no 3.5, use the slower 2.0 one
      $ErrorActionPreference = $EAP
      $script:xslt = new-object system.xml.xsl.xsltransform
   }
   $ErrorActionPreference = $EAP
   
   ## load xslt file
   $xslt.load( $xslFilePath )
     
   ## transform 
   $xslt.Transform( $originalXmlFilePath, $outputFilePath )
}

function global:create-commonAssemblyInfo($version,$applicationName,$filename)
{
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

    [assembly: ComVisibleAttribute(false)]
    [assembly: AssemblyVersionAttribute(""$version"")]
    [assembly: AssemblyFileVersionAttribute(""$version"")]
    [assembly: AssemblyCopyrightAttribute(""Copyright 2010"")]
    [assembly: AssemblyProductAttribute(""$applicationName"")]
    [assembly: AssemblyCompanyAttribute("""")]
    [assembly: AssemblyConfigurationAttribute(""release"")]
    [assembly: AssemblyInformationalVersionAttribute(""$version"")]"  | out-file $filename -encoding "ASCII"    
}