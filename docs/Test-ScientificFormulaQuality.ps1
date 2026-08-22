[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Scientific formula quality: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Assert-BalancedBraces(
    [string]$Id,
    [string]$Field,
    [string]$Formula) {

    $depth = 0

    foreach ($character in $Formula.ToCharArray()) {
        if ($character -eq '{') {
            $depth++
        }
        elseif ($character -eq '}') {
            $depth--
            if ($depth -lt 0) {
                throw "Scientific formula quality: '$Id/$Field' has an unmatched closing brace."
            }
        }
    }

    if ($depth -ne 0) {
        throw "Scientific formula quality: '$Id/$Field' has unbalanced braces."
    }
}

function Assert-TeXContent(
    [string]$Id,
    [string]$Field,
    [string]$Formula) {

    if ([string]::IsNullOrWhiteSpace($Formula)) {
        throw "Scientific formula quality: '$Id' has empty '$Field' mathematics."
    }

    if ($Formula.Contains('\[') -or
        $Formula.Contains('\]') -or
        $Formula.Contains('\f[') -or
        $Formula.Contains('\f]') -or
        $Formula.Contains('$$')) {
        throw "Scientific formula quality: '$Id/$Field' must contain TeX content only, without outer delimiters."
    }

    Assert-BalancedBraces -Id $Id -Field $Field -Formula $Formula

    foreach ($rule in @(
        '(?i)\bmin\s+f\s*\(',
        '(?i)(?<!\\)\bx\s+in\s+x\b',
        '(?i)\bShake\b',
        '(?i)\bcompute\b',
        '(?i)\baccept\s+strict\b',
        '(?i)\breset\s+k\b',
        '(?i)\botherwise\s+increment\b',
        '(?i)\bargbest\b',
        '(?i)\brequire\b',
        '(?i)(?<!\\)\bsum_j\b',
        '(?i)(?<!\\)\bc_min\b',
        '(?i)(?<!\\)\bc_max\b',
        '(?i)(?<!\\)\balpha\b',
        '(?i)(?<!\\)\bbeta\b',
        '(?i)(?<!\\)\blambda\b',
        '(?i)(?<!\\)\brho\b',
        '(?i)(?<!\\)\bln\s*\(',
        '(?i)(?<!\\)\bexp\s*\('
    )) {
        if ([regex]::IsMatch($Formula, $rule)) {
            throw "Scientific formula quality: '$Id/$Field' contains pseudo-mathematics: $Formula"
        }
    }
}

function Get-MarkdownApiExample([string]$RelativePage) {
    $text = Read-Utf8 $RelativePage

    $section = [regex]::Match(
        $text,
        '(?ms)^##[ \t]+API example[ \t]*\r?\n(?<body>.*?)(?=^##[ \t]+|\z)')

    if (-not $section.Success) {
        throw "Scientific formula quality: '$RelativePage' has no API example section."
    }

    $code = [regex]::Match(
        $section.Groups["body"].Value,
        '(?ms)```(?:csharp)?[ \t]*\r?\n(?<code>.*?)\r?\n```[ \t]*$')

    if (-not $code.Success) {
        throw "Scientific formula quality: '$RelativePage' has no fenced C# API example."
    }

    return $code.Groups["code"].Value.Trim()
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$algorithms = @($catalog.algorithms)

if ($algorithms.Count -lt 40) {
    throw "Scientific formula quality: expected at least 40 public algorithms."
}

foreach ($algorithm in $algorithms) {
    Assert-TeXContent `
        -Id ([string]$algorithm.id) `
        -Field "problem" `
        -Formula ([string]$algorithm.problem)

    Assert-TeXContent `
        -Id ([string]$algorithm.id) `
        -Field "update" `
        -Formula ([string]$algorithm.update)

    if (-not ([string]$algorithm.update).Contains('\begin{aligned}')) {
        throw "Scientific formula quality: '$($algorithm.id)' update mathematics must use an aligned display layout."
    }

    $apiExample = Get-MarkdownApiExample ([string]$algorithm.page)

    if ($apiExample.Contains('MetaheuristicFactory.Create<...>')) {
        throw "Scientific formula quality: '$($algorithm.id)' canonical API example still contains the generic placeholder."
    }
}

$componentMathCount = 0
$componentProseCount = 0

foreach ($catalogRelative in @(
    "docs\sa-cooling-catalog.json",
    "docs\threshold-accepting-schedule-catalog.json",
    "docs\acceptance-based-trajectory-catalog.json",
    "docs\ts-memory-control-catalog.json",
    "docs\advanced-iterated-greedy-catalog.json",
    "docs\advanced-scatter-search-catalog.json",
    "docs\advanced-genetic-algorithm-catalog.json",
    "docs\memetic-algorithm-catalog.json",
    "docs\advanced-ant-colony-optimization-catalog.json",
    "docs\cma-es-component-catalog.json"
)) {
    $componentCatalog =
        (Read-Utf8 $catalogRelative) |
        ConvertFrom-Json

    foreach ($entry in @($componentCatalog.entries)) {
        $modeProperty =
            $entry.PSObject.Properties["formulaMode"]

        if ($null -eq $modeProperty) {
            throw "Scientific formula quality: '$($entry.id)' is missing formulaMode."
        }

        $mode = [string]$modeProperty.Value
        $formula = [string]$entry.formula

        switch ($mode) {
            "math" {
                Assert-TeXContent `
                    -Id ([string]$entry.id) `
                    -Field "formula" `
                    -Formula $formula
                $componentMathCount++
            }

            "prose" {
                if ([string]::IsNullOrWhiteSpace($formula)) {
                    throw "Scientific formula quality: '$($entry.id)' has empty prose model description."
                }

                if ($formula.Contains('\begin') -or
                    $formula.Contains('\frac') -or
                    $formula.Contains('\sum')) {
                    throw "Scientific formula quality: '$($entry.id)' is marked prose but still contains TeX mathematics."
                }

                $componentProseCount++
            }

            default {
                throw "Scientific formula quality: '$($entry.id)' has unsupported formulaMode '$mode'."
            }
        }
    }
}

$pathRelinkingCatalog =
    (Read-Utf8 "docs\path-relinking-strategy-catalog.json") |
    ConvertFrom-Json

$pathRelinkingEntries =
    @($pathRelinkingCatalog.implemented) +
    @($pathRelinkingCatalog.reviewedDeferred)

foreach ($entry in $pathRelinkingEntries) {
    $mode = [string]$entry.formulaMode
    $formula = [string]$entry.formula

    switch ($mode) {
        "math" {
            Assert-TeXContent `
                -Id ([string]$entry.id) `
                -Field "formula" `
                -Formula $formula
            $componentMathCount++
        }

        "prose" {
            if ([string]::IsNullOrWhiteSpace($formula)) {
                throw "Scientific formula quality: '$($entry.id)' has empty prose model description."
            }

            if ($formula.Contains('\begin') -or
                $formula.Contains('\frac') -or
                $formula.Contains('\sum')) {
                throw "Scientific formula quality: '$($entry.id)' is marked prose but still contains TeX mathematics."
            }

            $componentProseCount++
        }

        default {
            throw "Scientific formula quality: '$($entry.id)' has unsupported formulaMode '$mode'."
        }
    }
}
$builder = Read-Utf8 "docs\build-documentation.ps1"

foreach ($marker in @(
    'Get-MarkdownApiExample',
    'Get-DoxygenPageFile',
    'Html ([string]$algorithm.problem)',
    'Html ([string]$algorithm.update)',
    '$apiExampleHtml',
    'science-link',
    'mathjax@3.2.2/es5/tex-chtml.js'
)) {
    if (-not $builder.Contains($marker)) {
        throw "Scientific formula quality: documentation builder is missing '$marker'."
    }
}

$implementationCount =
    [regex]::Matches(
        $builder,
        '\$algorithm\.implementation').Count

if ($implementationCount -ne 1) {
    throw "Scientific formula quality: portal builder must render the catalog implementation summary exactly once; found $implementationCount occurrences."
}

foreach ($specialBuilder in @(
    "docs\Build-SimulatedAnnealingCoolingDocumentation.ps1",
    "docs\Build-ThresholdAcceptingScheduleDocumentation.ps1",
    "docs\Build-AcceptanceBasedTrajectoryDocumentation.ps1",
    "docs\Build-TabuSearchAdvancedDocumentation.ps1",
    "docs\Build-PathRelinkingStrategyDocumentation.ps1",
    "docs\Build-AdvancedIteratedGreedyDocumentation.ps1",
    "docs\Build-AdvancedScatterSearchDocumentation.ps1",
    "docs\Build-AdvancedGeneticAlgorithmDocumentation.ps1",
    "docs\Build-MemeticAlgorithmDocumentation.ps1",
    "docs\Build-AdvancedAntColonyDocumentation.ps1",
    "docs\Build-CmaEsDocumentation.ps1"
)) {
    $specialSource = Read-Utf8 $specialBuilder

    foreach ($marker in @(
        'formulaMode',
        'formula-note',
        'mathjax@3.2.2/es5/tex-chtml.js'
    )) {
        if (-not $specialSource.Contains($marker)) {
            throw "Scientific formula quality: '$specialBuilder' is missing '$marker'."
        }
    }
}

$doxyfile = Read-Utf8 "docs\Doxyfile"

foreach ($marker in @(
    'MATHJAX_VERSION        = MathJax_3',
    'MATHJAX_FORMAT         = chtml',
    'MATHJAX_RELPATH        = https://cdn.jsdelivr.net/npm/mathjax@3.2.2'
)) {
    if (-not $doxyfile.Contains($marker)) {
        throw "Scientific formula quality: Doxyfile is missing pinned MathJax marker '$marker'."
    }
}

Write-Host (
    "Scientific formula quality validation passed: {0} algorithms, {1} aligned update blocks, {2} component math models, {3} prose-only composite models." -f
    $algorithms.Count,
    $algorithms.Count,
    $componentMathCount,
    $componentProseCount
) -ForegroundColor Green
