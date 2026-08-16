[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [switch]$SkipDoxygenIfUnavailable
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Html([string]$value) {
    return [System.Net.WebUtility]::HtmlEncode($value)
}

function Write-Utf8([string]$path, [string]$content) {
    $directory = Split-Path -Parent $path
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText(
        $path,
        $content,
        [System.Text.UTF8Encoding]::new($false))
}

function Get-MarkdownApiExample([string]$RelativePage) {
    $path = Join-Path $Root $RelativePage

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Canonical algorithm page not found: '$RelativePage'."
    }

    $text =
        [System.IO.File]::ReadAllText(
            $path,
            [System.Text.Encoding]::UTF8)

    $section =
        [regex]::Match(
            $text,
            '(?ms)^##[ \t]+API example[ \t]*\r?\n(?<body>.*?)(?=^##[ \t]+|\z)')

    if (-not $section.Success) {
        throw "Canonical algorithm page '$RelativePage' has no API example section."
    }

    $code =
        [regex]::Match(
            $section.Groups["body"].Value,
            '(?ms)```(?:csharp)?[ \t]*\r?\n(?<code>.*?)\r?\n```[ \t]*$')

    if (-not $code.Success) {
        throw "Canonical algorithm page '$RelativePage' has no fenced C# API example."
    }

    return $code.Groups["code"].Value.Trim()
}
function Get-DoxygenPageFile([string]$RelativePage) {
    $path = Join-Path $Root $RelativePage

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Canonical algorithm page not found: '$RelativePage'."
    }

    $text =
        [System.IO.File]::ReadAllText(
            $path,
            [System.Text.Encoding]::UTF8)

    $explicit =
        [regex]::Match(
            $text,
            '(?m)^[ \t]*@page[ \t]+(?<id>[A-Za-z0-9_]+)\b')

    if ($explicit.Success) {
        return $explicit.Groups["id"].Value + ".html"
    }

    $relative =
        $RelativePage.Replace('\','/')

    if ($relative.StartsWith(
            'docs/',
            [System.StringComparison]::OrdinalIgnoreCase)) {
        $relative = $relative.Substring(5)
    }

    if ($relative.EndsWith(
            '.md',
            [System.StringComparison]::OrdinalIgnoreCase)) {
        $relative = $relative.Substring(0, $relative.Length - 3)
    }

    return 'md_' + $relative.Replace('/', '_2') + '.html'
}
function PageShell(
    [string]$title,
    [string]$body,
    [string]$relativePrefix = "") {

    $logo = "${relativePrefix}assets/metaheuristicsplatform-logo.svg"
    $css = "${relativePrefix}assets/site.css"
    $homePage = "${relativePrefix}index.html"
    $api = "${relativePrefix}api/index.html"

    return @"
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>$(Html $title) &middot; MetaheuristicsPlatform</title>
<link rel="stylesheet" href="$css">
<script>
window.MathJax = { tex: { inlineMath: [['\\(','\\)']], displayMath: [['\\[','\\]']] } };
</script>
<script defer src="https://cdn.jsdelivr.net/npm/mathjax@3.2.2/es5/tex-chtml.js"></script>
</head>
<body>
<header><div class="wrap">
<div class="brand"><a href="$homePage"><img src="$logo" alt="MetaheuristicsPlatform"></a></div>
<nav>
<a href="$homePage">Home</a>
<a href="$homePage#algorithms">Algorithms</a>
<a href="$homePage#families">Families</a>
<a href="$api">API</a>
<a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform">GitHub</a>
</nav>
</div></header>
<main class="wrap">
$body
</main>
<footer><div class="wrap">MetaheuristicsPlatform &middot; Lemoine-OR Algorithms &middot; Clean. Scientific. Open.</div></footer>
</body></html>
"@
}

& (Join-Path $Root "docs\Test-DocumentationParity.ps1") -Root $Root

