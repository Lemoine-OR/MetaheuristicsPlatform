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
    Join-Path $Root "docs\threshold-accepting-schedule-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Threshold Accepting documentation: schedule catalog is missing."
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
    -Destination (Join-Path $Site "threshold-accepting-schedule-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in @($catalog.entries)) {
    $formula =
        Html ([string]$entry.formula)

    $formulaMode =
        [string]$entry.formulaMode

    $formulaHtml =
        switch ($formulaMode) {
            "math" {
                '<div class="math">\[' + $formula + '\]</div>'
            }

            "prose" {
                '<div class="formula-note">' + $formula + '</div>'
            }

            default {
                throw "Threshold Accepting documentation: unsupported formulaMode '$formulaMode' for '$($entry.id)'."
            }
        }

    $status =
        [string]$entry.status

    $badge =
        if ($status -eq "implemented") {
            "implemented"
        }
        else {
            "reviewed / deferred"
        }

    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">$badge</span></h3>
<div class="meta">$(Html ([string]$entry.kind))</div>
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
<title>Threshold Accepting Schedules &middot; MetaheuristicsPlatform</title>
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
<h1>Threshold Accepting Schedules and Acceptance Controls</h1>
<p>Version 0.33.0 provides three executable monotone threshold schedules for the Dueck-Scheuer Threshold Accepting trajectory. Old Bachelor Acceptance is reviewed separately because its self-tuning non-monotone threshold semantics require a different controller contract.</p>
<div class="grid">
$($cards -join "`n")
</div>
<div class="section">
<h2>Scientific references</h2>
<p>$($referenceLines -join "<br>")</p>
<p><a href="../api/threshold_accepting_schedules.html"><strong>Open the complete scientific Doxygen page</strong></a></p>
</div>
<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../threshold-accepting-schedule-catalog.json">Open <code>threshold-accepting-schedule-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "threshold-accepting-schedules.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "Threshold Accepting documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

$homeCard = @"
<div class="card">
<h3><a href="components/threshold-accepting-schedules.html">Threshold Accepting Schedules</a></h3>
<div class="meta">3 executable monotone threshold schedules &middot; deterministic Dueck-Scheuer acceptance &middot; Old Bachelor Acceptance reviewed/deferred</div>
<span class="id">ta.threshold.*</span>
</div>
"@

$componentsMarker =
    '<h2 id="components">Scientific components</h2>'

if ($homeContent.Contains(
    "components/threshold-accepting-schedules.html")) {
    # Idempotent no-op.
}
elseif ($homeContent.Contains($componentsMarker)) {
    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if ($gridStart -lt 0) {
        throw "Threshold Accepting documentation: scientific-components grid is missing."
    }

    $insertAt =
        $gridStart +
        '<div class="grid">'.Length

    $homeContent =
        $homeContent.Insert(
            $insertAt,
            "`n" + $homeCard)
}
else {
    if (-not $homeContent.Contains("</main>")) {
        throw "Threshold Accepting documentation: unable to inject Scientific components."
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
    Join-Path $Site "algorithms\threshold-accepting-dueck-scheuer-1990.html"

if (-not (Test-Path -LiteralPath $algorithmPath)) {
    throw "Threshold Accepting documentation: generated algorithm page is missing."
}

$algorithmContent =
    [System.IO.File]::ReadAllText(
        $algorithmPath,
        [System.Text.Encoding]::UTF8)

if (-not $algorithmContent.Contains(
    "components/threshold-accepting-schedules.html")) {

    $section = @"
<div class="section">
<h2>Threshold schedule catalog</h2>
<p><a href="../components/threshold-accepting-schedules.html"><strong>Open the complete Threshold Accepting schedule catalog</strong></a></p>
</div>
"@

    if (-not $algorithmContent.Contains("</main>")) {
        throw "Threshold Accepting documentation: unable to inject schedule-catalog link."
    }

    $algorithmContent =
        $algorithmContent.Replace(
            "</main>",
            $section + "`n</main>")

    Write-Utf8 $algorithmPath $algorithmContent
}

Write-Host (
    "Threshold Accepting component documentation generated: {0} implemented, {1} reviewed/deferred." -f
    @($catalog.entries | Where-Object status -eq "implemented").Count,
    @($catalog.entries | Where-Object status -ne "implemented").Count
) -ForegroundColor Green