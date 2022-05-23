# Needs to be at least that version, or mmm can't read the archiveme = (ls *.csproj).BaseName:vs)
#Requires -Modules @{ ModuleName="Microsoft.PowerShell.Archive"; ModuleVersion="1.2.3" }
$MyInvocation.MyCommand.Path | Split-Path | Push-Location # Run from this script's directory
$Name = (ls *.csproj).BaseName
dotnet build -c Release

curl -L https://github.com/Graicc/GorillaKZ/releases/download/v0.1.0/GorillaKZ-v0.1.0.zip -o DL.zip
Expand-Archive DL.zip 
rm DL.zip
mv DL\BepInEx .

cp .\Resources\BepInEx\ . -R -F
cp bin\Release\netstandard2.0\$Name.dll BepInEx\plugins\$Name\
Compress-Archive .\BepInEx\ $Name-v
rmdir .\BepInEx\ -Recurse
