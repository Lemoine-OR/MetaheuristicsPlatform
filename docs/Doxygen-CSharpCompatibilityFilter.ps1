[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$InputFile
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$text =
    [System.IO.File]::ReadAllText(
        $InputFile,
        [System.Text.Encoding]::UTF8)

# Doxygen 1.17 currently mis-scopes a small set of modern C# record/property
# constructs in this repository. This filter changes only Doxygen's input.
# The compiled C# source remains untouched.
$text =
    [regex]::Replace(
        $text,
        '\brecord(?!\s+struct\b)\b',
        'class')

$text =
    [regex]::Replace(
        $text,
        '\brequired\s+',
        '')

$text =
    [regex]::Replace(
        $text,
        '\binit\s*;',
        'set;')

# Doxygen 1.17 mis-associates members of the private nested GuidedDescentResult
# helper with the enclosing generic GLS optimizer. The helper is private,
# appears at the end of the source file, and is not part of the public API.
# Remove only this private helper from Doxygen's input stream while preserving
# the real compiled C# source unchanged.
if ($InputFile.EndsWith(
    "GuidedLocalSearchOptimizer.cs",
    [System.StringComparison]::OrdinalIgnoreCase)) {

    $nestedHelperPattern =
        '(?s)\r?\n    private readonly struct GuidedDescentResult\s*\{.*?\r?\n    \}\r?\n\}\s*$'

    $filtered =
        [regex]::Replace(
            $text,
            $nestedHelperPattern,
            "`n}")

    if ($filtered -eq $text) {
        throw "Doxygen C# filter could not isolate private GuidedDescentResult helper."
    }

    $text = $filtered
}

[Console]::OutputEncoding =
    [System.Text.UTF8Encoding]::new($false)

[Console]::Write($text)
