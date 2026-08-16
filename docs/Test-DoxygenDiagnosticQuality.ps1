[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Doxygen quality validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

$versionText = [version]([string]$version.version)

if ($versionText -lt [version]"0.27.1") {
    throw "Doxygen quality validation: expected repository version 0.27.1 or later."
}

$doxyfile = Read-Utf8 "docs\Doxyfile"

foreach ($marker in @(
    'FILTER_PATTERNS',
    'Doxygen-CSharpCompatibilityFilter.ps1',
    'WARN_AS_ERROR',
    'FAIL_ON_WARNINGS',
    'WARN_LOGFILE'
)) {
    if (-not $doxyfile.Contains($marker)) {
        throw "Doxygen quality validation: Doxyfile is missing '$marker'."
    }
}

$filter =
    Read-Utf8 "docs\Doxygen-CSharpCompatibilityFilter.ps1"

foreach ($marker in @(
    'record(?!\s+struct\b)',
    'required\s+',
    'init\s*;',
    'GuidedDescentResult'
)) {
    if (-not $filter.Contains($marker)) {
        throw "Doxygen quality validation: C# compatibility filter is missing '$marker'."
    }
}

$build =
    Read-Utf8 "docs\build-documentation.ps1"

foreach ($marker in @(
    'doxygen-build.log',
    'Doxygen emitted diagnostics',
    '(?i)(^|:\s)(warning|error):'
)) {
    if (-not $build.Contains($marker)) {
        throw "Doxygen quality validation: documentation build is missing '$marker'."
    }
}

$markdownFiles =
    @(
        Get-ChildItem `
            -LiteralPath (Join-Path $Root "docs\pages") `
            -Recurse `
            -File `
            -Filter "*.md"
    )

$mainpage =
    Join-Path $Root "docs\mainpage.md"

if (Test-Path -LiteralPath $mainpage) {
    $markdownFiles += Get-Item -LiteralPath $mainpage
}

$bareDisplayOpen =
    [regex]'(?m)^[ \t]*\\\[[ \t]*$'

$bareDisplayClose =
    [regex]'(?m)^[ \t]*\\\][ \t]*$'

$legacyInline =
    [regex]'\\\([^\r\n]*?\\\)'

foreach ($file in $markdownFiles) {
    $text =
        [System.IO.File]::ReadAllText(
            $file.FullName,
            [System.Text.Encoding]::UTF8)

    if ($bareDisplayOpen.IsMatch($text) -or
        $bareDisplayClose.IsMatch($text)) {
        throw (
            "Doxygen quality validation: legacy display-math delimiter remains in '{0}'." -f
            $file.FullName)
    }

    if ($legacyInline.IsMatch($text)) {
        throw (
            "Doxygen quality validation: legacy inline-math delimiter remains in '{0}'." -f
            $file.FullName)
    }
}

Write-Host `
    "Doxygen quality validation passed: compatibility filter wired, strict diagnostics enabled, Markdown math normalized." `
    -ForegroundColor Green
