[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Site = (Join-Path (Split-Path -Parent $PSScriptRoot) "Documentation\site")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8Path([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Rendered portal quality: missing '$Path'."
    }

    return [System.IO.File]::ReadAllText(
        $Path,
        [System.Text.Encoding]::UTF8)
}

function Get-MarkdownApiExample([string]$RelativePage) {
    $path = Join-Path $Root $RelativePage
    $text = Read-Utf8Path $path

    $section = [regex]::Match(
        $text,
        '(?ms)^##[ \t]+API example[ \t]*\r?\n(?<body>.*?)(?=^##[ \t]+|\z)')

    if (-not $section.Success) {
        throw "Rendered portal quality: '$RelativePage' has no API example section."
    }

    $code = [regex]::Match(
        $section.Groups["body"].Value,
        '(?ms)```(?:csharp)?[ \t]*\r?\n(?<code>.*?)\r?\n```[ \t]*$')

    if (-not $code.Success) {
        throw "Rendered portal quality: '$RelativePage' has no fenced C# API example."
    }

    return $code.Groups["code"].Value.Trim()
}

function Get-DoxygenPageFile([string]$RelativePage) {
    $path = Join-Path $Root $RelativePage
    $text = Read-Utf8Path $path

    $explicit =
        [regex]::Match(
            $text,
            '(?m)^[ \t]*@page[ \t]+(?<id>[A-Za-z0-9_]+)\b')

    if ($explicit.Success) {
        return $explicit.Groups["id"].Value + ".html"
    }

    $relative =
        $RelativePage.Replace('\','/')

    if ($relative.StartsWith('docs/', [System.StringComparison]::OrdinalIgnoreCase)) {
        $relative = $relative.Substring(5)
    }

    if ($relative.EndsWith('.md', [System.StringComparison]::OrdinalIgnoreCase)) {
        $relative = $relative.Substring(0, $relative.Length - 3)
    }

    return 'md_' + $relative.Replace('/', '_2') + '.html'
}

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog =
    (Read-Utf8Path $catalogPath) |
    ConvertFrom-Json

$checkedFormulaBlocks = 0
$checkedApiExamples = 0
$checkedScienceLinks = 0

foreach ($algorithm in @($catalog.algorithms)) {
    $pagePath =
        Join-Path $Site ("algorithms\" + [string]$algorithm.id + ".html")

    $html = Read-Utf8Path $pagePath

    if (-not $html.Contains('mathjax@3.2.2/es5/tex-chtml.js')) {
        throw "Rendered portal quality: '$($algorithm.id)' is not using pinned MathJax 3.2.2."
    }

    $expectedProblem =
        '<div class="math">\[' +
        [System.Net.WebUtility]::HtmlEncode([string]$algorithm.problem) +
        '\]</div>'

    $expectedUpdate =
        '<div class="math">\[' +
        [System.Net.WebUtility]::HtmlEncode([string]$algorithm.update) +
        '\]</div>'

    if (-not $html.Contains($expectedProblem)) {
        throw "Rendered portal quality: '$($algorithm.id)' problem formula does not match the catalog."
    }

    if (-not $html.Contains($expectedUpdate)) {
        throw "Rendered portal quality: '$($algorithm.id)' update formula does not match the catalog."
    }

    $blocks =
        [regex]::Matches(
            $html,
            '(?s)<div class="math">(?<math>.*?)</div>')

    if ($blocks.Count -ne 2) {
        throw "Rendered portal quality: '$($algorithm.id)' must contain exactly two primary math blocks."
    }

    foreach ($block in $blocks) {
        $inner = $block.Groups["math"].Value

        if ($inner -match '<(?!/?(?:span|mjx))') {
            throw "Rendered portal quality: '$($algorithm.id)' contains raw HTML markup inside a math block."
        }

        $checkedFormulaBlocks++
    }

    if ($html.Contains('MetaheuristicFactory.Create&lt;...&gt;')) {
        throw "Rendered portal quality: '$($algorithm.id)' still exposes the invalid generic API placeholder."
    }

    $canonicalApi =
        Get-MarkdownApiExample ([string]$algorithm.page)

    $encodedApi =
        [System.Net.WebUtility]::HtmlEncode($canonicalApi)

    $normalizedHtml = $html.Replace("`r`n", "`n")
    $normalizedApi = $encodedApi.Replace("`r`n", "`n")

    if (-not $normalizedHtml.Contains("<pre><code>$normalizedApi</code></pre>")) {
        throw "Rendered portal quality: '$($algorithm.id)' portal API example is not synchronized with its canonical Markdown page."
    }

    $checkedApiExamples++

    $doxygenFile =
        Get-DoxygenPageFile ([string]$algorithm.page)

    $expectedHref =
        '../api/' + $doxygenFile

    if (-not $html.Contains('class="science-link"') -or
        -not $html.Contains('href="' + $expectedHref + '"')) {
        throw "Rendered portal quality: '$($algorithm.id)' is missing its canonical scientific-documentation link."
    }

    $target =
        Join-Path $Site ("api\" + $doxygenFile)

    if (-not (Test-Path -LiteralPath $target)) {
        throw "Rendered portal quality: canonical Doxygen target is missing for '$($algorithm.id)': '$doxygenFile'."
    }

    $checkedScienceLinks++
}

$componentChecks = @(
    @{
        catalog = "docs\sa-cooling-catalog.json"
        page = "components\simulated-annealing-cooling-schedules.html"
    },
    @{
        catalog = "docs\threshold-accepting-schedule-catalog.json"
        page = "components\threshold-accepting-schedules.html"
    },    @{
        catalog = "docs\acceptance-based-trajectory-catalog.json"
        page = "components\acceptance-based-trajectory-methods.html"
    },    @{
        catalog = "docs\ts-memory-control-catalog.json"
        page = "components\tabu-search-memory-control-strategies.html"
    }
)

$componentMathBlocks = 0
$componentProseModels = 0

foreach ($check in $componentChecks) {
    $componentCatalog =
        (Read-Utf8Path (Join-Path $Root $check.catalog)) |
        ConvertFrom-Json

    $componentHtml =
        Read-Utf8Path (Join-Path $Site $check.page)

    if (-not $componentHtml.Contains('mathjax@3.2.2/es5/tex-chtml.js')) {
        throw "Rendered portal quality: '$($check.page)' is not using pinned MathJax 3.2.2."
    }

    foreach ($entry in @($componentCatalog.entries)) {
        $encoded =
            [System.Net.WebUtility]::HtmlEncode([string]$entry.formula)

        switch ([string]$entry.formulaMode) {
            "math" {
                $expected =
                    '<div class="math">\[' +
                    $encoded +
                    '\]</div>'

                if (-not $componentHtml.Contains($expected)) {
                    throw "Rendered portal quality: component '$($entry.id)' math formula is not rendered as display mathematics."
                }

                $componentMathBlocks++
            }

            "prose" {
                if (-not $componentHtml.Contains('<div class="formula-note">') -or
                    -not $componentHtml.Contains($encoded)) {
                    throw "Rendered portal quality: component '$($entry.id)' prose model is missing."
                }

                $mathWrapped =
                    '<div class="math">\[' +
                    $encoded +
                    '\]</div>'

                if ($componentHtml.Contains($mathWrapped)) {
                    throw "Rendered portal quality: prose component '$($entry.id)' is incorrectly wrapped in MathJax."
                }

                $componentProseModels++
            }

            default {
                throw "Rendered portal quality: unsupported formulaMode '$($entry.formulaMode)' for '$($entry.id)'."
            }
        }
    }
}

$thresholdAcceptingPortal =
    Read-Utf8Path (Join-Path $Site "algorithms\threshold-accepting-dueck-scheuer-1990.html")

if (-not $thresholdAcceptingPortal.Contains(
    '../components/threshold-accepting-schedules.html')) {
    throw "Rendered portal quality: Threshold Accepting portal page is missing the schedule-catalog link."
}

$thresholdAcceptingDoxygen =
    Join-Path $Site "api\threshold_accepting_schedules.html"

if (-not (Test-Path -LiteralPath $thresholdAcceptingDoxygen)) {
    throw "Rendered portal quality: canonical Threshold Accepting schedule Doxygen page is missing."
}
foreach ($dueckAlgorithm in @(
    "great-deluge-dueck-1993",
    "record-to-record-travel-dueck-1993"
)) {
    $dueckPortal =
        Read-Utf8Path (Join-Path $Site ("algorithms\" + $dueckAlgorithm + ".html"))

    if (-not $dueckPortal.Contains(
        '../components/acceptance-based-trajectory-methods.html')) {
        throw "Rendered portal quality: '$dueckAlgorithm' is missing its acceptance-family link."
    }
}

$acceptanceDoxygen =
    Join-Path $Site "api\acceptance_based_trajectory_methods.html"

if (-not (Test-Path -LiteralPath $acceptanceDoxygen)) {
    throw "Rendered portal quality: canonical acceptance-based trajectory Doxygen page is missing."
}
$pathRelinkingCatalog =
    (Read-Utf8Path (Join-Path $Root "docs\path-relinking-strategy-catalog.json")) |
    ConvertFrom-Json

$pathRelinkingHtml =
    Read-Utf8Path (Join-Path $Site "components\path-relinking-strategies.html")

if (-not $pathRelinkingHtml.Contains('mathjax@3.2.2/es5/tex-chtml.js')) {
    throw "Rendered portal quality: Path Relinking component is not using pinned MathJax 3.2.2."
}

foreach ($entry in @($pathRelinkingCatalog.implemented)) {
    $encoded =
        [System.Net.WebUtility]::HtmlEncode([string]$entry.formula)

    $expected =
        '<div class="math">\[' +
        $encoded +
        '\]</div>'

    if (-not $pathRelinkingHtml.Contains($expected)) {
        throw "Rendered portal quality: Path Relinking component '$($entry.id)' math formula is not rendered as display mathematics."
    }

    $componentMathBlocks++
}

foreach ($entry in @($pathRelinkingCatalog.reviewedDeferred)) {
    $encoded =
        [System.Net.WebUtility]::HtmlEncode([string]$entry.formula)

    if (-not $pathRelinkingHtml.Contains('<div class="formula-note">') -or
        -not $pathRelinkingHtml.Contains($encoded)) {
        throw "Rendered portal quality: Path Relinking component '$($entry.id)' prose model is missing."
    }

    $mathWrapped =
        '<div class="math">\[' +
        $encoded +
        '\]</div>'

    if ($pathRelinkingHtml.Contains($mathWrapped)) {
        throw "Rendered portal quality: Path Relinking prose component '$($entry.id)' is incorrectly wrapped in MathJax."
    }

    $componentProseModels++
}

$graspPathRelinkingPortal =
    Read-Utf8Path (Join-Path $Site "algorithms\grasp-path-relinking.html")

if (-not $graspPathRelinkingPortal.Contains(
    '../components/path-relinking-strategies.html')) {
    throw "Rendered portal quality: GRASP-PR portal page is missing the Advanced Path Relinking component link."
}

$pathRelinkingDoxygen =
    Join-Path $Site "api\path_relinking_strategies.html"

if (-not (Test-Path -LiteralPath $pathRelinkingDoxygen)) {
    throw "Rendered portal quality: canonical Path Relinking Doxygen page is missing."
}
$apiHtmlFiles =
    @(
        Get-ChildItem `
            -LiteralPath (Join-Path $Site "api") `
            -Recurse `
            -File `
            -Filter "*.html"
    )

$mathJaxApiPages = 0

foreach ($apiPage in $apiHtmlFiles) {
    $apiHtml =
        [System.IO.File]::ReadAllText(
            $apiPage.FullName,
            [System.Text.Encoding]::UTF8)

    $mathJaxScriptMatches =
        [regex]::Matches(
            $apiHtml,
            '<script\b[^>]*\bsrc=["''](?<src>[^"'']*mathjax[^"'']*)["''][^>]*>',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if ($mathJaxScriptMatches.Count -gt 0) {
        $mathJaxApiPages++

        $validPinnedScript = $false
        $observedSources =
            New-Object System.Collections.Generic.List[string]

        foreach ($scriptMatch in $mathJaxScriptMatches) {
            $src =
                [System.Net.WebUtility]::HtmlDecode(
                    $scriptMatch.Groups["src"].Value)

            $observedSources.Add($src)

            if ($src -match '(?i)/es5/es5/') {
                throw (
                    "Rendered portal quality: Doxygen page '{0}' contains a duplicated MathJax /es5/es5/ path: {1}" -f
                    $apiPage.Name,
                    $src)
            }

            if ($src -match '(?i)https://cdn\.jsdelivr\.net/npm/mathjax@3\.2\.2/es5/tex(?:-mml)?-chtml(?:-full)?\.js(?:[?#].*)?$') {
                $validPinnedScript = $true
                break
            }
        }

        if (-not $validPinnedScript) {
            throw (
                "Rendered portal quality: Doxygen page '{0}' does not use an accepted pinned MathJax 3.2.2 CHTML bundle. Observed: {1}" -f
                $apiPage.Name,
                ($observedSources -join ', '))
        }
    }
}

if ($mathJaxApiPages -eq 0) {
    throw "Rendered portal quality: no MathJax-enabled Doxygen pages were found."
}

Write-Host (
    "Rendered portal quality validation passed: {0} algorithm pages, {1} algorithm formula blocks, {2} canonical API examples, {3} scientific links, {4} component math models, {5} prose composite models, {6} pinned-MathJax Doxygen pages." -f
    @($catalog.algorithms).Count,
    $checkedFormulaBlocks,
    $checkedApiExamples,
    $checkedScienceLinks,
    $componentMathBlocks,
    $componentProseModels,
    $mathJaxApiPages
) -ForegroundColor Green
