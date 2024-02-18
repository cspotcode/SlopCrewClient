cd $PSScriptRoot/..
dotnet build

$pluginDir = "$env:BepInExDirectory/plugins/aaa-SlopCrewClient"
if(-not (test-path $pluginDir)) { mkdir $pluginDir }
cp ./Build/SlopCrewClient/* $pluginDir/
