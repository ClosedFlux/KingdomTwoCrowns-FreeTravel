param([Parameter(Mandatory)][string]$GameDirectory)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sdkRoot = Join-Path $env:ProgramFiles 'dotnet\sdk'
$sdk = Get-ChildItem -LiteralPath $sdkRoot -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$compiler = Join-Path $sdk.FullName 'Roslyn\bincore\csc.dll'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Install a .NET SDK before building.' }
$refs = @()
foreach ($relative in @('dotnet','BepInEx\core','BepInEx\interop')) {
    $folder = Join-Path $GameDirectory $relative
    if (!(Test-Path -LiteralPath $folder)) { throw "Missing game references: $relative. Start the game once with a compatible BepInEx IL2CPP installation." }
    $refs += Get-ChildItem -LiteralPath $folder -Filter '*.dll' -File
}
$refs = @($refs | Where-Object {
    if ($_.Name -eq 'UnityEngine.dll') { return $false }
    try { [Reflection.AssemblyName]::GetAssemblyName($_.FullName) | Out-Null; $true } catch { $false }
})
$outputDirectory = Join-Path $root 'dist\KingdomMod.FreeTravel'
$workDirectory = Join-Path $root 'work'
New-Item -ItemType Directory -Path $outputDirectory,$workDirectory -Force | Out-Null
$outputFile = Join-Path $outputDirectory 'KingdomMod.FreeTravel.dll'
$arguments = @('/nologo','/target:library','/nostdlib+','/unsafe+','/langversion:latest','/define:IL2CPP,BIE,BIE6','/optimize+','/debug:portable',("/out:`"$outputFile`""))
$arguments += $refs | ForEach-Object { "/reference:`"$($_.FullName)`"" }
$arguments += Get-ChildItem -LiteralPath (Join-Path $root 'src') -Filter '*.cs' -File | ForEach-Object { '"' + $_.FullName + '"' }
$responseFile = Join-Path $workDirectory 'FreeTravel.rsp'
$arguments | Set-Content -LiteralPath $responseFile -Encoding utf8
& dotnet $compiler "@$responseFile"
if ($LASTEXITCODE -ne 0) { throw 'FreeTravel compilation failed.' }
Write-Output "Compiled: $outputFile"
