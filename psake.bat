@echo off
cd %~dp0

SETLOCAL
SET CACHED_NUGET="%LocalAppData%\NuGet\NuGet.exe"

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto restore
echo Copying NuGet
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul

:restore
IF EXIST packages\KoreBuild goto run
echo Restoring...
.nuget\NuGet.exe install KoreBuild -ExcludeVersion -o packages -nocache -pre
.nuget\NuGet.exe install Sake -version 0.2 -o packages -ExcludeVersion

IF "%1" == "rebuild-package" goto run

IF "%SKIP_DNX_INSTALL%"=="1" (
    REM On the CI, don't upgrade since the previous installed DNVM is already there.
	echo Using default
    CALL packages\KoreBuild\build\dnvm use default -runtime CLR -arch x86
) ELSE (
	echo Upgrading
    CALL packages\KoreBuild\build\dnvm upgrade -Unstable -runtime CLR -arch x86
)

:run
CALL packages\KoreBuild\build\dnvm use default -runtime CLR -arch x86
powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& {Import-Module '.\tools\psake\psake.psm1'; invoke-psake .\default.ps1 %*; if ($lastexitcode -ne 0) {write-host "ERROR: $lastexitcode" -fore RED; exit $lastexitcode} }" 



