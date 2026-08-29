[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath = Join-Path $Root "docs\pages\reference-grade\scientific-reference-integrity.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\ReferenceGrade\ScientificReferenceIntegrity.cs"

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
        "## Validation",        "reference-grade-scientific-reference-integrity",
        "v0.172.0"
)) {
    if (-not $page.Contains($marker)) {
        throw "Reference-grade page marker missing: $marker"
    }
}

foreach ($marker in @(
        "public static class ScientificReferenceIntegrity",
        "NormalizeDoi"
)) {
    if (-not $source.Contains($marker)) {
        throw "Reference-grade source marker missing: $marker"
    }
}

Write-Host "Reference-grade contract GREEN: reference-grade-scientific-reference-integrity" -ForegroundColor Green
