$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$dll = Join-Path $project 'bin\Release\netstandard2.1\HDSnapshot.dll'

if (!(Test-Path -LiteralPath $dll)) {
    Write-Host "Building Release project..."
    & dotnet build (Join-Path $project 'HDSnapshot.csproj') -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

[xml]$metadata = Get-Content -LiteralPath (Join-Path $project 'HDSnapshot.csproj') -Raw
$version = $metadata.Project.PropertyGroup.Version
$dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant()

$release = Join-Path $project 'Releases'
New-Item -ItemType Directory -Path $release -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$zipPath = Join-Path $release "KingdomInc-HDSnapshot-$version.zip"
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

$entries = [ordered]@{
    'HD Snapshot requirements.txt' = (Join-Path $project 'Nexus\REQUIREMENTS.txt')
    'BepInEx/plugins/HDSnapshot/HDSnapshot.dll' = $dll
    'HD Snapshot README.txt' = (Join-Path $project 'README.md')
}

$archive = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($entry in $entries.GetEnumerator()) {
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $entry.Value, $entry.Key) | Out-Null
    }
} finally {
    $archive.Dispose()
}

$extracted = Join-Path $project ('Verification\Package-' + [Guid]::NewGuid().ToString('N'))
[IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $extracted)
$actualFiles = @(Get-ChildItem -LiteralPath $extracted -Recurse -File)
if ($actualFiles.Count -ne $entries.Count) { throw 'Unexpected archive entries count.' }

foreach ($entry in $entries.GetEnumerator()) {
    $extractedItem = Join-Path $extracted $entry.Key
    if (!(Test-Path -LiteralPath $extractedItem)) { throw "Missing expected entry in archive: $($entry.Key)" }
    if ((Get-FileHash -LiteralPath $extractedItem).Hash -ne (Get-FileHash -LiteralPath $entry.Value).Hash) {
        throw "Clean extraction differs: $($entry.Key)"
    }
}

Remove-Item -LiteralPath $extracted -Recurse -Force

$record = [ordered]@{
    version = $version
    archive = $zipPath
    bytes = (Get-Item -LiteralPath $zipPath).Length
    sha256 = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    dllSha256 = $dllHash
    cleanExtractionPassed = $true
    gameAssemblySha256 = (Get-FileHash -LiteralPath (Join-Path $project '..\..\KingdomInc_Data\Managed\Assembly-CSharp.dll')).Hash.ToLowerInvariant()
    publication = 'Local only'
}

$record | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $release 'RELEASE_VERIFICATION.json') -Encoding utf8
$record | ConvertTo-Json -Depth 4
Write-Host "Package successfully created and verified: $zipPath"
