[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Site = (Join-Path (Split-Path -Parent $PSScriptRoot) "Documentation\site")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Html([string]$value) {
    return [System.Net.WebUtility]::HtmlEncode($value)
}

function Write-Utf8([string]$path, [string]$content) {
    $directory = Split-Path -Parent $path

    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $path,
        $content,
        (New-Object System.Text.UTF8Encoding($false)))
}

function Get-OptionalPropertyString(
    [object]$Object,
    [string]$Name) {

    $property = $Object.PSObject.Properties[$Name]

    if ($null -eq $property) {
        return ""
    }

    return [string]$property.Value
}

$catalogPath =
    Join-Path $Root "docs\ts-memory-control-catalog.json"

$catalog =
    Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 |
    ConvertFrom-Json

$entries = @($catalog.entries)

$implementedCount =
    @(
        $entries |
        Where-Object availability -eq "implemented"
    ).Count

$reviewedCount =
    @(
        $entries |
        Where-Object availability -eq "reviewed-composite"
    ).Count

$componentDirectory =
    Join-Path $Site "components"

New-Item -ItemType Directory -Force -Path $componentDirectory | Out-Null

Copy-Item `
    -LiteralPath $catalogPath `
    -Destination (Join-Path $Site "ts-memory-control-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in $entries) {
    $status =
        if ([string]$entry.availability -eq "implemented") {
            "implemented"
        }
        else {
            "reviewed composite"
        }

    $doiValue =
        Get-OptionalPropertyString -Object $entry -Name "doi"

    $doi =
        if ([string]::IsNullOrWhiteSpace($doiValue)) {
            ""
        }
        else {
            "<br><strong>DOI:</strong> <code>$(Html $doiValue)</code>"
        }

    $formulaMode =
        [string]$entry.formulaMode

    $formulaBlock =
        switch ($formulaMode) {
            "math" {
                '<div class="math">\[' +
                (Html ([string]$entry.formula)) +
                '\]</div>'
            }

            "prose" {
                '<div class="formula-note"><strong>Scientific model:</strong> ' +
                (Html ([string]$entry.formula)) +
                '</div>'
            }

            default {
                throw "Unsupported formulaMode '$formulaMode' for '$($entry.id)'."
            }
        }
    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">$(Html $status)</span></h3>
$formulaBlock
<div class="meta">
<strong>Category:</strong> $(Html ([string]$entry.category))<br>
<strong>Scope:</strong> $(Html ([string]$entry.scope))<br>
<strong>Complexity:</strong> $(Html ([string]$entry.complexity))<br>
$(Html ([string]$entry.reference))$doi
</div>
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
<title>Tabu Search Memory and Reactive Control Catalog &middot; MetaheuristicsPlatform</title>
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
<h1>Tabu Search Memory and Reactive Control Catalog</h1>
<p>Introduced in v0.22.0, this catalog contains <strong>$implementedCount executable components</strong> and <strong>$reviewedCount reviewed advanced strategies</strong>. Reactive Tabu Search remains a distinct public algorithm with its own stable ID.</p>
<div class="section">
<h2>Scientific contract</h2>
<p>Short-term recency, longer-term frequency, configuration-repetition feedback, reactive tenure, intensification and diversification are documented separately. Domain-specific strategies such as strategic oscillation, influence memory and path relinking are reviewed without false scalar reductions.</p>
</div>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../ts-memory-control-catalog.json">Open <code>ts-memory-control-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "tabu-search-memory-control-strategies.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "Tabu Search advanced documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText($homePath)

$homeSection = @"
<div class="card">
<h3><a href="components/tabu-search-memory-control-strategies.html">Tabu Search Memory &amp; Reactive Control Catalog</a></h3>
<div class="meta">$implementedCount executable components &middot; $reviewedCount reviewed advanced strategies &middot; Reactive Tabu Search, repetition memory, adaptive tenure, intensification and diversification</div>
<span class="id">ts.*</span>
</div>
"@

$componentsGridMarker =
    '<h2 id="components">Scientific components</h2>'

if ($homeContent.Contains($componentsGridMarker)) {
    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsGridMarker))

    if ($gridStart -lt 0) {
        throw "Tabu Search advanced documentation: unable to locate scientific-components grid."
    }

    $insertAt =
        $gridStart + '<div class="grid">'.Length

    $homeContent =
        $homeContent.Insert(
            $insertAt,
            "`n" + $homeSection)
}
else {
    if (-not $homeContent.Contains("</main>")) {
        throw "Tabu Search advanced documentation: unable to inject component panel."
    }

    $newComponents = @"
<h2 id="components">Scientific components</h2>
<div class="grid">
$homeSection
</div>
"@

    $homeContent =
        $homeContent.Replace(
            "</main>",
            $newComponents + "`n</main>")
}

Write-Utf8 $homePath $homeContent

foreach ($algorithmPageName in @(
    "tabu-search-glover.html",
    "reactive-tabu-search-battiti-tecchiolli-1994.html"
)) {
    $algorithmPath =
        Join-Path $Site ("algorithms\" + $algorithmPageName)

    if (-not (Test-Path -LiteralPath $algorithmPath)) {
        throw "Tabu Search advanced documentation: generated algorithm page '$algorithmPageName' is missing."
    }

    $algorithmContent =
        [System.IO.File]::ReadAllText($algorithmPath)

    $section = @"
<div class="section">
<h2>Memory and reactive-control catalog</h2>
<p>MetaheuristicsPlatform v0.22.0 exposes $implementedCount executable Tabu Search memory/control components and reviews $reviewedCount advanced strategies without false reductions.</p>
<p><a href="../components/tabu-search-memory-control-strategies.html"><strong>Open the complete Tabu Search Memory and Reactive Control Catalog</strong></a></p>
</div>
"@

    if (-not $algorithmContent.Contains("</main>")) {
        throw "Tabu Search advanced documentation: unable to inject component link into '$algorithmPageName'."
    }

    $algorithmContent =
        $algorithmContent.Replace(
            "</main>",
            $section + "`n</main>")

    Write-Utf8 $algorithmPath $algorithmContent
}

Write-Host (
    "Tabu Search advanced documentation generated: {0} executable components, {1} reviewed strategies." -f
    $implementedCount,
    $reviewedCount
) -ForegroundColor Green
