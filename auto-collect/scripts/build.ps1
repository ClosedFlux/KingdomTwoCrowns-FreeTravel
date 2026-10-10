param([string]$GameDirectory = 'E:\Steam\steamapps\common\Kingdom Two Crowns')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$codeRoot = Join-Path $projectRoot 'src'
New-Item -ItemType Directory -Path (Join-Path $projectRoot 'work') -Force | Out-Null
$sdkRoot = Join-Path $env:ProgramFiles 'dotnet\sdk'
$sdk = Get-ChildItem -LiteralPath $sdkRoot -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$compiler = Join-Path $sdk.FullName 'Roslyn\bincore\csc.dll'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Install a .NET SDK before building.' }
$refs = @()
foreach ($folder in @('dotnet','BepInEx\core','BepInEx\interop')) {
    $refs += Get-ChildItem -LiteralPath (Join-Path $GameDirectory $folder) -Filter '*.dll' -File
}

$refs = @($refs | Where-Object { if ($_.Name -eq 'UnityEngine.dll') { return $false }; try { [Reflection.AssemblyName]::GetAssemblyName($_.FullName) | Out-Null; $true } catch { $false } })
$mod = 'AutoCollect'
$outDir = Join-Path $projectRoot 'dist/KingdomMod.AutoCollect'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$lines = @('/nologo','/target:library','/nostdlib+','/unsafe+','/langversion:latest','/optimize+',("/out:`"$outDir/KingdomMod.AutoCollect.dll`""))
$lines += $refs | ForEach-Object { "/reference:`"$($_.FullName)`"" }
$lines += Get-ChildItem (Join-Path $codeRoot $mod) -Filter '*.cs' | ForEach-Object { '"' + $_.FullName + '"' }
$response = Join-Path $projectRoot 'work/AutoCollect.rsp'
$lines | Set-Content -LiteralPath $response -Encoding utf8
& dotnet $compiler "@$response"
if ($LASTEXITCODE -ne 0) { throw 'AutoCollect compilation failed.' }
Write-Output "AutoCollect compiled: $outDir"
