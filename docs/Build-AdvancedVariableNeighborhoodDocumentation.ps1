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
    Join-Path $Root "docs\advanced-variable-neighborhood-search-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Advanced VNS documentation: catalog is missing."
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
    -Destination (Join-Path $Site "advanced-variable-neighborhood-search-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.executable)) {
    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.acronym)) &mdash; $(Html ([string]$entry.id)) <span class="badge">executable</span></h3>
<div class="meta">$(Html ([string]$entry.description))</div>
<span class="id">$(Html ([string]$entry.id))</span>
</div>
"@)
}

foreach ($entry in @($catalog.reviewedDeferred)) {
    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.acronym)) &mdash; $(Html ([string]$entry.id)) <span class="badge">reviewed / deferred</span></h3>
<div class="meta">$(Html ([string]$entry.reason))</div>
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
<title>Advanced Variable Neighborhood Search Variants &middot; MetaheuristicsPlatform</title>
<link rel="stylesheet" href="../assets/site.css">
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
<h1>Advanced Variable Neighborhood Search Variants</h1>
<p>This catalog distinguishes the three executable advanced VNS variants introduced in v0.27.0 from VNDS, which is scientifically reviewed but intentionally deferred until a generic decomposition/subproblem abstraction exists.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Scientific references</h2>
<p>$($referenceLines -join "<br>")</p>
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../advanced-variable-neighborhood-search-catalog.json">Open <code>advanced-variable-neighborhood-search-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "advanced-variable-neighborhood-search-variants.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "Advanced VNS documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

$homeCard = @"
<div class="card">
<h3><a href="components/advanced-variable-neighborhood-search-variants.html">Advanced Variable Neighborhood Search Variants</a></h3>
<div class="meta">3 executable variants (RVNS / GVNS / SVNS) &middot; VNDS reviewed/deferred until a decomposition contract exists</div>
<span class="id">vns.variants</span>
</div>
"@

$componentsMarker =
    '<h2 id="components">Scientific components</h2>'

if ($homeContent.Contains(
    "components/advanced-variable-neighborhood-search-variants.html")) {
    # Idempotent no-op.
}
elseif ($homeContent.Contains($componentsMarker)) {
    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if ($gridStart -lt 0) {
        throw "Advanced VNS documentation: scientific-components grid is missing."
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
        throw "Advanced VNS documentation: unable to inject Scientific components."
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

foreach ($algorithmPageName in @(
    "variable-neighborhood-search-mladenovic-hansen.html",
    "reduced-variable-neighborhood-search.html",
    "general-variable-neighborhood-search.html",
    "skewed-variable-neighborhood-search-hansen-mladenovic-2001.html"
)) {
    $algorithmPath =
        Join-Path $Site ("algorithms\" + $algorithmPageName)

    if (-not (Test-Path -LiteralPath $algorithmPath)) {
        throw "Advanced VNS documentation: generated algorithm page '$algorithmPageName' is missing."
    }

    $content =
        [System.IO.File]::ReadAllText(
            $algorithmPath,
            [System.Text.Encoding]::UTF8)

    if (-not $content.Contains(
        "components/advanced-variable-neighborhood-search-variants.html")) {

        $section = @"
<div class="section">
<h2>Advanced VNS variant catalog</h2>
<p><a href="../components/advanced-variable-neighborhood-search-variants.html"><strong>Open the complete Advanced Variable Neighborhood Search Variants catalog</strong></a></p>
</div>
"@

        if (-not $content.Contains("</main>")) {
            throw "Advanced VNS documentation: unable to inject component link into '$algorithmPageName'."
        }

        $content =
            $content.Replace(
                "</main>",
                $section + "`n</main>")

        Write-Utf8 $algorithmPath $content
    }
}

Write-Host "Advanced VNS component documentation generated." -ForegroundColor Green
