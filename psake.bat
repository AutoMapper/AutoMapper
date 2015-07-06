
@echo off
cd %~dp0

SETLOCAL

IF EXIST packages\KoreBuild goto run
.nuget\NuGet.exe install KoreBuild -ExcludeVersion -o packages -nocache -pre

IF "%SKIP_DNX_INSTALL%"=="1" goto run
CALL packages\KoreBuild\build\dnvm upgrade -unstable -runtime CLR -arch x86
CALL packages\KoreBuild\build\dnvm install default -runtime CoreCLR -arch x86

:run
CALL packages\KoreBuild\build\dnvm use default -runtime CLR -arch x86
powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& {Import-Module '.\tools\psake\psake.psm1'; invoke-psake .\default.ps1 %*; if ($lastexitcode -ne 0) {write-host "ERROR: $lastexitcode" -fore RED; exit $lastexitcode} }" 



