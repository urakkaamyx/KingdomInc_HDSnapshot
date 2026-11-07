param([string]$GameRoot = (Join-Path $PSScriptRoot '..\..'), [switch]$StageForNextLaunch)
$ErrorActionPreference = 'Stop'
$GameRoot = (Resolve-Path -LiteralPath $GameRoot).Path
if (!(Test-Path -LiteralPath (Join-Path $GameRoot 'KingdomInc.exe'))) { throw 'Choose the Kingdom Inc game directory.' }
$gameExe = Join-Path $GameRoot 'KingdomInc.exe'
$gameRunning = [bool](Get-CimInstance Win32_Process -Filter "Name='KingdomInc.exe'" | Where-Object { $_.ExecutablePath -eq $gameExe })
if ($gameRunning -and !$StageForNextLaunch) { throw 'Close Kingdom Inc before installing HD Snapshot, or use -StageForNextLaunch for a first installation.' }
$source = Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\HDSnapshot.dll'
if (!(Test-Path -LiteralPath $source)) { throw 'Build the Release project first.' }
$destination = Join-Path $GameRoot 'BepInEx\plugins\HDSnapshot'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$target = Join-Path $destination 'HDSnapshot.dll'
if (Test-Path -LiteralPath $target) {
    if ($gameRunning) { throw 'An installed DLL already exists. Close Kingdom Inc before replacing it.' }
    $backup = Join-Path $PSScriptRoot ('_Backup\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    New-Item -ItemType Directory -Path $backup -Force | Out-Null
    Copy-Item -LiteralPath $target -Destination $backup
}
Copy-Item -LiteralPath $source -Destination $target -Force
if ((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $target).Hash) { throw 'Installed DLL verification failed.' }
Write-Output "Installed HD Snapshot: $target"
if ($gameRunning) { Write-Output 'First installation staged. Restart Kingdom Inc to load HD Snapshot.' }

