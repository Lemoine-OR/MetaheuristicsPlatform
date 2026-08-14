[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$versionJson =
    Get-Content (Join-Path $Root "version.json") -Raw |
    ConvertFrom-Json

$releaseVersion =
    [string]$versionJson.version

$tag =
    "v$releaseVersion"

$release =
    Join-Path $Root "Documentation\release"

if (Test-Path $release) {
    Remove-Item $release -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $release | Out-Null

$binaryRoot =
    Join-Path $Root "src\MetaheuristicsPlatform\bin\Release\net10.0"

if (-not (Test-Path $binaryRoot)) {
    throw "Release binary directory does not exist. Run build/Build-All.ps1 first."
}

$binaryZip =
    Join-Path $release "MetaheuristicsPlatform-$releaseVersion-binaries.zip"

$documentationZip =
    Join-Path $release "MetaheuristicsPlatform-$releaseVersion-documentation.zip"

Compress-Archive `
    -Path (Join-Path $binaryRoot "*") `
    -DestinationPath $binaryZip `
    -Force

Compress-Archive `
    -Path (Join-Path $Root "Documentation\site\*") `
    -DestinationPath $documentationZip `
    -Force

$files = @(
    Get-ChildItem $release -File
)

foreach ($file in $files) {
    $hash =
        (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()

    "$hash  $($file.Name)" |
        Set-Content `
            -Path "$($file.FullName).sha256" `
            -Encoding utf8
}

$commit =
    (& git rev-parse HEAD 2>$null | Select-Object -First 1)

if ([string]::IsNullOrWhiteSpace($commit)) {
    $commit = "unknown"
}

[pscustomobject]@{
    ReleaseVersion = $releaseVersion
    BuildVersion = $releaseVersion
    Tag = $tag
    CommitId = $commit
    Prerelease = $releaseVersion.Contains("-")
}
