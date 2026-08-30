[CmdletBinding()]
param(
    [string]$Root = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($Root)) {
    if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        throw "Repository root is required."
    }

    $Root =
        Split-Path `
            -Parent `
            $PSScriptRoot
}

$Root =
    [System.IO.Path]::GetFullPath(
        $Root)

function Get-HistoricalReadmeRequirements([string]$RepositoryRoot) {
    $docsRoot =
        Join-Path `
            $RepositoryRoot `
            "docs"

    $testScripts =
        @(
            Get-ChildItem `
                -LiteralPath $docsRoot `
                -Filter "Test-*.ps1" `
                -File |
            Sort-Object FullName
        )

    if (@($testScripts).Length -eq 0) {
        throw "README compatibility extraction: no docs/Test-*.ps1 scripts were found."
    }

    $requirements = @()

    foreach ($testScript in $testScripts) {
        $tokens = $null
        $parseErrors = $null

        $ast =
            [System.Management.Automation.Language.Parser]::ParseFile(
                [string]$testScript.FullName,
                [ref]$tokens,
                [ref]$parseErrors)

        if (@($parseErrors).Length -ne 0) {
            throw (
                "README compatibility extraction: validator '{0}' does not parse." -f
                [string]$testScript.Name)
        }

        # Primary contract form used by historical validators:
        #
        # Require-Contains `
        #     "README.md" @(
        #         "marker-a",
        #         "marker-b"
        #     )
        #
        # Do not key this to a helper function name. Any command invocation
        # containing the exact README.md literal is inspected; string literals
        # occurring after README.md within that same CommandAst are preserved.
        $commands =
            @(
                $ast.FindAll(
                    {
                        param($node)

                        return (
                            $node -is
                            [System.Management.Automation.Language.CommandAst])
                    },
                    $true)
            )

        foreach ($command in $commands) {
            $stringNodes =
                @(
                    $command.FindAll(
                        {
                            param($node)

                            return (
                                $node -is
                                [System.Management.Automation.Language.StringConstantExpressionAst] -or
                                $node -is
                                [System.Management.Automation.Language.ExpandableStringExpressionAst])
                        },
                        $true) |
                    Sort-Object {
                        $_.Extent.StartOffset
                    }
                )

            $readmeNodes =
                @(
                    $stringNodes |
                    Where-Object {
                        [string]::Equals(
                            [string]$_.Value,
                            "README.md",
                            [System.StringComparison]::OrdinalIgnoreCase)
                    }
                )

            if (@($readmeNodes).Length -eq 0) {
                continue
            }

            $readmeNode =
                $readmeNodes[0]

            $readmeOffset =
                [int]$readmeNode.Extent.StartOffset

            foreach ($stringNode in $stringNodes) {
                if ([int]$stringNode.Extent.StartOffset -le
                    $readmeOffset) {

                    continue
                }

                $value =
                    [string]$stringNode.Value

                if ([string]::IsNullOrWhiteSpace(
                        $value)) {

                    continue
                }

                $requirements +=
                    $value
            }
        }

        # Secondary compatibility form retained deliberately. It was the v4
        # assumption and may still be useful if a validator later stores
        # per-file requirements in a hashtable keyed by README.md.
        $hashtables =
            @(
                $ast.FindAll(
                    {
                        param($node)

                        return (
                            $node -is
                            [System.Management.Automation.Language.HashtableAst])
                    },
                    $true)
            )

        foreach ($hashtable in $hashtables) {
            foreach ($pair in $hashtable.KeyValuePairs) {
                $keyAst =
                    $pair.Item1

                $keyValue = ""

                if ($keyAst -is
                    [System.Management.Automation.Language.StringConstantExpressionAst]) {

                    $keyValue =
                        [string]$keyAst.Value
                }
                elseif ($keyAst -is
                    [System.Management.Automation.Language.ExpandableStringExpressionAst]) {

                    $keyValue =
                        [string]$keyAst.Value
                }

                if (-not [string]::Equals(
                        $keyValue,
                        "README.md",
                        [System.StringComparison]::OrdinalIgnoreCase)) {

                    continue
                }

                $stringNodes =
                    @(
                        $pair.Item2.FindAll(
                            {
                                param($node)

                                return (
                                    $node -is
                                    [System.Management.Automation.Language.StringConstantExpressionAst] -or
                                    $node -is
                                    [System.Management.Automation.Language.ExpandableStringExpressionAst])
                            },
                            $true) |
                        Sort-Object {
                            $_.Extent.StartOffset
                        }
                    )

                foreach ($stringNode in $stringNodes) {
                    $value =
                        [string]$stringNode.Value

                    if ([string]::IsNullOrWhiteSpace(
                            $value)) {

                        continue
                    }

                    $requirements +=
                        $value
                }
            }
        }
    }

    $requirements |
        Sort-Object -Unique
}

