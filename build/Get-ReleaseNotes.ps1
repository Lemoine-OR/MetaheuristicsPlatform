[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Version,
    [Parameter(Mandatory=$true)][string]$OutputPath
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($Version)) {
    $versionObject = [System.IO.File]::ReadAllText((Join-Path $Root "version.json"), [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    $Version = [string]$versionObject.version
}

$changelog = [System.IO.File]::ReadAllText((Join-Path $Root "CHANGELOG.md"), [System.Text.Encoding]::UTF8)
$startMarker = "## [$Version]"
$start = $changelog.IndexOf($startMarker, [System.StringComparison]::Ordinal)
if ($start -lt 0) { throw "CHANGELOG section not found for version $Version." }

$bodyStart = $start + $startMarker.Length
$next = $changelog.IndexOf("## [", $bodyStart, [System.StringComparison]::Ordinal)
if ($next -lt 0) { $next = $changelog.Length }

$body = $changelog.Substring($bodyStart, $next - $bodyStart).Trim()
if ([string]::IsNullOrWhiteSpace($body)) { throw "CHANGELOG section for $Version is empty." }

$notes = "# MetaheuristicsPlatform $Version`r`n`r`n$body`r`n"
$parent = Split-Path -Parent $OutputPath
if ($parent -and -not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
[System.IO.File]::WriteAllText($OutputPath, $notes, [System.Text.UTF8Encoding]::new($false))
Write-Output $OutputPath
