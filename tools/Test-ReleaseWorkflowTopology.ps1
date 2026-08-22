[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path =
        Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Release workflow topology: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$documentation =
    Read-Utf8 ".github\workflows\documentation.yml"

$release =
    Read-Utf8 ".github\workflows\release.yml"

# GitHub Actions expressions are literal workflow text here. In PowerShell
# source they must be single-quoted (or have the dollar sign escaped).
$githubExpressionLiteral =
    'ref: ${{ env.TARGET_SHA }}'

if (-not $githubExpressionLiteral.Contains('${{')) {
    throw "Release workflow topology self-test: GitHub expression literal was not preserved."
}

foreach ($marker in @(
    "workflow_run:",
    "- Build and Test",
    "branches:",
    "- main",
    "github.event.workflow_run.conclusion == 'success'",
    'ref: ${{ env.TARGET_SHA }}',
    "pull_request:",
    "workflow_dispatch:"
)) {
    if (-not $documentation.Contains($marker)) {
        throw "Release workflow topology: documentation workflow is missing '$marker'."
    }
}

if ([regex]::IsMatch(
    $documentation,
    '(?m)^  push:\s*$')) {
    throw "Release workflow topology: documentation must not run directly on main push; it must follow Build and Test."
}

foreach ($marker in @(
    "workflow_run:",
    "- Build Documentation",
    "branches:",
    "- main",
    "WORKFLOW_CONCLUSION:",
    "Release trigger workflow did not succeed",
    "workflow_dispatch:"
)) {
    if (-not $release.Contains($marker)) {
        throw "Release workflow topology: release workflow is missing '$marker'."
    }
}

if ($release.Contains("- Build and Test")) {
    throw "Release workflow topology: Create Release must not listen directly to Build and Test."
}

$automaticPrerequisiteMatches =
    [regex]::Matches(
        $release,
        '(?m)^\s+- Build Documentation\s*$')

if ($automaticPrerequisiteMatches.Count -ne 1) {
    throw (
        "Release workflow topology: expected exactly one automatic release prerequisite; " +
        "found $($automaticPrerequisiteMatches.Count).")
}

Write-Host `
    "Release workflow topology passed: Build and Test -> Build Documentation -> one automatic Create Release run." `
    -ForegroundColor Green
