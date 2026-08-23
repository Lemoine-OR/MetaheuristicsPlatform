[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Site = (Join-Path (Split-Path -Parent $PSScriptRoot) "Documentation\site")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Html([string]$Value) {
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

function Write-Utf8([string]$Path,[string]$Content) {
    $directory = Split-Path -Parent $Path

    if ($directory -and
        -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        [System.Text.UTF8Encoding]::new($false))
}

$catalogPath =
    Join-Path $Root "docs\adaptive-large-neighborhood-search-component-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Adaptive LNS documentation: component catalog is missing."
}

$catalog =
    [System.IO.File]::ReadAllText(
        $catalogPath,
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$componentDirectory =
    Join-Path $Site "components"

New-Item `
    -ItemType Directory `
    -Force `
    -Path $componentDirectory |
    Out-Null

Copy-Item `
    -LiteralPath $catalogPath `
    -Destination (Join-Path $Site "adaptive-large-neighborhood-search-component-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.entries)) {
    $model =
        if ([string]$entry.formulaMode -eq "math") {
            '<div class="math">\[' +
            (Html ([string]$entry.formula)) +
            '\]</div>'
        }
        else {
            '<div class="formula-note">' +
            (Html ([string]$entry.formula)) +
            '</div>'
        }

    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">$(Html ([string]$entry.status))</span></h3>
<div class="meta">$(Html ([string]$entry.description))</div>
$model
<div class="meta">DOI: <code>$(Html ([string]$entry.doi))</code></div>
<span class="id">$(Html ([string]$entry.id))</span>
</div>
"@)
}

$page = @"
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Adaptive Large Neighborhood Search Components &middot; MetaheuristicsPlatform</title>
<link rel="stylesheet" href="../assets/site.css">
<script>
window.MathJax = { tex: { inlineMath: [['\\(','\\)']], displayMath: [['\\[','\\]']] } };
</script>
<script defer src="https://cdn.jsdelivr.net/npm/mathjax@3.2.2/es5/tex-chtml.js"></script>
</head>
<body>
<header><div class="wrap">
<div class="brand"><a href="../index.html"><img src="../assets/metaheuristicsplatform-logo.svg" alt="MetaheuristicsPlatform"></a></div>
<nav>
<a href="../index.html">Home</a>
<a href="../index.html#algorithms">Algorithms</a>
<a href="../index.html#families">Families</a>
<a href="../api/index.html">API</a>
<a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform">GitHub</a>
</nav>
</div></header>
<main class="wrap">
<h1>Adaptive Large Neighborhood Search Components</h1>
<p>Four executable canonical ALNS control mechanisms and three advanced extensions kept scientifically distinct.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../adaptive-large-neighborhood-search-component-catalog.json">Open <code>adaptive-large-neighborhood-search-component-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "adaptive-large-neighborhood-search-components.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "Adaptive LNS documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

$homeCard = @"
<div class="card">
<h3><a href="components/adaptive-large-neighborhood-search-components.html">Adaptive Large Neighborhood Search Components</a></h3>
<div class="meta">4 executable canonical adaptive mechanisms &middot; 3 advanced extensions reviewed separately</div>
<span class="id">alns.*</span>
</div>
"@

$componentsMarker =
    '<h2 id="components">Scientific components</h2>'

if (-not $homeContent.Contains(
    "components/adaptive-large-neighborhood-search-components.html")) {

    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if ($gridStart -lt 0) {
        throw "Adaptive LNS documentation: scientific-components grid is missing."
    }

    $insertAt =
        $gridStart +
        '<div class="grid">'.Length

    $homeContent =
        $homeContent.Insert(
            $insertAt,
            "`n" + $homeCard)
}

Write-Utf8 $homePath $homeContent

$algorithmPath =
    Join-Path $Site "algorithms\adaptive-large-neighborhood-search-ropke-pisinger-2006.html"

if (-not (Test-Path -LiteralPath $algorithmPath)) {
    throw "Adaptive LNS documentation: generated algorithm page is missing."
}

$algorithmHtml =
    [System.IO.File]::ReadAllText(
        $algorithmPath,
        [System.Text.Encoding]::UTF8)

if (-not $algorithmHtml.Contains(
    "../components/adaptive-large-neighborhood-search-components.html")) {

    $headingMarker =
        '<h2>Mathematical details</h2>'

    $headingIndex =
        $algorithmHtml.IndexOf($headingMarker)

    if ($headingIndex -lt 0) {
        throw "Adaptive LNS documentation: mathematical-details heading is missing."
    }

    $sectionIndex =
        $algorithmHtml.LastIndexOf(
            '<div class="section">',
            $headingIndex)

    if ($sectionIndex -lt 0) {
        throw "Adaptive LNS documentation: mathematical-details section is missing."
    }

    $linkBlock =
        '<div class="section"><h2>Adaptive LNS component catalog</h2>' +
        '<p><a href="../components/adaptive-large-neighborhood-search-components.html">' +
        '<strong>Open the ALNS component catalog</strong></a></p></div>'

    $algorithmHtml =
        $algorithmHtml.Insert(
            $sectionIndex,
            $linkBlock + "`n")
}

Write-Utf8 $algorithmPath $algorithmHtml
