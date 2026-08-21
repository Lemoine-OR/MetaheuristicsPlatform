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
    if($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        [System.Text.UTF8Encoding]::new($false))
}

$catalogPath =
    Join-Path $Root "docs\advanced-iterated-greedy-catalog.json"

if(-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Advanced Iterated Greedy documentation: catalog is missing."
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
    -Destination (Join-Path $Site "advanced-iterated-greedy-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach($entry in @($catalog.entries)) {
    $model =
        if([string]$entry.formulaMode -eq "math") {
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

$referenceLines =
    New-Object System.Collections.Generic.List[string]

foreach($reference in @($catalog.references)) {
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
<title>Advanced Iterated Greedy Strategies &middot; MetaheuristicsPlatform</title>
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
<h1>Advanced Iterated Greedy Strategies</h1>
<p>Five executable generic components are separated from nine reviewed complete variants whose published semantics remain problem-specific.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Scientific references</h2>
<p>$($referenceLines -join "<br>")</p>
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../advanced-iterated-greedy-catalog.json">Open <code>advanced-iterated-greedy-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "advanced-iterated-greedy-strategies.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if(-not (Test-Path -LiteralPath $homePath)) {
    throw "Advanced Iterated Greedy documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

$homeCard = @"
<div class="card">
<h3><a href="components/advanced-iterated-greedy-strategies.html">Advanced Iterated Greedy Strategies</a></h3>
<div class="meta">5 executable generic components &middot; 9 complete published variants reviewed separately</div>
<span class="id">ig.*</span>
</div>
"@

$componentsMarker =
    '<h2 id="components">Scientific components</h2>'

if($homeContent.Contains(
    "components/advanced-iterated-greedy-strategies.html")) {
    # Idempotent.
}
elseif($homeContent.Contains($componentsMarker)) {
    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if($gridStart -lt 0) {
        throw "Advanced Iterated Greedy documentation: scientific-components grid is missing."
    }

    $insertAt =
        $gridStart + '<div class="grid">'.Length

    $homeContent =
        $homeContent.Insert(
            $insertAt,
            "`n" + $homeCard)
}
else {
    if(-not $homeContent.Contains("</main>")) {
        throw "Advanced Iterated Greedy documentation: unable to inject Scientific components."
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
    Join-Path $Site "algorithms\iterated-greedy-ruiz-stutzle-2007.html"

if(-not (Test-Path -LiteralPath $algorithmPath)) {
    throw "Advanced Iterated Greedy documentation: generated IG page is missing."
}

$algorithmHtml =
    [System.IO.File]::ReadAllText(
        $algorithmPath,
        [System.Text.Encoding]::UTF8)

if(-not $algorithmHtml.Contains(
    "../components/advanced-iterated-greedy-strategies.html")) {

    $headingMarker =
        '<h2>Mathematical details</h2>'

    $headingIndex =
        $algorithmHtml.IndexOf($headingMarker)

    if($headingIndex -lt 0) {
        throw "Advanced Iterated Greedy documentation: IG mathematical-details heading missing."
    }

    $sectionMarker =
        '<div class="section">'

    $sectionIndex =
        $algorithmHtml.LastIndexOf(
            $sectionMarker,
            $headingIndex)

    if($sectionIndex -lt 0) {
        throw "Advanced Iterated Greedy documentation: IG mathematical-details section missing."
    }

    $linkBlock =
        '<div class="section"><h2>Advanced Iterated Greedy catalog</h2>' +
        '<p><a href="../components/advanced-iterated-greedy-strategies.html">' +
        '<strong>Open the advanced Iterated Greedy strategy catalog</strong></a></p></div>'

    $algorithmHtml =
        $algorithmHtml.Insert(
            $sectionIndex,
            $linkBlock + "`n")
}

Write-Utf8 $algorithmPath $algorithmHtml
