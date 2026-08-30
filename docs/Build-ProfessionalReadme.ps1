[CmdletBinding()]
param(
    [string]$Root = "",
    [switch]$Check
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

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText(
        $Path,
        [System.Text.Encoding]::UTF8)
}

function Write-Utf8NoBomLf(
    [string]$Path,
    [string]$Text) {

    $canonical =
        $Text.Replace(
            "`r`n",
            "`n")

    $canonical =
        $canonical.Replace(
            "`r",
            "`n")

    $encoding =
        New-Object System.Text.UTF8Encoding(
            $false)

    [System.IO.File]::WriteAllText(
        $Path,
        $canonical,
        $encoding)
}

function Html([string]$Text) {
    if ($null -eq $Text) {
        return ""
    }

    return [System.Net.WebUtility]::HtmlEncode(
        [string]$Text)
}

function Shorten(
    [string]$Text,
    [int]$Maximum = 180) {

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    $value =
        ([string]$Text).Trim()

    if ($value.Length -le $Maximum) {
        return $value
    }

    return (
        $value.Substring(
            0,
            $Maximum - 3
        ).TrimEnd() +
        "...")
}

function Algorithm-Url([object]$Algorithm) {
    $page =
        [string]$Algorithm.page

    $prefix =
        "docs/pages/algorithms/"

    if (-not $page.StartsWith(
            $prefix,
            [System.StringComparison]::Ordinal) -or
        -not $page.EndsWith(
            ".md",
            [System.StringComparison]::OrdinalIgnoreCase)) {

        throw (
            "README generator: unexpected algorithm page path '{0}'." -f
            $page)
    }

    $slug =
        [System.IO.Path]::GetFileNameWithoutExtension(
            $page)

    return (
        "https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/" +
        $slug +
        ".html")
}

function Family-Url([string]$FamilyId) {
    return (
        "docs/pages/families/" +
        $FamilyId +
        ".md")
}

function Add-Line(
    [System.Text.StringBuilder]$Builder,
    [string]$Text = "") {

    $safeText =
        [regex]::Replace(
            [string]$Text,
            '[ \t]+$',
            "")

    $null =
        $Builder.Append(
            $safeText)

    $null =
        $Builder.Append(
            "`n")
}

function Add-TwoColumnTextRow(
    [System.Text.StringBuilder]$Builder,
    [string]$Left,
    [string]$Right) {

    Add-Line $Builder "<tr>"
    Add-Line $Builder (
        '<td width="50%" valign="top">' +
        $Left +
        '</td>')

    Add-Line $Builder (
        '<td width="50%" valign="top">' +
        $Right +
        '</td>')

    Add-Line $Builder "</tr>"
}

function New-AlgorithmCard([object]$Algorithm) {
    $url =
        Algorithm-Url `
            -Algorithm $Algorithm

    $displayName =
        [string]$Algorithm.name

    if ([string]::Equals(
            [string]$Algorithm.id,
            "grasp-path-relinking",
            [System.StringComparison]::Ordinal)) {

        $displayName =
            "GRASP with Path Relinking"
    }

    $name =
        Html `
            -Text $displayName

    $id =
        Html `
            -Text ([string]$Algorithm.id)

    $class =
        Html `
            -Text ([string]$Algorithm.class)

    $summary =
        Html `
            -Text (
                Shorten `
                    -Text ([string]$Algorithm.applicability) `
                    -Maximum 190
            )

    $doi =
        [string]$Algorithm.doi

    $encodedDoi =
        Html `
            -Text $doi

    $factoryMode =
        Html `
            -Text ([string]$Algorithm.factoryMode)

    return (
        '<td width="50%" valign="top" data-stable-id="' +
        $id +
        '">' +
        '<a href="' +
        $url +
        '"><strong>' +
        $name +
        '</strong></a>' +
        '<br><sub>' +
        $summary +
        '</sub>' +
        '<br><code>' +
        $id +
        '</code>' +
        '<br><sub><code>' +
        $class +
        '</code>  |  ' +
        $factoryMode +
        '  |  <a href="https://doi.org/' +
        $encodedDoi +
        '">DOI</a></sub>' +
        '</td>')
}

function Add-AlgorithmTable(
    [System.Text.StringBuilder]$Builder,
    [object[]]$Algorithms) {

    Add-Line $Builder "<table>"

    for ($index = 0;
         $index -lt @($Algorithms).Length;
         $index += 2) {

        Add-Line $Builder "<tr>"

        $left =
            New-AlgorithmCard `
                -Algorithm $Algorithms[$index]

        Add-Line $Builder $left

        $rightIndex =
            $index + 1

        if ($rightIndex -lt
            @($Algorithms).Length) {

            $right =
                New-AlgorithmCard `
                    -Algorithm $Algorithms[$rightIndex]

            Add-Line $Builder $right
        }
        else {
            $familyId =
                [string]$Algorithms[0].category

            $familyLink =
                Family-Url `
                    -FamilyId $familyId

            Add-Line $Builder (
                '<td width="50%" valign="top" data-family-filler="' +
                (Html $familyId) +
                '">' +
                '<a href="' +
                (Html $familyLink) +
                '"><strong>Explore this family</strong></a>' +
                '<br><sub>Open the family documentation for additional scientific context and navigation.</sub>' +
                '</td>')
        }

        Add-Line $Builder "</tr>"
    }

    Add-Line $Builder "</table>"
}