$catalog =
    [System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"), [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$projectVersion = (([System.IO.File]::ReadAllText((Join-Path $Root "version.json"), [System.Text.Encoding]::UTF8) | ConvertFrom-Json).version).ToString()

$site =
    Join-Path $Root "Documentation\site"

if (Test-Path $site) {
    Remove-Item $site -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $site | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $site "assets") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $site "algorithms") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $site "families") | Out-Null

Copy-Item (Join-Path $Root "docs\portal\site.css") (Join-Path $site "assets\site.css") -Force
Copy-Item (Join-Path $Root "docs\assets\metaheuristicsplatform-logo.svg") (Join-Path $site "assets\metaheuristicsplatform-logo.svg") -Force
Copy-Item (Join-Path $Root "docs\assets\metaheuristicsplatform-favicon.svg") (Join-Path $site "assets\metaheuristicsplatform-favicon.svg") -Force
Copy-Item (Join-Path $Root "docs\assets\algorithms-icon.svg") (Join-Path $site "assets\algorithms-icon.svg") -Force
Copy-Item (Join-Path $Root "docs\algorithm-catalog.json") (Join-Path $site "algorithm-catalog.json") -Force

$familyCards = New-Object System.Collections.Generic.List[string]
foreach ($family in @($catalog.families)) {
    $count = @(
        $catalog.algorithms |
        Where-Object {
            $_.category -eq $family.id -or
            ($_.PSObject.Properties.Name -contains "additionalCategories" -and
             @($_.additionalCategories) -contains $family.id)
        }
    ).Count
    $familyCards.Add(
        "<div class='family'><h3><a href='families/$($family.id).html'>$(Html $family.name)</a></h3><div class='meta'>$(Html $family.description)<br><strong>$count public method(s)</strong></div></div>")
}

$algorithmCards = New-Object System.Collections.Generic.List[string]
foreach ($algorithm in @($catalog.algorithms)) {
    $composition = if ($algorithm.requiresComposition) { "<span class='badge'>composition</span>" } else { "" }
    $algorithmCards.Add(
        "<div class='card'><h3><a href='algorithms/$($algorithm.id).html'>$(Html $algorithm.name)</a>$composition</h3><div class='meta'>$(Html $algorithm.family)<br>$(Html $algorithm.time)<br>$(Html $algorithm.space)</div><span class='id'>$(Html $algorithm.id)</span></div>")
}

$homeBody = @"
<section class="hero"><div class="wrap">
<h1>MetaheuristicsPlatform</h1>
<p>Fast, scientific and reusable C#/.NET metaheuristics with one generic lifecycle, stable catalog IDs, literature-backed implementations and mandatory mathematical documentation.</p>
<p class="meta">Version v$projectVersion &middot; $(@($catalog.algorithms).Count) public algorithms &middot; validated documentation</p>
</div></section>
<h2 id="families">Choose a family</h2>
<div class="family-grid">$($familyCards -join "`n")</div>
<h2 id="algorithms">All algorithms</h2>
<div class="grid">$($algorithmCards -join "`n")</div>
"@

Write-Utf8 (Join-Path $site "index.html") (PageShell "Home" $homeBody "")

