[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-DocumentationTextWithoutCodeFences(
    [string]$Path) {

    $text =
        [System.IO.File]::ReadAllText(
            $Path,
            [System.Text.Encoding]::UTF8)

    return [regex]::Replace(
        $text,
        '(?ms)```.*?```',
        '')
}

function Get-InvalidDoxygenMathEscapes(
    [string]$Text) {

    $patterns =
        @(
            # Doxygen inline/display math delimiters must be \f$, \f[, \f].
            '\\\\f(?:\$|\[|\])',

            # TeX commands inside those formulas use one command backslash.
            # Two backslashes are reserved for TeX line breaks and are valid
            # only when they are not immediately starting another command.
            '\\\\(?:begin|end|min|max|argmin|argmax|mathcal|mathbb|operatorname|text|prod|sum|quad|qquad|sim|in|subseteq|supseteq|prec|ge|le|ldots|cdot|frac|sqrt)\b'
        )

    $found =
        New-Object System.Collections.Generic.List[string]

    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches(
            $Text,
            $pattern)) {

            $found.Add(
                [string]$match.Value)
        }
    }

    return @($found)
}

# Regression self-tests:
# single command backslashes + a genuine TeX line break are valid.
$validFixture =
    '\f[\begin{aligned}x&=1,\\y&=2\end{aligned}\f]'

if (@(Get-InvalidDoxygenMathEscapes $validFixture).Count -ne 0) {
    throw "Doxygen markup safety self-test: valid Doxygen/TeX mathematics was rejected."
}

$invalidFixture =
    '\\f[\\begin{aligned}x=1\\end{aligned}\\f]'

if (@(Get-InvalidDoxygenMathEscapes $invalidFixture).Count -lt 2) {
    throw "Doxygen markup safety self-test: doubled Doxygen/TeX escaping was not detected."
}

$files =
    @(
        Join-Path $Root "docs\mainpage.md"
    ) +
    @(
        Get-ChildItem `
            -LiteralPath (Join-Path $Root "docs\pages") `
            -Recurse `
            -File `
            -Filter "*.md" |
        ForEach-Object {
            $_.FullName
        }
    )

foreach ($path in $files) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        continue
    }

    $text =
        Get-DocumentationTextWithoutCodeFences `
            -Path $path

    $invalid =
        @(Get-InvalidDoxygenMathEscapes $text)

    if ($invalid.Count -ne 0) {
        $relative =
            $path.Substring($Root.Length).TrimStart('\')

        throw (
            (
                "Doxygen markup safety: '{0}' contains doubled Doxygen/TeX command escaping: {1}. " +
                "PowerShell here-strings preserve backslashes literally; use one backslash for commands " +
                "and two only for TeX line breaks."
            ) -f
            $relative,
            (($invalid | Sort-Object -Unique) -join ", "))
    }
}

Write-Host `
    "Doxygen markup safety passed: command backslashes are canonical and TeX line breaks remain valid." `
    -ForegroundColor Green