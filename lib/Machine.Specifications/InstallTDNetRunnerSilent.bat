@echo off & if not "%ECHO%"=="" echo %ECHO%

setlocal
set LOCALDIR=%~dp0

echo Windows Registry Editor Version 5.00 > MSpecTDNet.reg
echo [HKEY_CURRENT_USER\Software\MutantDesign\TestDriven.NET\TestRunners\MSpec] >> MSpecTDNet.reg
echo "Application"="" >> MSpecTDNet.reg
echo "AssemblyPath"="%LOCALDIR:\=\\%Machine.Specifications.TDNetRunner.dll" >> MSpecTDNet.reg
echo "TargetFrameworkAssemblyName"="Machine.Specifications" >> MSpecTDNet.reg
echo "TypeName"="Machine.Specifications.TDNetRunner.SpecificationRunner" >> MSpecTDNet.reg
echo @="5" >> MSpecTDNet.reg

regedit /s MSpecTDNet.reg 

del MSpecTDNet.reg
