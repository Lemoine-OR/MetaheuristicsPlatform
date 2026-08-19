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

function Write-Utf8(
    [string]$Path,
    [string]$Content) {

    $directory = Split-Path -Parent $Path

    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        [System.Text.UTF8Encoding]::new($false))
}

$catalogPath =
    Join-Path $Root "docs\path-relinking-strategy-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Path Relinking documentation: strategy catalog is missing."
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
    -Destination (Join-Path $Site "path-relinking-strategy-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.implemented)) {
    $formula = Html ([string]$entry.formula)
    $formulaMode = [string]$entry.formulaMode

    $formulaHtml =
        switch ($formulaMode) {
            "math" {
                '<div class="math">\[' + $formula + '\]</div>'
            }

            "prose" {
                '<div class="formula-note">' + $formula + '</div>'
            }

            default {
                throw "Path Relinking documentation: unsupported formulaMode '$formulaMode' for '$($entry.id)'."
            }
        }

    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">implemented</span></h3>
<div class="meta">$(Html ([string]$entry.kind))</div>
$formulaHtml
<span class="id">$(Html ([string]$entry.id))</span>
</div>
"@)
}

foreach ($entry in @($catalog.reviewedDeferred)) {
    $formula = Html ([string]$entry.formula)
    $formulaMode = [string]$entry.formulaMode

    $formulaHtml =
        switch ($formulaMode) {
            "math" {
                '<div class="math">\[' + $formula + '\]</div>'
            }

            "prose" {
                '<div class="formula-note">' + $formula + '</div>'
            }

            default {
                throw "Path Relinking documentation: unsupported formulaMode '$formulaMode' for '$($entry.id)'."
            }
        }

    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">reviewed / deferred</span></h3>
<div class="meta">$(Html ([string]$entry.reason))</div>
$formulaHtml
<span class="id">$(Html ([string]$entry.id))</span>
</div>
"@)
}

$referenceLines =
    New-Object System.Collections.Generic.List[string]

foreach ($reference in @($catalog.references)) {
    $referenceLines.Add(
        (Html ([string]$reference.publication)) +
        " &middot; DOI: <code>" +
        (Html ([string]$reference.doi)) +
        "</code>")
}

$page = @"
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Advanced Path Relinking Strategies &middot; MetaheuristicsPlatform</title>
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
<h1>Advanced Path Relinking Strategies</h1>
<p>Seven executable path-relinking strategies are available in v0.32.0, including generational Evolutionary Path Relinking over a bounded quality/diversity elite population.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Scientific references</h2>
<p>$($referenceLines -join "<br>")</p>
<p><a href="../api/path_relinking_strategies.html"><strong>Open the complete scientific Doxygen page</strong></a></p>
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../path-relinking-strategy-catalog.json">Open <code>path-relinking-strategy-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "path-relinking-strategies.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "Path Relinking documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

$homeCard = @"
<div class="card">
<h3><a href="components/path-relinking-strategies.html">Advanced Path Relinking Strategies</a></h3>
<div class="meta">7 executable strategies &middot; forward / backward / back-and-forward / mixed &middot; truncation &middot; greedy-randomized RCL &middot; generational evolutionary PR</div>
<span class="id">pr.*</span>
</div>
"@

$componentsMarker =
    '<h2 id="components">Scientific components</h2>'

if ($homeContent.Contains(
    "components/path-relinking-strategies.html")) {
    # Idempotent no-op.
}
elseif ($homeContent.Contains($componentsMarker)) {
    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if ($gridStart -lt 0) {
        throw "Path Relinking documentation: scientific-components grid is missing."
    }

    $insertAt =
        $gridStart + '<div class="grid">'.Length

    $homeContent =
        $homeContent.Insert(
            $insertAt,
            "`n" + $homeCard)
}
else {
    if (-not $homeContent.Contains("</main>")) {
        throw "Path Relinking documentation: unable to inject Scientific components."
    }

    $newSection = @"
<h2 id="components">Scientific components</h2>
<div class="grid">
$homeCard
</div>
"@

    $homeContent =
        $homeContent.Replace(
            "</main>",
            $newSection + "`n</main>")
}

Write-Utf8 $homePath $homeContent

$algorithmPath =
    Join-Path $Site "algorithms\grasp-path-relinking.html"

if (-not (Test-Path -LiteralPath $algorithmPath)) {
    throw "Path Relinking documentation: generated GRASP-PR algorithm page is missing."
}

$algorithmContent =
    [System.IO.File]::ReadAllText(
        $algorithmPath,
        [System.Text.Encoding]::UTF8)

if (-not $algorithmContent.Contains(
    "components/path-relinking-strategies.html")) {

    $section = @"
<div class="section">
<h2>Advanced Path Relinking strategy catalog</h2>
<p><a href="../components/path-relinking-strategies.html"><strong>Open the complete Advanced Path Relinking Strategies catalog</strong></a></p>
</div>
"@

    if (-not $algorithmContent.Contains("</main>")) {
        throw "Path Relinking documentation: unable to inject strategy-catalog link into GRASP-PR page."
    }

    $algorithmContent =
        $algorithmContent.Replace(
            "</main>",
            $section + "`n</main>")

    Write-Utf8 $algorithmPath $algorithmContent
}

Write-Host (
    "Advanced Path Relinking component documentation generated: {0} executable, {1} reviewed/deferred." -f
    @($catalog.implemented).Count,
    @($catalog.reviewedDeferred).Count
) -ForegroundColor Green