$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$rules = Get-Content -Raw (Join-Path $root 'src/AutoCollect/CollectionRules.cs')
$tests = Get-Content -Raw (Join-Path $root 'tests/CollectionTests.cs')
Add-Type -TypeDefinition ($rules + "`n" + $tests)
$n = [CollectionTests]::Run()
Write-Output "PASS: $n automatic collection checks"
