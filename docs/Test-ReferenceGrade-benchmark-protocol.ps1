[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath = Join-Path $Root "docs\pages\reference-grade\benchmark-protocol.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\ReferenceGrade\BenchmarkProtocol.cs"

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
        "## Validation",        "reference-grade-benchmark-protocol",
        "v0.169.0"
)) {
    if (-not $page.Contains($marker)) {
        throw "Reference-grade page marker missing: $marker"
    }
}

foreach ($marker in @(
        "public sealed record BenchmarkProtocol",
        "BenchmarkResultEnvelope"
)) {
    if (-not $source.Contains($marker)) {
        throw "Reference-grade source marker missing: $marker"
    }
}

Write-Host "Reference-grade contract GREEN: reference-grade-benchmark-protocol" -ForegroundColor Green