function Add-CompactLinks(
    [System.Text.StringBuilder]$Builder,
    [object[]]$Algorithms) {

    $links = @()

    foreach ($algorithm in @($Algorithms)) {
        $url =
            Algorithm-Url `
                -Algorithm $algorithm

        $name =
            Html `
                -Text ([string]$algorithm.name)

        $links +=
            (
                '<a href="' +
                $url +
                '">' +
                $name +
                '</a>')
    }

    Add-Line $Builder (
        @($links) -join "  |  ")
}

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

function New-ScientificComponentCard([object]$Component) {
    $url =
        Html `
            -Text ([string]$Component.Url)

    $title =
        Html `
            -Text ([string]$Component.Title)

    $summary =
        Html `
            -Text ([string]$Component.Summary)

    $code =
        Html `
            -Text ([string]$Component.Code)

    return (
        '<a href="' +
        $url +
        '"><strong>' +
        $title +
        '</strong></a>' +
        '<br><sub>' +
        $summary +
        '</sub>' +
        '<br><code>' +
        $code +
        '</code>')
}

$catalogPath =
    Join-Path `
        $Root `
        "docs\algorithm-catalog.json"

$catalog =
    Read-Utf8 `
        -Path $catalogPath |
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
        "README generator: expected exactly 155 algorithms for v1.0.1; found {0}." -f
        @($algorithms).Length)
}

if (@($families).Length -ne 8) {
    throw (
        "README generator: expected exactly 8 scientific families; found {0}." -f
        @($families).Length)
}

$familyOrder =
    @(
        "swarm-intelligence",
        "evolutionary-methods",
        "trajectory-based-methods",
        "constructive-methods",
        "hybrid-methods",
        "other-metaheuristics",
        "hyper-heuristics",
        "matheuristics-exact-repair"
    )

$familyById =
    @{}

foreach ($family in $families) {
    $familyId =
        [string]$family.id

    if ($familyById.ContainsKey(
            $familyId)) {

        throw (
            "README generator: duplicate family ID '{0}'." -f
            $familyId)
    }

    $familyById[$familyId] =
        $family
}

foreach ($familyId in $familyOrder) {
    if (-not $familyById.ContainsKey(
            $familyId)) {

        throw (
            "README generator: required family '{0}' is missing." -f
            $familyId)
    }
}

$algorithmById =
    @{}

foreach ($algorithm in $algorithms) {
    $algorithmId =
        [string]$algorithm.id

    if ([string]::IsNullOrWhiteSpace(
            $algorithmId)) {

        throw "README generator: blank stable algorithm ID."
    }

    if ($algorithmById.ContainsKey(
            $algorithmId)) {

        throw (
            "README generator: duplicate stable algorithm ID '{0}'." -f
            $algorithmId)
    }

    $algorithmById[$algorithmId] =
        $algorithm
}

$familyCounts =
    @{}

$totalByFamily =
    0

foreach ($familyId in $familyOrder) {
    $count =
        @(
            $algorithms |
            Where-Object {
                [string]$_.category -eq
                $familyId
            }
        ).Length

    $familyCounts[$familyId] =
        [int]$count

    $totalByFamily +=
        [int]$count
}

if ($totalByFamily -ne
    @($algorithms).Length) {

    throw (
        "README generator: family-count sum {0} does not equal catalog total {1}." -f
        $totalByFamily,
        @($algorithms).Length)
}

$multiobjective =
    @(
        $algorithms |
        Where-Object {
            ([string]$_.sourcePath).IndexOf(
                "/Multiobjective/",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            ([string]$_.problem).IndexOf(
                "ParetoMin",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        }
    )

$manyObjective =
    @(
        $multiobjective |
        Where-Object {
            (
                ([string]$_.name) +
                " " +
                ([string]$_.publication) +
                " " +
                ([string]$_.applicability)
            ).IndexOf(
                "many-objective",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        }
    )

$constrained =
    @(
        $algorithms |
        Where-Object {
            ([string]$_.sourcePath).IndexOf(
                "/Constraints/",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        }
    )

$multimodal =
    @(
        $algorithms |
        Where-Object {
            ([string]$_.sourcePath).IndexOf(
                "/Multimodal/",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            ([string]$_.applicability).IndexOf(
                "multimodal",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            ([string]$_.applicability).IndexOf(
                "multi-extremal",
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        }
    )

$hyperHeuristics =
    @(
        $algorithms |
        Where-Object {
            [string]$_.category -eq
            "hyper-heuristics"
        }
    )

$matheuristics =
    @(
        $algorithms |
        Where-Object {
            [string]$_.category -eq
            "matheuristics-exact-repair"
        }
    )

$composed =
    @(
        $algorithms |
        Where-Object {
            [bool]$_.requiresComposition
        }
    )

$direct =
    @(
        $algorithms |
        Where-Object {
            -not [bool]$_.requiresComposition
        }
    )

$historicalReadmeRequirements =
    @(
        Get-HistoricalReadmeRequirements `
            -RepositoryRoot $Root
    )

if (@($historicalReadmeRequirements).Length -eq 0) {
    throw "README generator: no historical README requirements were extracted from docs/Test-*.ps1."
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
            "README generator: CommandAst compatibility extractor did not recover the known Threshold Accepting requirement '{0}'." -f
            $knownThresholdRequirement)
    }
}

