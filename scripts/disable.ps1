param([Parameter(Mandatory)][string]$GameDirectory)
$ErrorActionPreference = 'Stop'
if (Get-Process -Name KingdomTwoCrowns -ErrorAction SilentlyContinue) { throw 'Save your game and exit before disabling.' }
$active = Join-Path $GameDirectory 'BepInEx\plugins\KingdomMod.FreeTravel\KingdomMod.FreeTravel.dll'
if (!(Test-Path -LiteralPath $active)) { Write-Output 'FreeTravel is already disabled.'; exit 0 }
$disabled = Join-Path $GameDirectory 'BepInEx\disabled-plugins'
New-Item -ItemType Directory -Path $disabled -Force | Out-Null
$target = Join-Path $disabled ('KingdomMod.FreeTravel-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.dll')
Move-Item -LiteralPath $active -Destination $target
Write-Output 'FreeTravel disabled. UI plugins and game saves were not changed.'
