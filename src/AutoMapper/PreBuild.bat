rd /s /q ..\LastMajorVersionBinary
mkdir ..\LastMajorVersionBinary
..\..\nuget install AutoMapper -Version %1% -OutputDirectory ..\LastMajorVersionBinary
copy ..\LastMajorVersionBinary\AutoMapper.%1%\lib\netstandard2.0\AutoMapper.dll ..\LastMajorVersionBinary