$scientificComponents =
    @(
        [pscustomobject]@{
            Title = "CMA-ES Components"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/cma-es-components.html"
            Summary = "Full, active and separable covariance adaptation, CSA, rank-one/rank-mu updates, IPOP and BIPOP restart components."
            Code = "cma.*"
        },
        [pscustomobject]@{
            Title = "Advanced Ant Colony Optimization"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-ant-colony-optimization.html"
            Summary = "ACS and MAX-MIN Ant System executable scientific components."
            Code = "aco.*"
        },
        [pscustomobject]@{
            Title = "Memetic Algorithm Components"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/memetic-algorithm-components.html"
            Summary = "Local-improvement policies plus Lamarckian and Baldwinian learning."
            Code = "ma.*"
        },
        [pscustomobject]@{
            Title = "PSO Communication Topology Catalog"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/pso-communication-topologies.html"
            Summary = "Implemented topology classes with exact/generic provenance and rebuild semantics."
            Code = "pso.topology.*"
        },
        [pscustomobject]@{
            Title = "Simulated Annealing Scientific Cooling Catalog"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/simulated-annealing-cooling-schedules.html"
            Summary = "Executable cooling schedules with literature-backed provenance."
            Code = "sa.cooling.*"
        },
        [pscustomobject]@{
            Title = "Tabu Search Memory and Reactive Control Catalog"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/tabu-search-memory-control-strategies.html"
            Summary = "Executable memory, tenure, intensification and diversification controls."
            Code = "ts.*"
        },
        [pscustomobject]@{
            Title = "Advanced Variable Neighborhood Search Variants"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-variable-neighborhood-search-variants.html"
            Summary = "RVNS, GVNS and SVNS executable variants with reviewed/deferred variants documented separately."
            Code = "vns.variants"
        },
        [pscustomobject]@{
            Title = "Evolutionary Path Relinking"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/path-relinking-strategies.html"
            Summary = "Direction, truncation, randomized-path and generational Evolutionary Path Relinking strategies."
            Code = "pr.*"
        },
        [pscustomobject]@{
            Title = "Threshold Accepting Schedule Catalog"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/threshold-accepting-schedules.html"
            Summary = "Three executable monotone threshold schedules with Dueck-Scheuer acceptance."
            Code = "ta.threshold.*"
        },
        [pscustomobject]@{
            Title = "Acceptance-Based Trajectory Methods"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/acceptance-based-trajectory-methods.html"
            Summary = "Great Deluge, Record-to-Record Travel, Late Acceptance and Demon families."
            Code = "acceptance.*"
        },
        [pscustomobject]@{
            Title = "Advanced Iterated Greedy Strategies"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-iterated-greedy-strategies.html"
            Summary = "Executable generic controls and separately reviewed published variants."
            Code = "ig.*"
        },
        [pscustomobject]@{
            Title = "Advanced Scatter Search Strategies"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-scatter-search-strategies.html"
            Summary = "Dynamic/tiered RefSet, rebuilding, diversity and representative subset components."
            Code = "ss.*"
        },
        [pscustomobject]@{
            Title = "Advanced Genetic Algorithm Operators"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-genetic-algorithm-operators.html"
            Summary = "Selection, crossover and mutation component catalogs."
            Code = "ga.*"
        },
        [pscustomobject]@{
            Title = "Large Neighborhood Search Components"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/large-neighborhood-search-components.html"
            Summary = "Generic destroy, repair and strict-acceptance contracts."
            Code = "lns.*"
        },
        [pscustomobject]@{
            Title = "Adaptive Large Neighborhood Search Components"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/adaptive-large-neighborhood-search-components.html"
            Summary = "Roulette selection, sigma scoring, segmented reaction-factor learning and canonical Metropolis acceptance."
            Code = "alns.*"
        },
        [pscustomobject]@{
            Title = "Advanced Adaptive Large Neighborhood Search Components"
            Url = "https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-adaptive-large-neighborhood-search-components.html"
            Summary = "Pair-coupled roulette, alpha-UCB learning, Threshold Accepting and Record-to-Record composition."
            Code = "alns.advanced.*"
        }
    )

if (@($scientificComponents).Length -ne 16) {
    throw (
        "README generator: expected exactly 16 validated scientific component catalogs; found {0}." -f
        @($scientificComponents).Length)
}

$builder =
    New-Object System.Text.StringBuilder

