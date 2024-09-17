#!/usr/bin/env pwsh

[CmdletBinding()]
param (
    [Parameter(Mandatory = $false,
        HelpMessage = "Path to secrets file")]
    [string]
    $SecretsFilePath,

    [Parameter(Mandatory = $false,
        HelpMessage = "Path to project file")]
    [string]
    $ProjectFilePath
)

if(-not $SecretsFilePath) {
    $files = (Get-ChildItem -Path "$PSScriptRoot/.." -Filter "secrets.*.json" -Recurse).FullName
    Write-Output "Found $($files.Count) secrets files. You have to pass one to the script as first parameter."
    foreach($file in $files) {
        Write-Output "> $file"
    }

    exit 0
}

if(-not $ProjectFilePath) {
    $csprojs = Get-ChildItem -Path "$PSScriptRoot/.." -Filter '*.csproj' -Recurse | 
                Where-Object { $_ | Select-String -Pattern '\<UserSecretsId\>' } |
                ForEach-Object { $_.FullName }
    Write-Output "Found $($csprojs.Count) projects with user secrets. You have to pass one to the script as second parameter."

    foreach($csproj in $csprojs) {
        Write-Output "> $csproj"
    }

    exit 0
}

if($PSVersionTable.OS -match 'linux') {
    cat $SecretsFilePath | dotnet user-secrets set --project $ProjectFilePath
}
else {
    $jsonText = Get-Content -Path $SecretsFilePath -Raw
    $jsonWithoutComments = $jsonText -replace '(?m)(?<=^([^"]|"[^"]*")*)//.*' -replace '(?ms)/\*.*?\*/' # https://stackoverflow.com/a/57092959
    $json = $jsonWithoutComments | ConvertFrom-Json
    foreach ($property in $json.PSObject.Properties) {
        dotnet user-secrets set $property.Name $property.Value --project $ProjectFilePath
    }
}
