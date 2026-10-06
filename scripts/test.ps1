$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Add-Type -Path (Join-Path $root 'src\TravelRules.cs')
function Check($target, $current, $maximum, $online, $playing, $canSave, $busy, $allowed) {
    $errorText = [KingdomMod.FreeTravel.TravelRules]::Validate($target,$current,$maximum,$online,$playing,$canSave,$busy)
    if ([string]::IsNullOrEmpty($errorText) -ne $allowed) { throw "Unexpected travel permission for target=$target, current=$current." }
}
Check 5 0 5 $false $true $true $false $true
Check 1 4 5 $false $true $true $false $true
Check 0 0 5 $false $true $true $false $false
Check 6 0 5 $false $true $true $false $false
Check 1 0 5 $false $true $true $false $false
Check 2 0 5 $true $true $true $false $false
Check 2 0 5 $false $false $true $false $false
Check 2 0 5 $false $true $false $false $false
Check 2 0 5 $false $true $true $true $false
Write-Output 'PASS: 9 travel validation checks.'
foreach ($state in @('Pushing','WaitingForPlayer','WaitingForPassengers')) {
    if ([KingdomMod.FreeTravel.TravelRules]::UseBoatGroup($true,$state)) { throw "Unready boat $state must not block free travel." }
}
if ([KingdomMod.FreeTravel.TravelRules]::UseBoatGroup($false,'Sailing')) { throw 'No boat cannot select boat travel.' }
if (![KingdomMod.FreeTravel.TravelRules]::UseBoatGroup($true,'Sailing')) { throw 'Sailing boat should retain native boat travel.' }
Write-Output 'PASS: 5 boat selection checks.'
