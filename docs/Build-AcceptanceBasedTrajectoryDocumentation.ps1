[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Site = (Join-Path (Split-Path -Parent $PSScriptRoot) "Documentation\site")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Html([string]$Value) { [System.Net.WebUtility]::HtmlEncode($Value) }
function Write-Utf8File([string]$p,[string]$c) {
    $d=Split-Path -Parent $p
    if ($d -and -not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Force -Path $d | Out-Null }
    [System.IO.File]::WriteAllText($p,$c,[System.Text.UTF8Encoding]::new($false))
}

$catalogPath=Join-Path $Root "docs\acceptance-based-trajectory-catalog.json"
$catalog=[System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$componentDirectory=Join-Path $Site "components"
New-Item -ItemType Directory -Force -Path $componentDirectory|Out-Null
Copy-Item $catalogPath (Join-Path $Site "acceptance-based-trajectory-catalog.json") -Force

$implementedCount=@($catalog.entries|Where-Object { [string]$_.status -eq "implemented" }).Count
$deferredCount=@($catalog.entries|Where-Object { [string]$_.status -ne "implemented" }).Count

$cards=New-Object System.Collections.Generic.List[string]
foreach($entry in @($catalog.entries)){
    $f=Html([string]$entry.formula)
    $fh=switch([string]$entry.formulaMode){
        "math" {'<div class="math">\['+$f+'\]</div>'}
        "prose" {'<div class="formula-note">'+$f+'</div>'}
        default {throw "Unsupported formulaMode '$($entry.formulaMode)'."}
    }
    $badge=if([string]$entry.status -eq "implemented"){"implemented"}else{"reviewed / deferred"}
    $cards.Add("<div class='card'><h3>$(Html([string]$entry.name)) <span class='badge'>$badge</span></h3><div class='meta'>$(Html([string]$entry.kind))</div>$fh<span class='id'>$(Html([string]$entry.id))</span></div>")
}

$page=@"
<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Acceptance-Based Trajectory Methods &middot; MetaheuristicsPlatform</title><link rel="stylesheet" href="../assets/site.css">
<script>window.MathJax={tex:{inlineMath:[['\\(','\\)']],displayMath:[['\\[','\\]']]}};</script>
<script defer src="https://cdn.jsdelivr.net/npm/mathjax@3.2.2/es5/tex-chtml.js"></script></head>
<body><header><div class="wrap"><div class="brand"><a href="../index.html"><img src="../assets/metaheuristicsplatform-logo.svg" alt="MetaheuristicsPlatform"></a></div>
<nav><a href="../index.html">Home</a><a href="../index.html#algorithms">Algorithms</a><a href="../index.html#families">Families</a><a href="../api/index.html">API</a><a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform">GitHub</a></nav></div></header>
<main class="wrap"><h1>Acceptance-Based Trajectory Methods</h1>
<p>Great Deluge, Record-to-Record Travel, final-form Late Acceptance Hill Climbing and Demon-Based Acceptance are executable. Extended/Flex Deluge, Demon-like credit-reset ILS and the Zimmermann-Salamon ensemble algorithm remain explicitly reviewed/deferred.</p>
<div class="grid">$($cards -join "`n")</div>
<div class="section"><h2>Scientific documentation</h2><p><a href="../api/acceptance_based_trajectory_methods.html"><strong>Open the complete Doxygen page</strong></a></p></div>
<div class="section"><h2>Machine-readable catalog</h2><p><a href="../acceptance-based-trajectory-catalog.json"><code>acceptance-based-trajectory-catalog.json</code></a></p></div>
</main><footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer></body></html>
"@
Write-Utf8File (Join-Path $componentDirectory "acceptance-based-trajectory-methods.html") $page

$homePath=Join-Path $Site "index.html"
$homeContent=[System.IO.File]::ReadAllText($homePath,[System.Text.Encoding]::UTF8)
if(-not $homeContent.Contains("components/acceptance-based-trajectory-methods.html")){
    $marker='<h2 id="components">Scientific components</h2>'
    if($homeContent.Contains($marker)){
        $g=$homeContent.IndexOf('<div class="grid">',$homeContent.IndexOf($marker))
        if($g-lt 0){throw "Scientific-components grid missing."}
        $at=$g+'<div class="grid">'.Length
        $card='<div class="card"><h3><a href="components/acceptance-based-trajectory-methods.html">Acceptance-Based Trajectory Methods</a></h3><div class="meta">GDA + RRT + LAHC + Demon executable &middot; Extended/Flex Deluge + distinct Demon variants reviewed/deferred</div><span class="id">acceptance.*</span></div>'
        $homeContent=$homeContent.Insert($at,"`n"+$card)
    }else{throw "Scientific components marker missing."}
    Write-Utf8File $homePath $homeContent
}

foreach($id in @(
    "great-deluge-dueck-1993",
    "record-to-record-travel-dueck-1993",
    "late-acceptance-hill-climbing-burke-bykov-2017",
    "demon-based-acceptance-talbi-2009"
)){
    $algorithmPath=Join-Path $Site ("algorithms\"+$id+".html")
    $algorithmContent=[System.IO.File]::ReadAllText($algorithmPath,[System.Text.Encoding]::UTF8)
    if(-not $algorithmContent.Contains("components/acceptance-based-trajectory-methods.html")){
        $section='<div class="section"><h2>Acceptance-family catalog</h2><p><a href="../components/acceptance-based-trajectory-methods.html"><strong>Open the acceptance-based trajectory method catalog</strong></a></p></div>'
        $algorithmContent=$algorithmContent.Replace("</main>",$section+"`n</main>")
        Write-Utf8File $algorithmPath $algorithmContent
    }
}

Write-Host "Acceptance-based trajectory component documentation generated: $implementedCount implemented, $deferredCount reviewed/deferred." -ForegroundColor Green
