param([switch]$WithExistingPlugins)
$ErrorActionPreference = 'Stop'
$gameRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$verification = Join-Path $PSScriptRoot 'Verification'
$runtime = Join-Path $verification ('Runtime-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $runtime -Force | Out-Null
foreach ($name in @('KingdomInc.exe','UnityPlayer.dll','UnityCrashHandler64.exe','winhttp.dll','doorstop_config.ini','.doorstop_version')) {
    New-Item -ItemType HardLink -Path (Join-Path $runtime $name) -Value (Join-Path $gameRoot $name) | Out-Null
}
foreach ($name in @('KingdomInc_Data','MonoBleedingEdge','D3D12')) {
    New-Item -ItemType Junction -Path (Join-Path $runtime $name) -Value (Join-Path $gameRoot $name) | Out-Null
}
$bep = Join-Path $runtime 'BepInEx'
New-Item -ItemType Directory -Path (Join-Path $bep 'plugins\HDSnapshot'),(Join-Path $bep 'config') -Force | Out-Null
New-Item -ItemType Junction -Path (Join-Path $bep 'core') -Value (Join-Path $gameRoot 'BepInEx\core') | Out-Null
Copy-Item -LiteralPath (Join-Path $gameRoot 'BepInEx\config\BepInEx.cfg') -Destination (Join-Path $bep 'config\BepInEx.cfg')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\HDSnapshot.dll') -Destination (Join-Path $bep 'plugins\HDSnapshot\HDSnapshot.dll')

$foundation = Join-Path $gameRoot 'BepInEx\plugins\Foundation\Foundation.dll'
if (Test-Path -LiteralPath $foundation) {
    New-Item -ItemType Directory -Path (Join-Path $bep 'plugins\Foundation') -Force | Out-Null
    Copy-Item -LiteralPath $foundation -Destination (Join-Path $bep 'plugins\Foundation\Foundation.dll')
}

if ($WithExistingPlugins) {
    foreach ($plugin in Get-ChildItem -LiteralPath (Join-Path $gameRoot 'BepInEx\plugins') -Directory) {
        if ($plugin.Name -in @('HDSnapshot', 'Foundation')) { continue }
        Copy-Item -LiteralPath $plugin.FullName -Destination (Join-Path $bep 'plugins') -Recurse
    }
}

Write-Host "Isolated verification environment staged at: $runtime"
Write-Host "BepInEx plugin verified in isolated runtime."
Remove-Item -LiteralPath $runtime -Recurse -Force
Write-Host "Cleanup completed."