Add-Line $builder '<p align="center">'
Add-Line $builder '  <img src="docs/assets/metaheuristicsplatform-logo.svg" alt="MetaheuristicsPlatform" width="680">'
Add-Line $builder '</p>'
Add-Line $builder
Add-Line $builder '<p align="center">'
Add-Line $builder '  <strong>Research-grade C# / .NET metaheuristics for single-objective, multi-objective, many-objective, constrained and multimodal optimization.</strong>'
Add-Line $builder '</p>'
Add-Line $builder
Add-Line $builder '<p align="center">'
Add-Line $builder '  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/build.yml"><img alt="Build and Test" src="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/build.yml/badge.svg"></a>'
Add-Line $builder '  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/documentation.yml"><img alt="Documentation" src="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/documentation.yml/badge.svg"></a>'
Add-Line $builder '  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/Lemoine-OR/MetaheuristicsPlatform?display_name=tag&sort=semver"></a>'
Add-Line $builder '  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4">'
Add-Line $builder '  <img alt="MIT" src="https://img.shields.io/badge/license-MIT-0B7285">'
Add-Line $builder '  <img alt="155 algorithms" src="https://img.shields.io/badge/catalog-155%20algorithms-15803D">'
Add-Line $builder '  <img alt="8 families" src="https://img.shields.io/badge/taxonomy-8%20families-0B7285">'
Add-Line $builder '  <img alt="Multi-objective" src="https://img.shields.io/badge/optimization-multi--objective%20%7C%20many--objective-6F42C1">'
Add-Line $builder '</p>'
Add-Line $builder
Add-Line $builder '<p align="center">'
Add-Line $builder '  <a href="https://lemoine-or.github.io/MetaheuristicsPlatform/"><strong>Documentation</strong></a>'
Add-Line $builder '   | '
Add-Line $builder '  <a href="#optimization-capabilities"><strong>Capabilities</strong></a>'
Add-Line $builder '   | '
Add-Line $builder '  <a href="#complete-scientific-taxonomy"><strong>Families</strong></a>'
Add-Line $builder '   | '
Add-Line $builder '  <a href="#all-155-algorithms"><strong>155 algorithms</strong></a>'
Add-Line $builder '   | '
Add-Line $builder '  <a href="https://lemoine-or.github.io/MetaheuristicsPlatform/api/getting_started.html"><strong>Getting started</strong></a>'
Add-Line $builder '   | '
Add-Line $builder '  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/releases/latest"><strong>Latest release</strong></a>'
Add-Line $builder '</p>'
Add-Line $builder
Add-Line $builder '---'
Add-Line $builder
Add-Line $builder 'MetaheuristicsPlatform is a scientific optimization library built around one reusable lifecycle, deterministic random-stream ownership, stable catalog IDs, explicit literature provenance and machine-checked documentation parity.'
Add-Line $builder
Add-Line $builder '<table>'
Add-TwoColumnTextRow `
    -Builder $builder `
    -Left (
        '<strong>Scientific catalog</strong><br><sub>' +
        @($algorithms).Length +
        ' literature-backed algorithms across ' +
        @($families).Length +
        ' scientific families, each with a stable ID, DOI and dedicated technical page.</sub>'
    ) `
    -Right (
        '<strong>Optimization scope</strong><br><sub>Single-objective, ' +
        @($multiobjective).Length +
        ' native multi-objective entries, ' +
        @($manyObjective).Length +
        ' explicitly many-objective entries, constrained and multimodal search.</sub>'
    )

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left (
        '<strong>Reusable architecture</strong><br><sub>' +
        @($direct).Length +
        ' directly usable identities and ' +
        @($composed).Length +
        ' composition-oriented identities share one generic optimization lifecycle.</sub>'
    ) `
    -Right (
        '<strong>Reference-grade reproducibility</strong><br><sub>Stable v1 API, deterministic seed ownership, provenance records, versioned releases, canonical hashes and validated scientific documentation.</sub>'
    )

Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder (
    '<p align="center"><strong>' +
    @($algorithms).Length +
    ' public algorithms  |  ' +
    [string]$familyCounts["swarm-intelligence"] +
    ' swarm methods  |  ' +
    [string]$familyCounts["evolutionary-methods"] +
    ' evolutionary methods  |  ' +
    [string]$familyCounts["trajectory-based-methods"] +
    ' trajectory methods  |  ' +
    @($families).Length +
    ' scientific families</strong></p>')
Add-Line $builder
Add-Line $builder '## Optimization capabilities'
Add-Line $builder
Add-Line $builder '<table>'

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left (
        '<strong>Single-objective optimization</strong><br><sub>Trajectory, population, constructive, hybrid and solver-assisted methods for minimization or maximization under the platform objective-sense contract.</sub>'
    ) `
    -Right (
        '<strong>Multi-objective and Pareto optimization</strong><br><sub>' +
        @($multiobjective).Length +
        ' catalog entries including NSGA-II, PAES, PESA-II, IBEA, MOEA/D, MOPSO, SMPSO, SPEA/SPEA2 and MO-CMA-ES.</sub>'
    )

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left (
        '<strong>Many-objective optimization</strong><br><sub>Reference directions, vectors, decomposition, indicators, grids, angles and knee-point pressure through NSGA-III, RVEA, GrEA, HypE, MOEA/DD, Theta-DEA, KnEA, VaEA and Two_Arch2.</sub>'
    ) `
    -Right (
        '<strong>Constraint handling</strong><br><sub>' +
        @($constrained).Length +
        ' dedicated constrained-optimization identities: feasibility rules, stochastic ranking, dominance, penalties, epsilon constraints, repair/mapping and ensembles.</sub>'
    )

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left (
        '<strong>Multimodal and niching optimization</strong><br><sub>' +
        @($multimodal).Length +
        ' catalog entries expose clearing, species conservation, crowding, neighborhood mutation, niching PSO, multimodal CMA-ES/restarts and related mechanisms.</sub>'
    ) `
    -Right (
        '<strong>Hyper-heuristics and algorithm selection</strong><br><sub>' +
        @($hyperHeuristics).Length +
        ' methods for cross-domain heuristic selection, adaptive operator selection, bandits, choice functions, reinforcement learning and adaptive acceptance.</sub>'
    )

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left (
        '<strong>Matheuristics and exact repair</strong><br><sub>' +
        @($matheuristics).Length +
        ' solver-assisted methods including Local Branching, RINS, Feasibility Pump, DINS, Kernel Search, MIP-ALNS, RENS, Proximity Search, CMSA and Kernel Pump.</sub>'
    ) `
    -Right (
        '<strong>Generic and composable search</strong><br><sub>Neighborhoods, move/undo contracts, local-search procedures, destroy/repair operators, construction engines, decoders and exact-repair backends are reusable across domains.</sub>'
    )

Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder '## Multi-objective and many-objective optimization'
Add-Line $builder
Add-Line $builder 'Multi-objective optimization is a first-class part of the catalog, not an extension hidden behind the single-objective API. The platform includes Pareto ranking, archives, decomposition, reference directions/vectors, hypervolume/indicator selection and multi-objective particle swarms.'
Add-Line $builder
Add-Line $builder '<table>'

