[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Site = (Join-Path (Split-Path -Parent $PSScriptRoot) "Documentation\site")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Memetic documentation: missing '$Path'."
    }

    return [System.IO.File]::ReadAllText(
        $Path,
        [System.Text.Encoding]::UTF8)
}

function Write-Utf8([string]$Path,[string]$Text) {
    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $Text,
        [System.Text.UTF8Encoding]::new($false))
}

function Html([string]$Value) {
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

$catalogPath =
    Join-Path $Root "docs\memetic-algorithm-catalog.json"

$catalog =
    (Read-Utf8 $catalogPath) |
    ConvertFrom-Json

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.entries)) {
    $status = [string]$entry.status

    $badge =
        if ($status -eq "implemented") {
            "implemented"
        }
        else {
            "reviewed / deferred"
        }

    $formulaMode = [string]$entry.formulaMode
    $formula = [string]$entry.formula
    $formulaEncoded = Html $formula

    $formulaHtml =
        if ($formulaMode -eq "math") {
            '<div class="math">\[' + $formulaEncoded + '\]</div>'
        }
        else {
            '<div class="formula-note">' + $formulaEncoded + '</div>'
        }

    $card = @"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">$badge</span></h3>
<div class="meta">$(Html ([string]$entry.kind))</div>
$formulaHtml
<span class="id">$(Html ([string]$entry.id))</span>
</div>
"@

    $cards.Add($card)
}

$page = @"
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Memetic Algorithm Components &middot; MetaheuristicsPlatform</title>
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
<h1>Memetic Algorithm Components</h1>
<p>Version 0.43.0 provides five executable local-improvement application policies and two executable learning policies. Self-adaptive meme choice and additional population-engine wiring remain explicitly reviewed/deferred rather than being approximated.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Scientific documentation</h2>
<p><a href="../api/memetic_algorithm_components.html"><strong>Open the complete Doxygen page</strong></a></p>
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../memetic-algorithm-catalog.json"><code>memetic-algorithm-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body>
</html>
"@

$componentDirectory =
    Join-Path $Site "components"

Write-Utf8 `
    (Join-Path $componentDirectory "memetic-algorithm-components.html") `
    $page

$catalogTarget =
    Join-Path $Site "memetic-algorithm-catalog.json"

Write-Utf8 `
    $catalogTarget `
    (Read-Utf8 $catalogPath)

$homePath =
    Join-Path $Site "index.html"

$homeHtml =
    Read-Utf8 $homePath

if (-not $homeHtml.Contains(
    "components/memetic-algorithm-components.html")) {

    $marker =
        '<h2 id="components">Scientific components</h2>'

    $markerIndex =
        $homeHtml.IndexOf($marker)

    if ($markerIndex -lt 0) {
        throw "Memetic documentation: Scientific components marker is missing."
    }

    $gridStart =
        $homeHtml.IndexOf(
            '<div class="grid">',
            $markerIndex)

    if ($gridStart -lt 0) {
        throw "Memetic documentation: Scientific components grid is missing."
    }

    $insertAt =
        $gridStart +
        '<div class="grid">'.Length

    $card = @'
<div class="card"><h3><a href="components/memetic-algorithm-components.html">Memetic Algorithm Components</a></h3><div class="meta">5 local-improvement policies &middot; Lamarckian / Baldwinian learning &middot; adaptive stagnation control</div><span class="id">ma.*</span></div>
'@

    $homeHtml =
        $homeHtml.Insert(
            $insertAt,
            "`n" + $card)

    Write-Utf8 $homePath $homeHtml
}

$algorithmPath =
    Join-Path $Site "algorithms\memetic-algorithm-moscato-1989.html"

$algorithmHtml =
    Read-Utf8 $algorithmPath

if (-not $algorithmHtml.Contains(
    '../components/memetic-algorithm-components.html')) {

    $mainEnd =
        $algorithmHtml.LastIndexOf('</main>')

    if ($mainEnd -lt 0) {
        throw "Memetic documentation: algorithm portal main element is missing."
    }

    $componentLink = @'
<div class="section">
<h2>Memetic scientific components</h2>
<p><a href="../components/memetic-algorithm-components.html"><strong>Open the Memetic Algorithm Components catalog</strong></a></p>
</div>
'@

    $algorithmHtml =
        $algorithmHtml.Insert(
            $mainEnd,
            $componentLink)

    Write-Utf8 $algorithmPath $algorithmHtml
}
