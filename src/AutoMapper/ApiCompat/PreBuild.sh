#!/bin/bash
version=$1
echo $version
readarray -d . -t versionNumbers <<< $version
if [[ ${versionNumbers[1]} -eq "0" && ${versionNumbers[2]} -eq "0" ]]
then
    oldVersion=$(({versionNumbers[0]} - 1))
else
    oldVersion=${versionNumbers[0]}
fi
oldVersion="$oldVersion.0.0"
echo $oldVersion
rm -rf ../LastMajorVersionBinary
curl https://globalcdn.nuget.org/packages/automapper.$oldVersion.nupkg --create-dirs -o ../LastMajorVersionBinary/automapper.$oldVersion.nupkg
unzip -j ../LastMajorVersionBinary/automapper.$oldVersion.nupkg lib/netstandard2.1/AutoMapper.dll -d ../LastMajorVersionBinary
