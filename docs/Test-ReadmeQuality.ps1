[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$readmePath =
    Join-Path $Root "README.md"

if (-not (Test-Path -LiteralPath $readmePath)) {
    throw "README quality: README.md is missing."
}

$readme =
    [System.IO.File]::ReadAllText(
        $readmePath,
        [System.Text.Encoding]::UTF8)

if ($readme.Contains("## Documentation contract")) {
    throw "README quality: the internal Documentation contract section must not be exposed in README.md."
}

function Test-HasTrailingWhitespace([string]$Line) {
    if ($null -eq $Line -or $Line.Length -eq 0) {
        return $false
    }

    $last =
        $Line[$Line.Length - 1]

    return (
        $last -eq ' ' -or
        $last -eq [char]9)
}

if (Test-HasTrailingWhitespace '```') {
    throw "README quality self-test: Markdown fence must not be treated as trailing whitespace."
}

if (-not (Test-HasTrailingWhitespace "x ")) {
    throw "README quality self-test: trailing space was not detected."
}

$tabSelfTest =
    "x" + [char]9

if (-not (Test-HasTrailingWhitespace $tabSelfTest)) {
    throw "README quality self-test: trailing tab was not detected."
}

$readmeLines =
    $readme -split "`r?`n"

for ($index = 0; $index -lt $readmeLines.Count; $index++) {
    if (Test-HasTrailingWhitespace $readmeLines[$index]) {
        throw (
            "README quality: trailing whitespace at line {0}." -f
            ($index + 1))
    }
}

if ([regex]::IsMatch(
    $readme,
    '(?is)<td\b[^>]*>\s*</td>')) {
    throw "README quality: empty table cells are forbidden; use balanced cards or colspan."
}

if ([regex]::IsMatch(
    $readme,
    '(?is)<p>\s*<a\s+href="[^"]*/components/')) {
    throw "README quality: scientific component cards must live inside the Scientific components table."
}

$algorithmSection =
    [regex]::Match(
        $readme,
        '(?ms)^## All algorithms\s*(?<body>.*?)(?=^## Scientific components\s*$)')

if (-not $algorithmSection.Success) {
    throw "README quality: All algorithms section is missing or malformed."
}

foreach ($cell in [regex]::Matches(
    $algorithmSection.Groups["body"].Value,
    '(?is)<td\b[^>]*>(?<body>.*?)</td>')) {

    if (-not $cell.Groups["body"].Value.Contains('<a href=')) {
        throw "README quality: every algorithm card must have a clickable title."
    }
}

$componentSection =
    [regex]::Match(
        $readme,
        '(?ms)^## Scientific components\s*(?<body>.*)\z')

if (-not $componentSection.Success) {
    throw "README quality: Scientific components section is missing."
}

foreach ($cell in [regex]::Matches(
    $componentSection.Groups["body"].Value,
    '(?is)<td\b[^>]*>(?<body>.*?)</td>')) {

    if (-not $cell.Groups["body"].Value.Contains('<a href=')) {
        throw "README quality: every scientific component card must be clickable."
    }
}

foreach ($linkedTitle in @(
    "GRASP with Path Relinking",
    "Evolutionary Path Relinking",
    "CMA-ES Components",
    "Advanced Ant Colony Optimization",
    "Memetic Algorithm Components"
)) {
    $pattern =
        '<a\s+href="[^"]+"><strong>' +
        [regex]::Escape($linkedTitle) +
        '</strong></a>'

    if (-not [regex]::IsMatch(
        $readme,
        $pattern)) {
        throw "README quality: '$linkedTitle' is not presented as a clickable card."
    }
}

foreach ($marker in @(
    "artificial-bee-colony-karaboga-basturk-2007"
)) {
    if (-not $readme.Contains($marker)) {
        throw "README quality: expected marker '$marker' is missing."
    }
}

$catalog =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "docs\algorithm-catalog.json"),
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$algorithms =
    @($catalog.algorithms)

$publicCountMarker =
    "$($algorithms.Count) public algorithms"

if (-not $readme.Contains($publicCountMarker)) {
    throw "README quality: dynamic public-algorithm count marker '$publicCountMarker' is missing."
}

$swarmCount =
    @(
        $algorithms |
        Where-Object {
            [string]$_.category -eq "swarm-intelligence"
        }
    ).Count

$swarmCountMarker =
    "$swarmCount swarm methods"

if (-not $readme.Contains($swarmCountMarker)) {
    throw "README quality: dynamic swarm count marker '$swarmCountMarker' is missing."
}

$evolutionaryCount =
    @(
        $algorithms |
        Where-Object {
            [string]$_.category -eq "evolutionary-methods"
        }
    ).Count

$evolutionaryCountMarker =
    "$evolutionaryCount evolutionary methods"

if (-not $readme.Contains($evolutionaryCountMarker)) {
    throw "README quality: dynamic evolutionary count marker '$evolutionaryCountMarker' is missing."
}

foreach ($algorithm in $algorithms) {
    $id =
        [string]$algorithm.id

    if (-not $readme.Contains($id)) {
        throw "README quality: stable ID '$id' is missing."
    }

    $cardPattern =
        '(?is)<td\b[^>]*>' +
        '(?:(?!</td>).)*' +
        '<a\s+href="[^"]+"><strong>[^<]+</strong></a>' +
        '(?:(?!</td>).)*' +
        [regex]::Escape($id) +
        '(?:(?!</td>).)*' +
        '</td>'

    if (-not [regex]::IsMatch(
            $readme,
            $cardPattern)) {
        throw (
            "README quality: stable ID '$id' is not contained " +
            "in a table card with a clickable title.")
    }
}

Write-Host `
    "README quality validation passed: all cards clickable, tables balanced, components integrated and internal contract removed." `
    -ForegroundColor Green
