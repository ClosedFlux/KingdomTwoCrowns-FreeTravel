param([string]$GameDirectory = 'E:\Steam\steamapps\common\Kingdom Two Crowns')
$ErrorActionPreference = 'Stop'
if (Get-Process -Name KingdomTwoCrowns -ErrorAction SilentlyContinue) { throw 'Save your game and exit before installing.' }
if (!(Test-Path -LiteralPath (Join-Path $GameDirectory 'BepInEx/plugins/KingdomUnlimitedWallet/KingdomUnlimitedWallet.dll'))) { throw 'Install Unlimited Wallet before AutoCollect.' }
$root = Split-Path $PSScriptRoot -Parent
$built = Join-Path $root 'dist\KingdomMod.AutoCollect\KingdomMod.AutoCollect.dll'
if (!(Test-Path -LiteralPath $built)) { throw 'Build AutoCollect before installing.' }
$backup = Join-Path $root ('backups\AutoCollect-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $backup -Force | Out-Null
$saves = Join-Path ([Environment]::GetFolderPath('UserProfile')) 'AppData\LocalLow\noio\KingdomTwoCrowns\Release'
if (!(Test-Path -LiteralPath $saves)) { throw 'Expected local game saves were not found. Locate them before installing.' }
Copy-Item -LiteralPath $saves -Destination (Join-Path $backup 'OriginalSaves') -Recurse
$fileCount = @(Get-ChildItem -LiteralPath $saves -Recurse -File).Count
$savedCount = @(Get-ChildItem -LiteralPath (Join-Path $backup 'OriginalSaves') -Recurse -File).Count
if ($fileCount -eq 0 -or $fileCount -ne $savedCount) { throw 'Save backup is incomplete.' }
foreach ($file in Get-ChildItem -LiteralPath $saves -Recurse -File) {
    $relative = [IO.Path]::GetRelativePath($saves, $file.FullName)
    if ((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath (Join-Path $backup "OriginalSaves\$relative")).Hash) { throw 'Save backup checksum mismatch.' }
}
$activeDirectory = Join-Path $GameDirectory 'BepInEx\plugins\KingdomMod.AutoCollect'
if (Test-Path -LiteralPath $activeDirectory) { Copy-Item -LiteralPath $activeDirectory -Destination (Join-Path $backup 'PreviousPlugin') -Recurse }
$configPath = Join-Path $GameDirectory 'BepInEx\config\KingdomMod.AutoCollect.cfg'
if (Test-Path -LiteralPath $configPath) { Copy-Item -LiteralPath $configPath -Destination $backup }
New-Item -ItemType Directory -Path $activeDirectory -Force | Out-Null
$active = Join-Path $activeDirectory 'KingdomMod.AutoCollect.dll'
Copy-Item -LiteralPath $built -Destination $active -Force
if ((Get-FileHash -LiteralPath $built).Hash -ne (Get-FileHash -LiteralPath $active).Hash) { throw 'Installed plugin checksum mismatch.' }
$backup | Set-Content -LiteralPath (Join-Path $root 'work\auto-collect-backup.txt')
Write-Output "AutoCollect installed and verified; $fileCount save files backed up: $backup"
