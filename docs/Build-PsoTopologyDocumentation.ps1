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
    Join-Path $Root "docs\pso-topology-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "PSO topology documentation: catalog is missing."
}

$catalog =
    [System.IO.File]::ReadAllText(
        $catalogPath,
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$entries = @($catalog.entries)

if ($entries.Count -eq 0) {
    throw "PSO topology documentation: catalog contains no entries."
}

$componentDirectory =
    Join-Path $Site "components"

New-Item `
    -ItemType Directory `
    -Force `
    -Path $componentDirectory |
    Out-Null

Copy-Item `
    -LiteralPath $catalogPath `
    -Destination (Join-Path $Site "pso-topology-catalog.json") `
    -Force

$cards =
    New-Object System.Collections.Generic.List[string]

foreach ($entry in $entries) {
    $status =
        switch ([string]$entry.publishedStatus) {
            "published-exact" { "published exact" }
            "generic-inspired" { "generic / literature-inspired" }
            "extension-point" { "extension point" }
            default { [string]$entry.publishedStatus }
        }

    $defaultBadge =
        if ([bool]$entry.isPsoParameterDefault) {
            "<span class='badge'>PSO default</span>"
        }
        elseif ([bool]$entry.inDefaultCatalog) {
            "<span class='badge'>default catalog</span>"
        }
        else {
            ""
        }

    $dynamicBadge =
        if ([string]$entry.dynamics -eq "FitnessDynamic") {
            "<span class='badge'>dynamic</span>"
        }
        elseif ([string]$entry.dynamics -eq "RandomStatic") {
            "<span class='badge'>random-static</span>"
        }
        else {
            ""
        }

    $referenceLines =
        New-Object System.Collections.Generic.List[string]

    foreach ($reference in @($entry.references)) {
        $line =
            Html ([string]$reference.text)

        if (-not [string]::IsNullOrWhiteSpace(
            [string]$reference.doi)) {
            $line +=
                " &middot; DOI: <code>" +
                (Html ([string]$reference.doi)) +
                "</code>"
        }

        $referenceLines.Add($line)
    }

    $references =
        if ($referenceLines.Count -eq 0) {
            "User-defined extension; no publication identity is claimed."
        }
        else {
            $referenceLines -join "<br>"
        }

    $cards.Add(@"
<div class="card">
<h3>$(Html ([string]$entry.name)) <span class="badge">$(Html $status)</span>$defaultBadge$dynamicBadge</h3>
<div class="meta">
<strong>Class:</strong> <code>$(Html ([string]$entry.class))</code><br>
<strong>Dynamics:</strong> $(Html ([string]$entry.dynamics))<br>
<strong>Required state:</strong> $(Html ([string]$entry.requiredData))<br>
<strong>Parameters:</strong> $(Html ([string]$entry.parameters))
</div>
<p><strong>Graph construction.</strong> $(Html ([string]$entry.construction))</p>
<p><strong>Information flow.</strong> $(Html ([string]$entry.informationFlow))</p>
<p><strong>Rebuild semantics.</strong> $(Html ([string]$entry.rebuildPolicy))</p>
<div class="meta"><strong>Scientific provenance.</strong><br>$references</div>
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
<title>PSO Communication Topology Catalog &middot; MetaheuristicsPlatform</title>
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
<h1>PSO Communication Topology Catalog</h1>
<p>MetaheuristicsPlatform currently documents <strong>$($entries.Count) implemented PSO topology classes</strong>. The topology defines the communication graph; the selected social-influence policy determines how each particle uses the informers visible through that graph.</p>

<div class="section">
<h2>How topology interacts with PSO</h2>
<p>With the default canonical best-neighborhood policy, each particle combines its own personal best with the best personal-best available in its communication neighborhood. A topology therefore controls the speed and structure of information propagation through the swarm.</p>
<p><strong>Static</strong> graphs are built once. <strong>RandomStatic</strong> graphs are sampled once per run. Dynamic topology classes are invalidated by the PSO runtime after every completed iteration and rebuilt before the next movement step from the state they declare as required.</p>
</div>

<div class="section">
<h2>Important implementation distinctions</h2>
<p><strong>Fully Connected</strong> is the default PSO topology and has a graphless canonical fast path. <strong>DCluster</strong> is the currently implemented fitness-dynamic exact published topology and is rebuilt from current-fitness ranking. The Watts-Strogatz and Barabasi-Albert classes are intentionally documented as generic static graph implementations rather than falsely labeled as the complete adaptive SWPSO or SFIPSO methods.</p>
</div>

<div class="grid">
$($cards -join "`n")
</div>

<div class="section">
<h2>Machine-readable catalog</h2>
<p><a href="../pso-topology-catalog.json">Open <code>pso-topology-catalog.json</code></a></p>
</div>
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@

$componentPage =
    Join-Path $componentDirectory "pso-communication-topologies.html"

Write-Utf8 $componentPage $page

$homePath =
    Join-Path $Site "index.html"

if (-not (Test-Path -LiteralPath $homePath)) {
    throw "PSO topology documentation: generated home page is missing."
}

$homeContent =
    [System.IO.File]::ReadAllText(
        $homePath,
        [System.Text.Encoding]::UTF8)

$homeCard = @"
<div class="card">
<h3><a href="components/pso-communication-topologies.html">PSO Communication Topology Catalog</a></h3>
<div class="meta">$($entries.Count) implemented topology classes &middot; static, random-static and fitness-dynamic structures &middot; exact-vs-generic scientific provenance &middot; DCluster documented explicitly</div>
<span class="id">pso.topology.*</span>
</div>
"@

$componentsMarker =
    '<h2 id="components">Scientific components</h2>'

if ($homeContent.Contains(
    "components/pso-communication-topologies.html")) {
    # Idempotent no-op.
}
elseif ($homeContent.Contains($componentsMarker)) {
    $gridStart =
        $homeContent.IndexOf(
            '<div class="grid">',
            $homeContent.IndexOf($componentsMarker))

    if ($gridStart -lt 0) {
        throw "PSO topology documentation: scientific-components grid is missing."
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
        throw "PSO topology documentation: unable to inject Scientific components."
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

$psoPath =
    Join-Path $Site "algorithms\particle-swarm.html"

if (-not (Test-Path -LiteralPath $psoPath)) {
    throw "PSO topology documentation: generated Particle Swarm page is missing."
}

$psoContent =
    [System.IO.File]::ReadAllText(
        $psoPath,
        [System.Text.Encoding]::UTF8)

if (-not $psoContent.Contains(
    "components/pso-communication-topologies.html")) {

    $psoSection = @"
<div class="section">
<h2>Implemented communication topologies</h2>
<p>The PSO runtime contains <strong>$($entries.Count) documented topology classes</strong>, including the graphless-compatible fully connected default, local ring/Von-Neumann structures, clustered/random/small-world/scale-free graphs, exact dynamic DCluster, and a custom graph extension point.</p>
<p><a href="../components/pso-communication-topologies.html"><strong>Open the complete PSO Communication Topology Catalog</strong></a></p>
</div>
"@

    if (-not $psoContent.Contains("</main>")) {
        throw "PSO topology documentation: unable to inject topology link into PSO page."
    }

    $psoContent =
        $psoContent.Replace(
            "</main>",
            $psoSection + "`n</main>")

    Write-Utf8 $psoPath $psoContent
}

Write-Host (
    "PSO topology documentation generated: {0} implemented topology classes." -f
    $entries.Count) -ForegroundColor Green
