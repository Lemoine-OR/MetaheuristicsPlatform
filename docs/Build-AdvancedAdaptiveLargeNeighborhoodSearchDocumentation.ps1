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
    Join-Path $Root "docs\advanced-adaptive-large-neighborhood-search-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Advanced ALNS documentation: component catalog is missing."
}

$catalog =
    [System.IO.File]::ReadAllText(
        $catalogPath,
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$componentDirectory =
    Join-Path $Site "components"

New-Item -ItemType Directory -Force -Path $componentDirectory | Out-Null

Copy-Item `
    -LiteralPath $catalogPath `
    -Destination (Join-Path $Site "advanced-adaptive-large-neighborhood-search-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.entries)) {
    $formulaBlock =
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
$formulaBlock
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
<title>Advanced ALNS Components &middot; MetaheuristicsPlatform</title>
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
<h1>Advanced Adaptive Large Neighborhood Search Components</h1>
<p>Pair-coupled roulette, alpha-UCB pair learning and reusable alternative acceptance criteria.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../advanced-adaptive-large-neighborhood-search-catalog.json">Open the advanced ALNS catalog</a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

Write-Utf8 `
    (Join-Path $componentDirectory "advanced-adaptive-large-neighborhood-search-components.html") `
    $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "Advanced ALNS documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

if (-not $homeContent.Contains(
    "components/advanced-adaptive-large-neighborhood-search-components.html")) {

    $componentsMarker =
        '<h2 id="components">Scientific components</h2>'

    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if ($gridStart -lt 0) {
        throw "Advanced ALNS documentation: scientific-components grid is missing."
    }

    $insertAt =
        $gridStart +
        '<div class="grid">'.Length

    $card = @"
<div class="card">
<h3><a href="components/advanced-adaptive-large-neighborhood-search-components.html">Advanced ALNS Components</a></h3>
<div class="meta">Pair-coupled roulette &middot; alpha-UCB pair learning &middot; alternative acceptance adapters</div>
<span class="id">alns.advanced.*</span>
</div>
"@

    $homeContent =
        $homeContent.Insert(
            $insertAt,
            "`n" + $card)
}

Write-Utf8 $homePath $homeContent