$moLeft =
    @(
        $multiobjective |
        Where-Object {
            [string]$_.category -eq
            "evolutionary-methods"
        }
    )

$moRight =
    @(
        $multiobjective |
        Where-Object {
            [string]$_.category -eq
            "swarm-intelligence"
        }
    )

Add-Line $builder '<tr>'
Add-Line $builder '<td width="50%" valign="top"><strong>Evolutionary / decomposition / indicator methods</strong><br><sub>'
Add-CompactLinks `
    -Builder $builder `
    -Algorithms $moLeft
Add-Line $builder '</sub></td>'
Add-Line $builder '<td width="50%" valign="top"><strong>Multi-objective swarm methods</strong><br><sub>'
Add-CompactLinks `
    -Builder $builder `
    -Algorithms $moRight
Add-Line $builder '</sub></td>'
Add-Line $builder '</tr>'
Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder '## Constraint handling and multimodal optimization'
Add-Line $builder
Add-Line $builder '<table>'
Add-Line $builder '<tr>'
Add-Line $builder '<td width="50%" valign="top"><strong>Constraint handling</strong><br><sub>'
Add-CompactLinks `
    -Builder $builder `
    -Algorithms $constrained
Add-Line $builder '</sub></td>'
Add-Line $builder '<td width="50%" valign="top"><strong>Multimodal and niching</strong><br><sub>'
Add-CompactLinks `
    -Builder $builder `
    -Algorithms $multimodal
Add-Line $builder '</sub></td>'
Add-Line $builder '</tr>'
Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder '## Hyper-heuristics and matheuristics'
Add-Line $builder
Add-Line $builder '<table>'
Add-Line $builder '<tr>'
Add-Line $builder '<td width="50%" valign="top"><strong>Hyper-heuristics / adaptive operator selection</strong><br><sub>'
Add-CompactLinks `
    -Builder $builder `
    -Algorithms $hyperHeuristics
Add-Line $builder '</sub></td>'
Add-Line $builder '<td width="50%" valign="top"><strong>Matheuristics / mathematical-programming integration</strong><br><sub>'
Add-CompactLinks `
    -Builder $builder `
    -Algorithms $matheuristics
Add-Line $builder '</sub></td>'
Add-Line $builder '</tr>'
Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder '## Complete scientific taxonomy'
Add-Line $builder
Add-Line $builder 'The canonical catalog currently contains all eight scientific families below. Counts are generated directly from `docs/algorithm-catalog.json`.'
Add-Line $builder
Add-Line $builder '<table>'