$builderPath =$builderPath =
    Join-Path `
        $Root `
        "docs\Build-ProfessionalReadme.ps1"

& $builderPath `
    -Root $Root `
    -Check

$catalog =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "docs\algorithm-catalog.json"),
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$algorithms =
    @(
        $catalog.algorithms
    )

$families =
    @(
        $catalog.families
    )

if (@($algorithms).Length -ne 155) {
    throw (
        "Professional README parity: expected exactly 155 algorithms; found {0}." -f
        @($algorithms).Length)
}

if (@($families).Length -ne 8) {
    throw (
        "Professional README parity: expected exactly 8 families; found {0}." -f
        @($families).Length)
}

$readme =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "README.md"),
        [System.Text.Encoding]::UTF8)

$normalized =
    $readme.Replace(
        "`r`n",
        "`n")

$normalized =
    $normalized.Replace(
        "`r",
        "`n")

foreach ($forbidden in @(
    'colspan=',
    'width="20%"',
    'width="25%"',
    'width="33%"',
    'width="100%"'
)) {
    if ($normalized.IndexOf(
            $forbidden,
            [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {

        throw (
            "Professional README parity: non-uniform table layout token is forbidden: {0}" -f
            $forbidden)
    }
}

$requiredSections =
    @(
        "## Optimization capabilities",
        "## Multi-objective and many-objective optimization",
        "## Constraint handling and multimodal optimization",
        "## Hyper-heuristics and matheuristics",
        "## Complete scientific taxonomy",
        "## Why MetaheuristicsPlatform?",
        "## v1.0 stability contract",
        "## Start in 30 seconds",
        "## All algorithms",
        "## Scientific components",
        "## Build and validate",
        "## Documentation and scientific provenance",
        "## Keywords and discoverability"
    )

foreach ($section in $requiredSections) {
    if ($normalized.IndexOf(
            $section,
            [System.StringComparison]::Ordinal) -lt 0) {

        throw (
            "Professional README parity: missing section '{0}'." -f
            $section)
    }
}

$requiredDynamicQualityMarkers =
    @(
        ("{0} public algorithms" -f @($algorithms).Length),
        ("{0} swarm methods" -f @(
            $algorithms |
            Where-Object {
                [string]$_.category -eq
                "swarm-intelligence"
            }
        ).Length),
        ("{0} evolutionary methods" -f @(
            $algorithms |
            Where-Object {
                [string]$_.category -eq
                "evolutionary-methods"
            }
        ).Length),
        ("{0} trajectory methods" -f @(
            $algorithms |
            Where-Object {
                [string]$_.category -eq
                "trajectory-based-methods"
            }
        ).Length)
    )

foreach ($dynamicQualityMarker in $requiredDynamicQualityMarkers) {
    if ($normalized.IndexOf(
            $dynamicQualityMarker,
            [System.StringComparison]::Ordinal) -lt 0) {

        throw (
            "Professional README parity: historical README-quality marker missing: '{0}'." -f
            $dynamicQualityMarker)
    }
}

$requiredScientificTerms =
    @(
        "single-objective",
        "multi-objective",
        "many-objective",
        "Pareto optimization",
        "constraint handling",
        "multimodal optimization",
        "niching",
        "hyper-heuristics",
        "adaptive operator selection",
        "matheuristics",
        "mathematical programming",
        "mixed-integer programming",
        "exact repair",
        "NSGA-II",
        "MOEA/D",
        "MOPSO",
        "SMPSO",
        "NSGA-III",
        "RVEA",
        "SPEA2",
        "MO-CMA-ES",
        "HypE",
        "Local Branching",
        "Feasibility Pump",
        "Kernel Search",
        "Proximity Search",
        "Kernel Pump",
        "simulated annealing",
        "tabu search",
        "variable neighborhood search",
        "large neighborhood search",
        "GRASP",
        "memetic algorithms",
        "CMA-ES",
        "differential evolution",
        "particle swarm optimization",
        "ant colony optimization",
        "operations research",
        "reproducible optimization"
    )

foreach ($term in $requiredScientificTerms) {
    if ($normalized.IndexOf(
            $term,
            [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {

        throw (
            "Professional README parity: missing scientific/discoverability term '{0}'." -f
            $term)
    }
}

$beginMarker =
    "<!-- PROFESSIONAL-ALGORITHM-CATALOG-BEGIN -->"

$endMarker =
    "<!-- PROFESSIONAL-ALGORITHM-CATALOG-END -->"

$beginIndex =
    $normalized.IndexOf(
        $beginMarker,
        [System.StringComparison]::Ordinal)

$endIndex =
    $normalized.IndexOf(
        $endMarker,
        [System.StringComparison]::Ordinal)

if ($beginIndex -lt 0 -or
    $endIndex -lt 0 -or
    $endIndex -le $beginIndex) {

    throw "Professional README parity: algorithm-catalog markers are missing or out of order."
}

if ($normalized.IndexOf(
        $beginMarker,
        $beginIndex + $beginMarker.Length,
        [System.StringComparison]::Ordinal) -ge 0 -or
    $normalized.IndexOf(
        $endMarker,
        $endIndex + $endMarker.Length,
        [System.StringComparison]::Ordinal) -ge 0) {

    throw "Professional README parity: algorithm-catalog markers are not unique."
}

$catalogRegion =
    $normalized.Substring(
        $beginIndex,
        ($endIndex + $endMarker.Length) -
        $beginIndex)

$cardMatches =
    @(
        [regex]::Matches(
            $catalogRegion,
            'data-stable-id="([^"]+)"')
    )

if (@($cardMatches).Length -ne
    @($algorithms).Length) {

    throw (
        "Professional README parity: expected {0} catalog cards, found {1}." -f
        @($algorithms).Length,
        @($cardMatches).Length)
}

foreach ($algorithm in $algorithms) {
    $token =
        'data-stable-id="' +
        [string]$algorithm.id +
        '"'

    $matches =
        @(
            [regex]::Matches(
                $catalogRegion,
                [regex]::Escape(
                    $token))
        )

    if (@($matches).Length -ne 1) {
        throw (
            "Professional README parity: stable ID '{0}' must appear exactly once as a catalog card; found {1}." -f
            [string]$algorithm.id,
            @($matches).Length)
    }
}

$expectedRows =
    0

foreach ($family in $families) {
    $familyAlgorithms =
        @(
            $algorithms |
            Where-Object {
                [string]$_.category -eq
                [string]$family.id
            }
        )

    $count =
        @($familyAlgorithms).Length

    $heading =
        "### " +
        [string]$family.name +
        " (" +
        $count +
        ")"

    if ($catalogRegion.IndexOf(
            $heading,
            [System.StringComparison]::Ordinal) -lt 0) {

        throw (
            "Professional README parity: family heading missing or count drifted: '{0}'." -f
            $heading)
    }

    $rows =
        [int](
            [Math]::Ceiling(
                $count /
                2.0))

    $expectedRows +=
        $rows
}

$rowMatches =
    @(
        [regex]::Matches(
            $catalogRegion,
            '(?m)^<tr>$')
    )

if (@($rowMatches).Length -ne
    $expectedRows) {

    throw (
        "Professional README parity: expected {0} two-column catalog rows, found {1}." -f
        $expectedRows,
        @($rowMatches).Length)
}

$catalogCellMatches =
    @(
        [regex]::Matches(
            $catalogRegion,
            '<td width="50%" valign="top"')
    )

if (@($catalogCellMatches).Length -ne
    (2 * $expectedRows)) {

    throw (
        "Professional README parity: expected {0} 50%-width catalog cells, found {1}." -f
        (2 * $expectedRows),
        @($catalogCellMatches).Length)
}

$taxonomyStart =
    $normalized.IndexOf(
        "## Complete scientific taxonomy",
        [System.StringComparison]::Ordinal)

$taxonomyEnd =
    $normalized.IndexOf(
        "## Why MetaheuristicsPlatform?",
        [System.StringComparison]::Ordinal)

if ($taxonomyStart -lt 0 -or
    $taxonomyEnd -le $taxonomyStart) {

    throw "Professional README parity: taxonomy region cannot be resolved."
}

$taxonomyRegion =
    $normalized.Substring(
        $taxonomyStart,
        $taxonomyEnd -
        $taxonomyStart)

foreach ($family in $families) {
    $familyName =
        [string]$family.name

    if ($taxonomyRegion.IndexOf(
            $familyName,
            [System.StringComparison]::Ordinal) -lt 0) {

        throw (
            "Professional README parity: taxonomy does not expose family '{0}'." -f
            $familyName)
    }
}

$requiredComponentUrls =
    @(
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/cma-es-components.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-ant-colony-optimization.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/memetic-algorithm-components.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/pso-communication-topologies.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/simulated-annealing-cooling-schedules.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/tabu-search-memory-control-strategies.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-variable-neighborhood-search-variants.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/path-relinking-strategies.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/threshold-accepting-schedules.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/acceptance-based-trajectory-methods.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-iterated-greedy-strategies.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-scatter-search-strategies.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-genetic-algorithm-operators.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/large-neighborhood-search-components.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/adaptive-large-neighborhood-search-components.html",
        "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-adaptive-large-neighborhood-search-components.html"
    )

if (@($requiredComponentUrls).Length -ne 16) {
    throw "Professional README parity: internal component URL contract must contain exactly 16 entries."
}

$componentBegin =
    $normalized.IndexOf(
        "<!-- SCIENTIFIC-COMPONENT-CATALOG-BEGIN -->",
        [System.StringComparison]::Ordinal)

$componentEnd =
    $normalized.IndexOf(
        "<!-- SCIENTIFIC-COMPONENT-CATALOG-END -->",
        [System.StringComparison]::Ordinal)

if ($componentBegin -lt 0 -or
    $componentEnd -le $componentBegin) {

    throw "Professional README parity: scientific component-catalog markers are missing or out of order."
}

$componentRegion =
    $normalized.Substring(
        $componentBegin,
        ($componentEnd +
            "<!-- SCIENTIFIC-COMPONENT-CATALOG-END -->".Length) -
        $componentBegin)

$componentCells =
    @(
        [regex]::Matches(
            $componentRegion,
            '<td width="50%" valign="top" data-component-url="')
    )

if (@($componentCells).Length -ne 16) {
    throw (
        "Professional README parity: expected 16 uniform scientific-component cells; found {0}." -f
        @($componentCells).Length)
}

$componentRows =
    @(
        [regex]::Matches(
            $componentRegion,
            '(?m)^<tr>$')
    )

if (@($componentRows).Length -ne 8) {
    throw (
        "Professional README parity: expected 8 two-column scientific-component rows; found {0}." -f
        @($componentRows).Length)
}

foreach ($componentUrl in $requiredComponentUrls) {
    $urlMatches =
        @(
            [regex]::Matches(
                $componentRegion,
                [regex]::Escape(
                    $componentUrl))
        )

    if (@($urlMatches).Length -ne 2) {
        throw (
            "Professional README parity: component URL '{0}' must appear exactly twice in its component card (data attribute + href); found {1}." -f
            $componentUrl,
            @($urlMatches).Length)
    }
}

$historicalReadmeRequirements =
    @(
        Get-HistoricalReadmeRequirements `
            -RepositoryRoot $Root
    )

if (@($historicalReadmeRequirements).Length -eq 0) {
    throw "Professional README parity: no historical README requirements were extracted by AST."
}

$knownThresholdRequirements =
    @(
        "threshold-accepting-dueck-scheuer-1990",
        "components/threshold-accepting-schedules.html"
    )

foreach ($knownThresholdRequirement in $knownThresholdRequirements) {
    $knownThresholdMatches =
        @(
            $historicalReadmeRequirements |
            Where-Object {
                [string]::Equals(
                    [string]$_,
                    $knownThresholdRequirement,
                    [System.StringComparison]::Ordinal)
            }
        )

    if (@($knownThresholdMatches).Length -ne 1) {
        throw (
            "Professional README parity: CommandAst extractor did not recover the known Threshold Accepting README contract '{0}'." -f
            $knownThresholdRequirement)
    }
}

$missingHistoricalRequirements = @()

foreach ($requirement in $historicalReadmeRequirements) {
    if ($normalized.IndexOf(
            [string]$requirement,
            [System.StringComparison]::Ordinal) -lt 0) {

        $missingHistoricalRequirements +=
            [string]$requirement
    }
}

if (@($missingHistoricalRequirements).Length -ne 0) {
    throw (
        "Professional README parity: {0} historical README validator requirement(s) are missing: {1}" -f
        @($missingHistoricalRequirements).Length,
        (@($missingHistoricalRequirements) -join " | "))
}

if ($normalized.IndexOf(
        "## v1.0 stability contract",
        [System.StringComparison]::Ordinal) -lt 0) {

    throw "Professional README parity: direct v1 freeze README stability contract is missing."
}

Write-Host (
    "HISTORICAL README CONTRACT GREEN: {0} unique README requirement token(s) extracted from CommandAst/HashtableAst in docs/Test-*.ps1; all are preserved, including both Threshold Accepting sentinels and 16 scientific component catalogs." -f
    @($historicalReadmeRequirements).Length) -ForegroundColor Green

$historicalQualityPath =
    Join-Path `
        $Root `
        "docs\Test-ReadmeQuality.ps1"

& $historicalQualityPath `
    -Root $Root

Write-Host (
    "HISTORICAL README QUALITY CONTRACT GREEN: exact Test-ReadmeQuality.ps1 passed before Build-Validated.") -ForegroundColor Green

$historicalValidatorCandidates =
    @(
        Get-ChildItem `
            -LiteralPath (
                Join-Path `
                    $Root `
                    "docs"
            ) `
            -Filter "Test-*.ps1" `
            -File |
        Sort-Object FullName
    )

$historicalReadmeValidators = @()

foreach ($historicalValidator in $historicalValidatorCandidates) {
    if ([string]::Equals(
            [string]$historicalValidator.Name,
            "Test-ProfessionalReadme.ps1",
            [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals(
            [string]$historicalValidator.Name,
            "Test-ReadmeQuality.ps1",
            [System.StringComparison]::OrdinalIgnoreCase)) {

        continue
    }

    $validatorTokens = $null
    $validatorParseErrors = $null

    $validatorAst =
        [System.Management.Automation.Language.Parser]::ParseFile(
            [string]$historicalValidator.FullName,
            [ref]$validatorTokens,
            [ref]$validatorParseErrors)

    if (@($validatorParseErrors).Length -ne 0) {
        throw (
            "Professional README historical-validator preflight: '{0}' does not parse." -f
            [string]$historicalValidator.Name)
    }

    $readmeLiteralNodes =
        @(
            $validatorAst.FindAll(
                {
                    param($node)

                    if ($node -is
                        [System.Management.Automation.Language.StringConstantExpressionAst]) {

                        return [string]::Equals(
                            [string]$node.Value,
                            "README.md",
                            [System.StringComparison]::OrdinalIgnoreCase)
                    }

                    if ($node -is
                        [System.Management.Automation.Language.ExpandableStringExpressionAst]) {

                        return [string]::Equals(
                            [string]$node.Value,
                            "README.md",
                            [System.StringComparison]::OrdinalIgnoreCase)
                    }

                    return $false
                },
                $true)
        )

    if (@($readmeLiteralNodes).Length -eq 0) {
        continue
    }

    $parameterNames =
        @(
            $validatorAst.ParamBlock.Parameters |
            ForEach-Object {
                [string]$_.Name.VariablePath.UserPath
            }
        )

    $rootParameters =
        @(
            $parameterNames |
            Where-Object {
                [string]::Equals(
                    [string]$_,
                    "Root",
                    [System.StringComparison]::OrdinalIgnoreCase)
            }
        )

    if (@($rootParameters).Length -ne 1) {
        throw (
            "Professional README historical-validator preflight: README-owning validator '{0}' must expose exactly one Root parameter; found parameters: {1}" -f
            [string]$historicalValidator.Name,
            (@($parameterNames) -join " | "))
    }

    $historicalReadmeValidators +=
        $historicalValidator
}

if (@($historicalReadmeValidators).Length -eq 0) {
    throw "Professional README historical-validator preflight: no README-owning historical validators were discovered."
}

foreach ($historicalValidator in $historicalReadmeValidators) {
    & $historicalValidator.FullName `
        -Root $Root

    Write-Host (
        "HISTORICAL README VALIDATOR GREEN: {0}" -f
        [string]$historicalValidator.Name) -ForegroundColor Green
}

Write-Host (
    "ALL HISTORICAL README VALIDATORS GREEN: {0} README-owning Test-*.ps1 validator(s) executed successfully before Build-Validated." -f
    @($historicalReadmeValidators).Length) -ForegroundColor Green

Write-Host (
    "PROFESSIONAL README PARITY GREEN: 155 catalog cards, 8 visible scientific families, {0} uniform two-column algorithm rows, 8 uniform two-column component rows, zero colspan/full-width cards, complete multi-/many-objective, constrained, multimodal, hyper-heuristic, matheuristic, historical-validator, historical-quality and keyword coverage." -f
    $expectedRows) -ForegroundColor Green
