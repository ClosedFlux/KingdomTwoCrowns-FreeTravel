param([Parameter(Mandatory)][string]$GameDirectory)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sdk = Get-ChildItem (Join-Path $env:ProgramFiles 'dotnet\sdk') -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$refs = @(foreach($relative in @('dotnet','BepInEx\core','BepInEx\interop')) { Get-ChildItem (Join-Path $GameDirectory $relative) -Filter '*.dll' -File })
$refs += Get-Item (Join-Path $GameDirectory 'BepInEx\plugins\BepInExConfigManager\UniverseLib.BIE.IL2CPP.Interop.dll'),(Join-Path $GameDirectory 'BepInEx\patchers\BepInExConfigManager.Il2Cpp.Patcher.dll')
$refs = @($refs | Where-Object { if($_.Name -eq 'UnityEngine.dll') { return $false }; try { [Reflection.AssemblyName]::GetAssemblyName($_.FullName) | Out-Null; $true } catch { $false } })
$out = Join-Path $root 'dist\BepInExConfigManagerChinese'
New-Item -ItemType Directory -Path $out,(Join-Path $root 'work') -Force | Out-Null
$arguments = @('/nologo','/target:library','/nostdlib+','/unsafe+','/langversion:latest','/define:CPP,INTEROP','/optimize+','/debug:portable',("/out:`"$out\BepInExConfigManager.Il2Cpp.CoreCLR.dll`""))
$arguments += $refs | ForEach-Object { '/reference:"' + $_.FullName + '"' }
$arguments += Get-ChildItem (Join-Path $root 'src_plugin') -Recurse -Filter '*.cs' | Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' } | ForEach-Object { '"' + $_.FullName + '"' }
$response = Join-Path $root 'work\MenuChinese.rsp'
$arguments | Set-Content $response -Encoding utf8
& dotnet (Join-Path $sdk.FullName 'Roslyn\bincore\csc.dll') "@$response"
if($LASTEXITCODE -ne 0) { throw 'Chinese menu compilation failed.' }
Write-Output "Compiled Chinese menu: $out"
