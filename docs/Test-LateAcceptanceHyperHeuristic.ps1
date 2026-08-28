[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

foreach ($marker in @(
        "## General description",
        "## Technical specifications",
        "## Complexity",
        "## Applicability",
        "## Detailed operation",
        "## Parameters",
        "## API example",
        "## Stable factory ID",
        "## Mathematical details",
        "### Problem formulation",
        "### Update equations / iterations",
        "### Assumptions",
        "### Convergence conditions",
        "### Scientific references",
        "late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009",
        "10.1109/CEC.2009.4983054",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009" -ForegroundColor Green
