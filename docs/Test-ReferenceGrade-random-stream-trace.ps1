[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath = Join-Path $Root "docs\pages\reference-grade\random-stream-trace.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\ReferenceGrade\ReferenceRandomStreamTrace.cs"

foreach ($path in @($pagePath,$sourcePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Reference-grade consolidation file missing: $path"
    }
}

$page = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
$source = [System.IO.File]::ReadAllText($sourcePath,[System.Text.Encoding]::UTF8)

foreach ($marker in @(
        "## Purpose",
        "## Contract",
        "## Invariants",
        "## API example",
        "## Failure modes",
        "## Stability guarantee",
        "## Versioning rule",
        "## Validation",        "reference-grade-random-stream-trace",
        "v0.171.0"
)) {
    if (-not $page.Contains($marker)) {
        throw "Reference-grade page marker missing: $marker"
    }
}

foreach ($marker in @(
        "public sealed class ReferenceRandomStreamTrace",
        "DeriveSeed"
)) {
    if (-not $source.Contains($marker)) {
        throw "Reference-grade source marker missing: $marker"
    }
}

Write-Host "Reference-grade contract GREEN: reference-grade-random-stream-trace" -ForegroundColor Green