foreach ($family in @($catalog.families)) {
    # MULTI-FAMILY-ITEMS
    $items = @(
        $catalog.algorithms |
        Where-Object {
            $_.category -eq $family.id -or
            ($_.PSObject.Properties.Name -contains "additionalCategories" -and
             @($_.additionalCategories) -contains $family.id)
        }
    )
    $cards = New-Object System.Collections.Generic.List[string]

    foreach ($algorithm in $items) {
        $cards.Add(
            "<div class='card'><h3><a href='../algorithms/$($algorithm.id).html'>$(Html $algorithm.name)</a></h3><div class='meta'>$(Html $algorithm.time)<br>$(Html $algorithm.applicability)</div><span class='id'>$(Html $algorithm.id)</span></div>")
    }

    if ($cards.Count -eq 0) {
        $cards.Add("<div class='card'><h3>Foundation ready</h3><div class='meta'>No public algorithm is assigned to this family yet.</div></div>")
    }

    $body = "<h1>$(Html $family.name)</h1><p>$(Html $family.description)</p><div class='grid'>$($cards -join "`n")</div>"
    Write-Utf8 (Join-Path $site "families\$($family.id).html") (PageShell $family.name $body "../")
}

foreach ($algorithm in @($catalog.algorithms)) {
    $apiExample =
        Get-MarkdownApiExample ([string]$algorithm.page)

    $apiExampleHtml =
        Html $apiExample

    $doxygenPageFile =
        Get-DoxygenPageFile ([string]$algorithm.page)

    $doxygenHref =
        "../api/" + $doxygenPageFile

    $body = @"
<h1>$(Html $algorithm.name)</h1>
<div class="section">
<h2>General description</h2>
<p>$(Html $algorithm.implementation)</p>
</div>
<div class="section">
<h2>Technical specifications</h2>
<p><strong>Stable factory ID:</strong> <code>$(Html $algorithm.id)</code><br>
<strong>Class:</strong> <code>$(Html $algorithm.class)</code><br>
<strong>Family:</strong> $(Html $algorithm.family)<br>
<strong>Source:</strong> <code>$(Html $algorithm.sourcePath)</code></p>
</div>
<div class="section"><h2>Complexity</h2>
<p><strong>Time:</strong> $(Html $algorithm.time)<br><strong>Space:</strong> $(Html $algorithm.space)</p></div>
<div class="section"><h2>Applicability</h2><p>$(Html $algorithm.applicability)</p></div>
<div class="section"><h2>Detailed operation</h2>
<p>This catalog page is the concise algorithm overview. The canonical scientific page contains the complete step-by-step operation, parameter semantics, mathematical derivation, assumptions, convergence qualifications and bibliography.</p>
<p><a class="science-link" href="$doxygenHref"><strong>Open the full scientific documentation</strong></a></p>
</div>
<div class="section"><h2>Parameters</h2>
<p>Generic stopping, callbacks, deterministic randomization and cancellation are common. Exact method-specific parameters, defaults and domain-composition requirements are maintained in the canonical scientific page and generated API.</p>
<p><a href="$doxygenHref">Open parameter documentation and API details.</a></p>
</div>
<div class="section"><h2>API example</h2><pre><code>$apiExampleHtml</code></pre></div>
<div class="section">
<h2>Mathematical details</h2>
<h3>Problem formulation</h3><div class="math">\[$(Html ([string]$algorithm.problem))\]</div>
<h3>Update equations / iterations</h3><div class="math">\[$(Html ([string]$algorithm.update))\]</div>
<h3>Assumptions</h3><p>$(Html $algorithm.assumptions)</p>
<h3>Convergence conditions</h3><p>$(Html $algorithm.convergence)</p>
<h3>Scientific references</h3>
<p>$(Html $algorithm.publication)</p>
<p><strong>Principal catalog DOI:</strong> <a href="https://doi.org/$(Html $algorithm.doi)"><code>$(Html $algorithm.doi)</code></a></p>
<p><a href="$doxygenHref">Open the complete bibliography, provenance and convergence notes.</a></p>
</div>
"@

    Write-Utf8 (Join-Path $site "algorithms\$($algorithm.id).html") (PageShell $algorithm.name $body "../")
}

$doxygen =
    Get-Command doxygen -ErrorAction SilentlyContinue

if ($null -eq $doxygen) {
    if (-not $SkipDoxygenIfUnavailable) {
        throw "Doxygen is not installed. Run tools/Install-Doxygen.ps1 or pass -SkipDoxygenIfUnavailable."
    }

    New-Item -ItemType Directory -Force -Path (Join-Path $site "api") | Out-Null
    Write-Utf8 (Join-Path $site "api\index.html") (PageShell "API unavailable" "<h1>Generated API</h1><p>Doxygen was not available during this local build.</p>" "../")
}
else {
    $doxygenBuildLog =
        Join-Path $Root "Documentation\doxygen-build.log"

    if (Test-Path -LiteralPath $doxygenBuildLog) {
        Remove-Item -LiteralPath $doxygenBuildLog -Force
    }

    $doxygenOutput = @()

    Push-Location (Join-Path $Root "docs")
    try {
        & $doxygen.Source "Doxyfile" 2>&1 |
            Tee-Object -Variable doxygenOutput |
            Out-Host

        $doxygenExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    $doxygenText =
        @($doxygenOutput |
          ForEach-Object { [string]$_ })

    [System.IO.File]::WriteAllLines(
        $doxygenBuildLog,
        $doxygenText,
        [System.Text.UTF8Encoding]::new($false))

    $doxygenDiagnostics =
        @(
            $doxygenText |
            Where-Object {
                $_ -match '(?i)(^|:\s)(warning|error):'
            }
        )

    if ($doxygenExitCode -ne 0) {
        throw (
            "Doxygen build failed with exit code {0}. See {1}" -f
            $doxygenExitCode,
            $doxygenBuildLog)
    }

    if ($doxygenDiagnostics.Count -gt 0) {
        throw (
            "Doxygen emitted diagnostics despite a zero exit code. See {0}" -f
            $doxygenBuildLog)
    }

    $doxygenHtml =
        Join-Path $Root "Documentation\doxygen\html"

    if (-not (Test-Path $doxygenHtml)) {
        throw "Doxygen did not produce Documentation/doxygen/html."
    }

    Copy-Item $doxygenHtml (Join-Path $site "api") -Recurse -Force
}

& (Join-Path $Root "docs\Build-SimulatedAnnealingCoolingDocumentation.ps1") -Root $Root -Site $site
& (Join-Path $Root "docs\Build-TabuSearchAdvancedDocumentation.ps1") -Root $Root -Site $site
& (Join-Path $Root "docs\Build-PsoTopologyDocumentation.ps1") -Root $Root -Site $site
& (Join-Path $Root "docs\Build-AdvancedVariableNeighborhoodDocumentation.ps1") -Root $Root -Site $site
& (Join-Path $Root "docs\Build-PathRelinkingStrategyDocumentation.ps1") -Root $Root -Site $site
# Inject the project favicon into every generated HTML page, including Doxygen output.
$allHtmlPages = Get-ChildItem -LiteralPath $site -Recurse -Filter "*.html" -File
foreach ($htmlPage in $allHtmlPages) {
    $htmlText = [System.IO.File]::ReadAllText($htmlPage.FullName, [System.Text.Encoding]::UTF8)
    if (-not $htmlText.Contains('metaheuristicsplatform-favicon.svg')) {
        $relativeFile = $htmlPage.FullName.Substring($site.Length).TrimStart('\','/')
        $depth = ([regex]::Matches($relativeFile, '[\\/]').Count)
        $prefix = ""
        for ($i = 0; $i -lt $depth; $i++) { $prefix += "../" }
        $iconTag = '<link rel="icon" type="image/svg+xml" href="' + $prefix + 'assets/metaheuristicsplatform-favicon.svg">'
        $htmlText = $htmlText.Replace('</head>', $iconTag + "`n</head>")
        Write-Utf8 $htmlPage.FullName $htmlText
    }
}

& (Join-Path $Root "docs\Test-RenderedPortalQuality.ps1") -Root $Root -Site $site
& (Join-Path $Root "docs\Test-TextEncoding.ps1") -Root $Root -AdditionalPath $site
& (Join-Path $Root "docs\Test-DocumentationLinks.ps1") -Root $Root

Write-Host ""
Write-Host "MetaheuristicsPlatform documentation successfully built and validated." -ForegroundColor Green
Write-Host "Portal: $site"
Write-Host "Algorithms: $(@($catalog.algorithms).Count)"
Write-Host "Families: $(@($catalog.families).Count)"
