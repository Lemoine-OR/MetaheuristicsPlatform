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

$catalogPath =
    Join-Path $Root "docs\sa-cooling-catalog.json"

$catalog =
    Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 |
    ConvertFrom-Json

$entries = @($catalog.entries)
$implementedCount = @(
    $entries |
    Where-Object availability -eq "implemented"
).Count
$reviewedCount = @(
    $entries |
    Where-Object availability -eq "reviewed-composite"
).Count

$componentDirectory =
    Join-Path $Site "components"
New-Item -ItemType Directory -Force -Path $componentDirectory | Out-Null

Copy-Item `
    -LiteralPath $catalogPath `
    -Destination (Join-Path $Site "sa-cooling-catalog.json") `
    -Force

$cards = New-Object System.Collections.Generic.List[string]

foreach ($entry in $entries) {
    $status =
        if ([string]$entry.availability -eq "implemented") {
            "implemented"
        }
        else {
            "reviewed composite"
        }

    $doi =
        if ([string]::IsNullOrWhiteSpace([string]$entry.doi)) {
            ""
        }
        else {
            "<br><strong>DOI:</strong> <code>$(Html ([string]$entry.doi))</code>"
        }

    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">$(Html $status)</span></h3>
<div class="math">\($(Html ([string]$entry.formula))\)</div>
<div class="meta">
<strong>Scope:</strong> $(Html ([string]$entry.scope))<br>
<strong>Parameters:</strong> $(Html ([string]$entry.parameters))<br>
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
<title>SA Scientific Cooling Catalog &middot; MetaheuristicsPlatform</title>
<link rel="stylesheet" href="../assets/site.css">
<script>
window.MathJax = { tex: { inlineMath: [['\\(','\\)']], displayMath: [['\\[','\\]']] } };
</script>
<script async src="https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js"></script>
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
<h1>Simulated Annealing Scientific Cooling Catalog</h1>
<p>The scientific cooling catalog introduced in v0.20.0 separates executable temperature laws from broader annealing controllers. The catalog contains <strong>$implementedCount executable schedules</strong> and <strong>$reviewedCount reviewed composite controllers</strong>.</p>
<div class="section">
<h2>Scientific contract</h2>
<p>A temperature formula extracted from FSA, VFSR/ASA, GSA or another composite annealing method is explicitly identified as a component when the rest of the published method is not implemented. Statistical schedules use allocation-free level statistics only when required.</p>
</div>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../sa-cooling-catalog.json">Open <code>sa-cooling-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "simulated-annealing-cooling-schedules.html"
Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "SA cooling documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText($homePath)

$homeSection = @"
<h2 id="components">Scientific components</h2>
<div class="grid">
<div class="card">
<h3><a href="components/simulated-annealing-cooling-schedules.html">Simulated Annealing Scientific Cooling Catalog</a></h3>
<div class="meta">$implementedCount executable schedules &middot; $reviewedCount reviewed composite controllers &middot; literature-backed formulas, assumptions and scope</div>
<span class="id">sa.cooling.*</span>
</div>
</div>
"@

if (-not $homeContent.Contains("</main>")) {
    throw "SA cooling documentation: unable to inject the component panel into generated home page."
}

$homeContent =
    $homeContent.Replace(
        "</main>",
        $homeSection + "`n</main>")
Write-Utf8 $homePath $homeContent

$saPath =
    Join-Path $Site "algorithms\simulated-annealing-metropolis.html"

if (-not (Test-Path -LiteralPath $saPath)) {
    throw "SA cooling documentation: generated Simulated Annealing page is missing."
}

$saPage =
    [System.IO.File]::ReadAllText($saPath)

$saSection = @"
<div class="section">
<h2>Scientific cooling catalog</h2>
<p>Since v0.20.0, the SA engine exposes <strong>$implementedCount built-in executable schedules</strong>, including deterministic, logarithmic, fast, dimension-dependent, generalized and statistical/adaptive laws. Broader controllers are reviewed without being reduced to scientifically misleading scalar approximations.</p>
<p><a href="../components/simulated-annealing-cooling-schedules.html"><strong>Open the complete Simulated Annealing Scientific Cooling Catalog</strong></a></p>
</div>
"@

if (-not $saPage.Contains("</main>")) {
    throw "SA cooling documentation: unable to inject the cooling link into generated SA page."
}

$saPage =
    $saPage.Replace(
        "</main>",
        $saSection + "`n</main>")
Write-Utf8 $saPath $saPage

Write-Host (
    "SA cooling documentation generated: {0} executable schedules, {1} reviewed-composite controllers." -f
    $implementedCount,
    $reviewedCount) -ForegroundColor Green
