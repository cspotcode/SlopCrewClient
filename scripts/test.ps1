cd $PSScriptRoot/..
dotnet build

$pluginDir = "$env:BepInExDirectory/plugins/aaa-SlopCrewClient"
test-path $pluginDir || mkdir $pluginDir
cp ./Build/SlopCrewClient/* $pluginDir/
