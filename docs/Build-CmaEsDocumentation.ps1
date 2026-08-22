[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Site = (Join-Path (Split-Path -Parent $PSScriptRoot) "Documentation\site")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "CMA-ES documentation: missing '$Path'."
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
        (New-Object System.Text.UTF8Encoding($false)))
}

function Html([string]$Value) {
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

$catalogPath =
    Join-Path $Root "docs\cma-es-component-catalog.json"

$catalog =
    (Read-Utf8 $catalogPath) |
    ConvertFrom-Json

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.entries)) {
    $badge =
        if ([string]$entry.status -eq "implemented") {
            "implemented"
        }
        else {
            "reviewed / deferred"
        }

    $formulaEncoded =
        Html ([string]$entry.formula)

    $formulaHtml =
        if ([string]$entry.formulaMode -eq "math") {
            '<div class="math">\[' +
            $formulaEncoded +
            '\]</div>'
        }
        else {
            '<div class="formula-note">' +
            $formulaEncoded +
            '</div>'
        }

    $cards.Add(
        '<div class="card"><h3>' +
        (Html ([string]$entry.name)) +
        ' <span class="badge">' +
        $badge +
        '</span></h3><div class="meta">' +
        (Html ([string]$entry.reference)) +
        '</div>' +
        $formulaHtml +
        '<span class="id">' +
        (Html ([string]$entry.id)) +
        '</span></div>')
}

$page =
'<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>CMA-ES Components &middot; MetaheuristicsPlatform</title>
<link rel="stylesheet" href="../assets/site.css">
<script>
window.MathJax = { tex: { inlineMath: [["\\(","\\)"]], displayMath: [["\\[","\\]"]] } };
</script>
<script defer src="https://cdn.jsdelivr.net/npm/mathjax@3.2.2/es5/tex-chtml.js"></script>
</head>
<body>
<header><div class="wrap">
<div class="brand"><a href="../index.html"><img src="../assets/metaheuristicsplatform-logo.svg" alt="MetaheuristicsPlatform"></a></div>
<nav><a href="../index.html">Home</a><a href="../index.html#algorithms">Algorithms</a><a href="../index.html#families">Families</a><a href="../api/index.html">API</a></nav>
</div></header>
<main class="wrap">
<h1>CMA-ES Components</h1>
<p>CMA-ES components in v0.47.0: canonical full covariance, weighted Active CMA-ES and sep-CMA-ES are executable; IPOP/BIPOP remain reviewed/deferred.</p>
<div class="grid">' +
($cards -join [Environment]::NewLine) +
'</div>
<div class="section"><h2>Scientific documentation</h2><p><a href="../api/cma_es_components.html"><strong>Open the complete Doxygen page</strong></a></p></div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body>
</html>'

$componentDirectory =
    Join-Path $Site "components"

Write-Utf8 `
    (Join-Path $componentDirectory "cma-es-components.html") `
    $page

Write-Utf8 `
    (Join-Path $Site "cma-es-component-catalog.json") `
    (Read-Utf8 $catalogPath)

$homePath =
    Join-Path $Site "index.html"

$homeHtml =
    Read-Utf8 $homePath

if (-not $homeHtml.Contains(
        "components/cma-es-components.html")) {

    $marker =
        '<h2 id="components">Scientific components</h2>'

    $markerIndex =
        $homeHtml.IndexOf($marker)

    if ($markerIndex -lt 0) {
        throw "CMA-ES documentation: Scientific components marker is missing."
    }

    $gridStart =
        $homeHtml.IndexOf(
            '<div class="grid">',
            $markerIndex)

    if ($gridStart -lt 0) {
        throw "CMA-ES documentation: Scientific components grid is missing."
    }

    $insertAt =
        $gridStart +
        '<div class="grid">'.Length

    $card =
        '<div class="card"><h3><a href="components/cma-es-components.html">CMA-ES Components</a></h3><div class="meta">Gaussian sampling &middot; CSA &middot; rank-one/rank-mu &middot; active negative update &middot; separable covariance</div><span class="id">cma.*</span></div>'

    $homeHtml =
        $homeHtml.Insert(
            $insertAt,
            [Environment]::NewLine + $card)

    Write-Utf8 $homePath $homeHtml
}

$algorithmPath =
    Join-Path $Site "algorithms\cma-es-hansen-ostermeier-2001.html"

$algorithmHtml =
    Read-Utf8 $algorithmPath

if (-not $algorithmHtml.Contains(
        '../components/cma-es-components.html')) {

    $mainEnd =
        $algorithmHtml.LastIndexOf('</main>')

    if ($mainEnd -lt 0) {
        throw "CMA-ES documentation: algorithm portal main element is missing."
    }

    $componentLink =
        '<div class="section"><h2>CMA-ES scientific components</h2><p><a href="../components/cma-es-components.html"><strong>Open the CMA-ES Components catalog</strong></a></p></div>'

    $algorithmHtml =
        $algorithmHtml.Insert(
            $mainEnd,
            $componentLink)

    Write-Utf8 $algorithmPath $algorithmHtml
}