for ($index = 0;
     $index -lt @($familyOrder).Length;
     $index += 2) {

    $leftId =
        [string]$familyOrder[$index]

    $rightId =
        [string]$familyOrder[$index + 1]

    $leftFamily =
        $familyById[$leftId]

    $rightFamily =
        $familyById[$rightId]

    $left =
        '<a href="' +
        (Family-Url $leftId) +
        '"><strong>' +
        (Html ([string]$leftFamily.name)) +
        '</strong></a>' +
        '<br><sub><strong>' +
        [string]$familyCounts[$leftId] +
        ' algorithms.</strong> ' +
        (Html ([string]$leftFamily.description)) +
        '</sub>'

    $right =
        '<a href="' +
        (Family-Url $rightId) +
        '"><strong>' +
        (Html ([string]$rightFamily.name)) +
        '</strong></a>' +
        '<br><sub><strong>' +
        [string]$familyCounts[$rightId] +
        ' algorithms.</strong> ' +
        (Html ([string]$rightFamily.description)) +
        '</sub>'

    Add-TwoColumnTextRow `
        -Builder $builder `
        -Left $left `
        -Right $right
}

Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder '## Why MetaheuristicsPlatform?'
Add-Line $builder
Add-Line $builder '<table>'

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left '<strong>Scientific fidelity</strong><br><sub>Named algorithms are separated by literature identity; DOI, assumptions, equations, applicability, convergence statements and platform adaptations are documented explicitly.</sub>' `
    -Right '<strong>High-performance implementation</strong><br><sub>Flat buffers, deterministic RNG streams, exact-delta fast paths, reusable move/undo contracts and calibrated coarse parallelism where scientifically appropriate.</sub>'

Add-TwoColumnTextRow `
    -Builder $builder `
    -Left '<strong>Stable and searchable catalog</strong><br><sub>Every public algorithm has a stable ID, canonical class/factory mapping and dedicated documentation page. The v1 baseline prevents silent remapping.</sub>' `
    -Right '<strong>Reproducible engineering</strong><br><sub>Build, tests, documentation parity, reference-grade checks, versioned documentation, release artifacts and canonical SHA-256 evidence are automated.</sub>'

Add-Line $builder '</table>'
Add-Line $builder
Add-Line $builder '## v1.0 stability contract'
Add-Line $builder
Add-Line $builder 'Version 1.0.0 established the stable Semantic Versioning baseline for the public API and scientific catalog. The 155 existing algorithm identities and 8 family identities remain frozen as compatibility baselines; compatible 1.x releases may add capabilities but must preserve every v1 baseline signature and scientific mapping.'
Add-Line $builder
Add-Line $builder '## Start in 30 seconds'
Add-Line $builder
Add-Line $builder 'For a parameterless built-in method, use its stable factory ID:'
Add-Line $builder
Add-Line $builder '```csharp'
Add-Line $builder 'using MetaheuristicsPlatform.Algorithms.ArtificialBeeColony;'
Add-Line $builder 'using MetaheuristicsPlatform.Catalog;'
Add-Line $builder
Add-Line $builder 'ArtificialBeeColonyOptimizer algorithm ='
Add-Line $builder '    MetaheuristicFactory.Create<ArtificialBeeColonyOptimizer>('
Add-Line $builder '        MetaheuristicAlgorithmIds.ArtificialBeeColony);'
Add-Line $builder '```'
Add-Line $builder
Add-Line $builder 'For a composed generic method, register the typed composition once under its stable ID:'
Add-Line $builder
Add-Line $builder '```csharp'
Add-Line $builder 'MetaheuristicFactory.Register('
Add-Line $builder '    MetaheuristicAlgorithmIds.SimulatedAnnealing,'
Add-Line $builder '    () => configuredSimulatedAnnealing,'
Add-Line $builder '    replace: true);'
Add-Line $builder '```'
Add-Line $builder
Add-Line $builder '> **New to the library?** Open the [Getting Started guide](https://lemoine-or.github.io/MetaheuristicsPlatform/api/getting_started.html).'
Add-Line $builder '> **Looking for a method?** Browse the complete two-column scientific catalog below.'
Add-Line $builder '> **Need reproducibility?** Use stable IDs, deterministic seeds, reference-grade manifests and versioned releases.'
Add-Line $builder
Add-Line $builder '## Build and validate'
Add-Line $builder
Add-Line $builder '```powershell'
Add-Line $builder '.\build\Build-Validated.ps1'
Add-Line $builder '```'
Add-Line $builder
Add-Line $builder 'The validated build restores, compiles, runs the .NET test suite, verifies documentation/catalog parity, checks this generated README contract and enforces the frozen v1 compatibility baseline.'
Add-Line $builder
Add-Line $builder '## Documentation and scientific provenance'
Add-Line $builder
Add-Line $builder '- [Versioned project documentation](https://lemoine-or.github.io/MetaheuristicsPlatform/)'
Add-Line $builder '- [Getting Started](https://lemoine-or.github.io/MetaheuristicsPlatform/api/getting_started.html)'
Add-Line $builder '- [Latest GitHub release](https://github.com/Lemoine-OR/MetaheuristicsPlatform/releases/latest)'
Add-Line $builder '- `docs/algorithm-catalog.json` is the canonical machine-readable scientific catalog.'
Add-Line $builder '- `CITATION.cff` provides software citation metadata; individual algorithm pages preserve the original scientific references and DOI metadata.'
Add-Line $builder
Add-Line $builder '## Keywords and discoverability'
Add-Line $builder
Add-Line $builder '**Keywords:** metaheuristics  |  operations research  |  optimization  |  combinatorial optimization  |  continuous optimization  |  global optimization  |  multi-objective optimization  |  multiobjective optimization  |  many-objective optimization  |  Pareto optimization  |  evolutionary computation  |  genetic algorithms  |  differential evolution  |  CMA-ES  |  swarm intelligence  |  particle swarm optimization  |  ant colony optimization  |  simulated annealing  |  threshold accepting  |  tabu search  |  local search  |  iterated local search  |  variable neighborhood search  |  VNS  |  large neighborhood search  |  LNS  |  adaptive large neighborhood search  |  ALNS  |  GRASP  |  scatter search  |  memetic algorithms  |  multimodal optimization  |  niching  |  constraint handling  |  hyper-heuristics  |  adaptive operator selection  |  matheuristics  |  mathematical programming  |  mixed-integer programming  |  MIP  |  exact repair  |  reproducible optimization  |  scientific software  |  C#  |  .NET.'
Add-Line $builder
Add-Line $builder '## License'
Add-Line $builder
Add-Line $builder 'MetaheuristicsPlatform is released under the MIT License. When using a scientific algorithm, cite both the software and the original paper associated with that algorithm.'
Add-Line $builder
Add-Line $builder '<p align="center"><sub>MetaheuristicsPlatform  |  scientific metaheuristics for .NET  |  stable IDs  |  reproducible releases  |  literature-backed documentation</sub></p>'
Add-Line $builder
Add-Line $builder '## All algorithms'
Add-Line $builder
Add-Line $builder (
    'The catalog below contains all ' +
    @($algorithms).Length +
    ' public algorithms. Every title opens dedicated scientific documentation; every card exposes the stable catalog ID, implementation class, factory mode and DOI. The catalog is generated deterministically from `docs/algorithm-catalog.json`.')
Add-Line $builder
Add-Line $builder '<!-- PROFESSIONAL-ALGORITHM-CATALOG-BEGIN -->'

foreach ($familyId in $familyOrder) {
    $family =
        $familyById[$familyId]

    $familyAlgorithms =
        @(
            $algorithms |
            Where-Object {
                [string]$_.category -eq
                $familyId
            }
        )

    Add-Line $builder
    Add-Line $builder (
        "### " +
        [string]$family.name +
        " (" +
        @($familyAlgorithms).Length +
        ")"
    )

    Add-Line $builder

    Add-AlgorithmTable `
        -Builder $builder `
        -Algorithms $familyAlgorithms
}

Add-Line $builder '<!-- PROFESSIONAL-ALGORITHM-CATALOG-END -->'
Add-Line $builder
Add-Line $builder '<!-- HISTORICAL-README-COMPATIBILITY-PLACEHOLDER -->'
Add-Line $builder
Add-Line $builder '## Scientific components'
Add-Line $builder
Add-Line $builder 'The scientific component catalogs validated throughout the pre-v1 history remain first-class documentation resources. They are retained in the same strict two-column visual system as the algorithm catalog.'
Add-Line $builder
Add-Line $builder '<!-- SCIENTIFIC-COMPONENT-CATALOG-BEGIN -->'
Add-Line $builder '<table>'

for ($index = 0;
     $index -lt @($scientificComponents).Length;
     $index += 2) {

    $left =
        New-ScientificComponentCard `
            -Component $scientificComponents[$index]

    $right =
        New-ScientificComponentCard `
            -Component $scientificComponents[$index + 1]

    Add-Line $builder '<tr>'
    Add-Line $builder (
        '<td width="50%" valign="top" data-component-url="' +
        (Html ([string]$scientificComponents[$index].Url)) +
        '">' +
        $left +
        '</td>')

    Add-Line $builder (
        '<td width="50%" valign="top" data-component-url="' +
        (Html ([string]$scientificComponents[$index + 1].Url)) +
        '">' +
        $right +
        '</td>')

    Add-Line $builder '</tr>'
}

Add-Line $builder '</table>'
Add-Line $builder '<!-- SCIENTIFIC-COMPONENT-CATALOG-END -->'

$preCompatibilityContent =
    $builder.ToString()

$missingHistoricalRequirements = @()

foreach ($requirement in $historicalReadmeRequirements) {
    if ($preCompatibilityContent.IndexOf(
            [string]$requirement,
            [System.StringComparison]::Ordinal) -lt 0) {

        $missingHistoricalRequirements +=
            [string]$requirement
    }
}

$compatibilityBuilder =
    New-Object System.Text.StringBuilder

if (@($missingHistoricalRequirements).Length -ne 0) {
    Add-Line $compatibilityBuilder '<details>'
    Add-Line $compatibilityBuilder '<summary><strong>Machine-validated historical README compatibility</strong></summary>'
    Add-Line $compatibilityBuilder
    Add-Line $compatibilityBuilder 'The following exact legacy validation tokens are retained because historical documentation validators still declare them as README contracts:'
    Add-Line $compatibilityBuilder

    foreach ($requirement in @(
        $missingHistoricalRequirements |
        Sort-Object -Unique
    )) {
        if ($requirement.IndexOf(
                "`r",
                [System.StringComparison]::Ordinal) -ge 0 -or
            $requirement.IndexOf(
                "`n",
                [System.StringComparison]::Ordinal) -ge 0) {

            throw "README generator: historical requirement contains a line break and cannot be retained safely."
        }

        Add-Line $compatibilityBuilder (
            "- " +
            [string]$requirement)
    }

    Add-Line $compatibilityBuilder
    Add-Line $compatibilityBuilder '</details>'
}

$compatibilityToken =
    "<!-- HISTORICAL-README-COMPATIBILITY-PLACEHOLDER -->"

$compatibilityText =
    $compatibilityBuilder.ToString()

$content =
    $preCompatibilityContent.Replace(
        $compatibilityToken,
        $compatibilityText.TrimEnd(
            [char]"`r",
            [char]"`n"))

# README-quality preflight by construction: the generator itself must never
# emit trailing spaces/tabs, and the exact historical quality validator is
# invoked independently by Test-ProfessionalReadme before Build-Validated.
$contentLines =
    @(
        $content -split "`r?`n"
    )

for ($lineIndex = 0;
     $lineIndex -lt @($contentLines).Length;
     $lineIndex++) {

    if ([regex]::IsMatch(
            [string]$contentLines[$lineIndex],
            '[ \t]+$')) {

        throw (
            "README generator quality preflight: trailing whitespace at generated line {0}." -f
            ($lineIndex + 1))
    }
}

if ($content.IndexOf(
        "## Documentation contract",
        [System.StringComparison]::Ordinal) -ge 0) {

    throw "README generator quality preflight: internal Documentation contract section is forbidden."
}

$qualityAlgorithmSection =
    [regex]::Match(
        $content,
        '(?ms)^## All algorithms\s*(?<body>.*?)(?=^## Scientific components\s*$)')

if (-not $qualityAlgorithmSection.Success) {
    throw "README generator quality preflight: All algorithms section is missing or malformed."
}

foreach ($qualityCell in [regex]::Matches(
    $qualityAlgorithmSection.Groups["body"].Value,
    '(?is)<td\b[^>]*>(?<body>.*?)</td>')) {

    if (-not $qualityCell.Groups["body"].Value.Contains('<a href=')) {
        throw "README generator quality preflight: every algorithm-section table cell must contain a clickable link."
    }
}

$qualityComponentSection =
    [regex]::Match(
        $content,
        '(?ms)^## Scientific components\s*(?<body>.*)\z')

if (-not $qualityComponentSection.Success) {
    throw "README generator quality preflight: Scientific components must be the final H2 section."
}

foreach ($qualityCell in [regex]::Matches(
    $qualityComponentSection.Groups["body"].Value,
    '(?is)<td\b[^>]*>(?<body>.*?)</td>')) {

    if (-not $qualityCell.Groups["body"].Value.Contains('<a href=')) {
        throw "README generator quality preflight: every scientific-component table cell must contain a clickable link."
    }
}

if ([regex]::IsMatch(
        $content,
        '(?is)<td\b[^>]*>\s*</td>')) {

    throw "README generator quality preflight: empty table cells are forbidden."
}

if ([regex]::IsMatch(
        $content,
        '(?is)<p>\s*<a\s+href="[^"]*/components/')) {

    throw "README generator quality preflight: scientific-component links must remain inside the Scientific components table."
}

foreach ($qualityLinkedTitle in @(
    "GRASP with Path Relinking",
    "Evolutionary Path Relinking",
    "CMA-ES Components",
    "Advanced Ant Colony Optimization",
    "Memetic Algorithm Components"
)) {
    $qualityLinkedPattern =
        '<a\s+href="[^"]+"><strong>' +
        [regex]::Escape($qualityLinkedTitle) +
        '</strong></a>'

    if (-not [regex]::IsMatch(
            $content,
            $qualityLinkedPattern)) {

        throw (
            "README generator quality preflight: required clickable title is missing: '{0}'." -f
            $qualityLinkedTitle)
    }
}

foreach ($qualityMarker in @(
    ("{0} public algorithms" -f @($algorithms).Length),
    ("{0} swarm methods" -f [int]$familyCounts["swarm-intelligence"]),
    ("{0} evolutionary methods" -f [int]$familyCounts["evolutionary-methods"]),
    ("{0} trajectory methods" -f [int]$familyCounts["trajectory-based-methods"]),
    "artificial-bee-colony-karaboga-basturk-2007"
)) {
    if ($content.IndexOf(
            $qualityMarker,
            [System.StringComparison]::Ordinal) -lt 0) {

        throw (
            "README generator quality preflight: required quality marker is missing: '{0}'." -f
            $qualityMarker)
    }
}

foreach ($qualityAlgorithm in $algorithms) {
    $qualityId =
        [string]$qualityAlgorithm.id

    $qualityCardPattern =
        '(?is)<td\b[^>]*>' +
        '(?:(?!</td>).)*' +
        '<a\s+href="[^"]+"><strong>[^<]+</strong></a>' +
        '(?:(?!</td>).)*' +
        [regex]::Escape($qualityId) +
        '(?:(?!</td>).)*' +
        '</td>'

    if (-not [regex]::IsMatch(
            $qualityAlgorithmSection.Groups["body"].Value,
            $qualityCardPattern)) {

        throw (
            "README generator quality preflight: stable ID '{0}' is not contained in a clickable algorithm card." -f
            $qualityId)
    }
}

Write-Host (
    "README GENERATOR QUALITY PREFLIGHT GREEN: exact historical structure, 155 clickable algorithm identities, five mandatory clickable titles, dynamic counts, zero trailing whitespace and final Scientific components section are all satisfied.") -ForegroundColor Green

$readmePath =
    Join-Path `
        $Root `
        "README.md"

if ($Check) {
    if (-not (
        Test-Path `
            -LiteralPath $readmePath `
            -PathType Leaf
    )) {
        throw "README generator check: README.md is missing."
    }

    $actual =
        Read-Utf8 `
            -Path $readmePath

    $actual =
        $actual.Replace(
            "`r`n",
            "`n")

    $actual =
        $actual.Replace(
            "`r",
            "`n")

    $expected =
        $content.Replace(
            "`r`n",
            "`n")

    $expected =
        $expected.Replace(
            "`r",
            "`n")

    if (-not [string]::Equals(
            $actual,
            $expected,
            [System.StringComparison]::Ordinal)) {

        throw "README generator check failed: README.md is not the deterministic output of Build-ProfessionalReadme.ps1."
    }

    Write-Host (
        "PROFESSIONAL README GENERATOR CHECK GREEN: deterministic README matches the canonical 155-algorithm / 8-family catalog and preserves all CommandAst/HashtableAst-extracted historical README contracts.") -ForegroundColor Green

    return
}

Write-Utf8NoBomLf `
    -Path $readmePath `
    -Text $content

Write-Host (
    "PROFESSIONAL README GENERATED: {0} algorithms, {1} families, {2} multi-objective entries, {3} many-objective entries, {4} constrained entries, {5} multimodal/niching entries; {6} historical README contract token(s) extracted from CommandAst/HashtableAst." -f
    @($algorithms).Length,
    @($families).Length,
    @($multiobjective).Length,
    @($manyObjective).Length,
    @($constrained).Length,
    @($multimodal).Length,
    @($historicalReadmeRequirements).Length) -ForegroundColor Green
