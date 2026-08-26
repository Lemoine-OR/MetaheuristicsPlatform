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

$build =
    Read-Utf8 ".github\workflows\build.yml"

$documentation =
    Read-Utf8 ".github\workflows\documentation.yml"

$release =
    Read-Utf8 ".github\workflows\release.yml"

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
    throw "Release workflow topology: documentation must follow Build and Test."
}

foreach ($marker in @(
    "MetaheuristicsPlatform-Binaries",
    "src/MetaheuristicsPlatform/bin/Release/net10.0",
    "actions/upload-artifact@v7"
)) {
    if (-not $build.Contains($marker)) {
        throw "Release workflow topology: build workflow is missing validated-binary artifact marker '$marker'."
    }
}

foreach ($marker in @(
    "workflow_run:",
    "- Build Documentation",
    "branches:",
    "- main",
    "WORKFLOW_CONCLUSION:",
    "Release trigger workflow did not succeed",
    "workflow_dispatch:",
    "build_run_id:",
    "docs_run_id:",
    "MetaheuristicsPlatform-Binaries",
    "MetaheuristicsPlatform-Documentation",
    "gh run download",
    '-CommitId $env:TARGET_SHA'
)) {
    if (-not $release.Contains($marker)) {
        throw "Release workflow topology: release workflow is missing '$marker'."
    }
}

if ($release.Contains("Build-All.ps1") -or
    $release.Contains("Build-Validated.ps1") -or
    $release.Contains("build-documentation.ps1") -or
    $release.Contains("Install Doxygen") -or
    $release.Contains("Install Graphviz")) {

    throw "Release workflow topology: Create Release must consume exact-SHA validated artifacts and must not rebuild/retest/regenerate documentation."
}

if ($release.Contains("- Build and Test")) {
    throw "Release workflow topology: Create Release must not listen directly to Build and Test."
}

foreach ($marker in @(
    'select(.path==".github/workflows/build.yml")',
    'select(.path==".github/workflows/documentation.yml")',
    'docs_conclusion="$WORKFLOW_CONCLUSION"'
)) {
    if (-not $release.Contains($marker)) {
        throw "Release workflow topology: stable workflow identity marker '$marker' is missing."
    }
}

if ($release.Contains('select(.name=="Build and Test")') -or
    $release.Contains('select(.name=="Build Documentation")')) {

    throw "Release workflow topology: workflow runs must be identified by stable path, not display/run name."
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
    "Release workflow topology passed: Build -> Documentation -> artifact-only Release; no release-stage rebuild/retest/Doxygen." `
    -ForegroundColor Green
