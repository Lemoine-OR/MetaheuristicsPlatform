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

# CANONICAL-TEXT-SHA256-CONTRACT
function Get-CanonicalTextSha256FromText([string]$Text) {
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

    $bytes =
        $encoding.GetBytes(
            $canonical)

    $sha256 =
        [System.Security.Cryptography.SHA256]::Create()

    try {
        $hashBytes =
            $sha256.ComputeHash(
                $bytes)
    }
    finally {
        $sha256.Dispose()
    }

    return (
        [System.BitConverter]::ToString(
            $hashBytes
        ).Replace(
            "-",
            ""
        ).ToLowerInvariant())
}

function Get-CanonicalTextSha256([string]$Path) {
    $text =
        [System.IO.File]::ReadAllText(
            $Path,
            [System.Text.Encoding]::UTF8)

    return Get-CanonicalTextSha256FromText `
        -Text $text
}

$canonicalLfProbe =
    "alpha`nbeta`n"

$canonicalCrlfProbe =
    "alpha`r`nbeta`r`n"

$canonicalCrProbe =
    "alpha`rbeta`r"

$canonicalLfHash =
    Get-CanonicalTextSha256FromText `
        -Text $canonicalLfProbe

$canonicalCrlfHash =
    Get-CanonicalTextSha256FromText `
        -Text $canonicalCrlfProbe

$canonicalCrHash =
    Get-CanonicalTextSha256FromText `
        -Text $canonicalCrProbe

if ($canonicalLfHash -ne $canonicalCrlfHash -or
    $canonicalLfHash -ne $canonicalCrHash) {

    throw "Canonical text SHA-256 contract failed across LF/CRLF/CR."
}

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required v1 freeze file is missing: '$Path'."
    }

    return (
        [System.IO.File]::ReadAllText(
            $Path,
            [System.Text.Encoding]::UTF8) |
        ConvertFrom-Json
    )
}

$manifestPath =
    Join-Path `
        $Root `
        "docs\v1-compatibility-freeze-manifest.json"

$catalogBaselinePath =
    Join-Path `
        $Root `
        "docs\v1-scientific-catalog-baseline.json"

$apiBaselinePath =
    Join-Path `
        $Root `
        "docs\v1-public-api-baseline.json"

$currentCatalogPath =
    Join-Path `
        $Root `
        "docs\algorithm-catalog.json"

$manifest =
    Read-Json `
        -Path $manifestPath

$catalogBaseline =
    Read-Json `
        -Path $catalogBaselinePath

$apiBaseline =
    Read-Json `
        -Path $apiBaselinePath

$currentCatalog =
    Read-Json `
        -Path $currentCatalogPath

if ([string]$manifest.baselineRelease -ne "1.0.0") {
    throw "Unexpected v1 freeze baseline release."
}

if ([string]$manifest.sourceCommit -ne
    "7ac478247fc88052296565f22a2eb2d2809f0b5f") {

    throw "Unexpected v1 freeze source commit."
}
$hashSemanticsProperty =
    $manifest.PSObject.Properties["hashSemantics"]

if ($null -eq $hashSemanticsProperty -or
    [string]$hashSemanticsProperty.Value -ne
    "canonical-utf8-lf-no-bom") {

    throw "Unexpected v1 freeze hash semantics."
}

if ([string]$manifest.catalogBaselineSha256 -ne
    (Get-CanonicalTextSha256 -Path $catalogBaselinePath)) {

    throw "Scientific catalog baseline SHA-256 mismatch."
}

if ([string]$manifest.publicApiBaselineSha256 -ne
    (Get-CanonicalTextSha256 -Path $apiBaselinePath)) {

    throw "Public API baseline SHA-256 mismatch."
}

$baselineAlgorithms =
    @(
        $catalogBaseline.algorithms
    )

$baselineFamilies =
    @(
        $catalogBaseline.families
    )

$currentAlgorithms =
    @(
        $currentCatalog.algorithms
    )

$currentFamilies =
    @(
        $currentCatalog.families
    )

if (@($baselineAlgorithms).Length -ne 155) {
    throw (
        "The v1 scientific catalog baseline must contain exactly 155 algorithm identities; found {0}." -f
        @($baselineAlgorithms).Length)
}

if (@($baselineFamilies).Length -ne 8) {
    throw (
        "The v1 family baseline must contain exactly 8 families; found {0}." -f
        @($baselineFamilies).Length)
}

if ([int]$manifest.algorithmCount -ne
    @($baselineAlgorithms).Length) {

    throw "Freeze manifest algorithm count mismatch."
}

if ([int]$manifest.familyCount -ne
    @($baselineFamilies).Length) {

    throw "Freeze manifest family count mismatch."
}

$duplicateCurrentIds =
    @(
        $currentAlgorithms |
        Group-Object id |
        Where-Object {
            @($_.Group).Length -ne 1
        }
    )

if (@($duplicateCurrentIds).Length -ne 0) {
    throw "Current scientific catalog contains duplicate stable IDs."
}

foreach ($baseline in $baselineAlgorithms) {
    $matches =
        @(
            $currentAlgorithms |
            Where-Object {
                [string]$_.id -eq
                [string]$baseline.id
            }
        )

    if (@($matches).Length -ne 1) {
        throw (
            "Frozen v1 algorithm ID missing or ambiguous: '{0}'." -f
            [string]$baseline.id)
    }

    $current =
        $matches[0]

    foreach ($propertyName in @(
        "class",
        "doi",
        "category",
        "factoryMode",
        "page"
    )) {
        $baselineValue =
            [string]$baseline.$propertyName

        $currentValue =
            [string]$current.$propertyName

        if ($baselineValue -ne
            $currentValue) {

            throw (
                "Frozen v1 scientific mapping changed for '{0}': property '{1}' expected '{2}', found '{3}'." -f
                [string]$baseline.id,
                $propertyName,
                $baselineValue,
                $currentValue)
        }
    }
}

foreach ($baselineFamily in $baselineFamilies) {
    $matches =
        @(
            $currentFamilies |
            Where-Object {
                [string]$_.id -eq
                [string]$baselineFamily.id
            }
        )

    if (@($matches).Length -ne 1) {
        throw (
            "Frozen v1 family ID missing or ambiguous: '{0}'." -f
            [string]$baselineFamily.id)
    }

    if ([string]$matches[0].name -ne
        [string]$baselineFamily.name) {

        throw (
            "Frozen v1 family name changed for '{0}'." -f
            [string]$baselineFamily.id)
    }
}

$apiSignatures =
    @(
        $apiBaseline.signatures
    )

if (@($apiSignatures).Length -le 100) {
    throw "The public API baseline is unexpectedly small."
}

if ([int]$manifest.publicApiSignatureCount -ne
    @($apiSignatures).Length) {

    throw "Freeze manifest public API signature count mismatch."
}

$apiStability =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "API-STABILITY.md"),
        [System.Text.Encoding]::UTF8)

$readme =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "README.md"),
        [System.Text.Encoding]::UTF8)

foreach ($markerText in @(
    "## v1.0.0 - stable compatibility baseline",
    "155 scientific catalog identities",
    "Semantic Versioning"
)) {
    if ($apiStability.IndexOf(
            $markerText,
            [System.StringComparison]::Ordinal) -lt 0) {

        throw (
            "API-STABILITY v1 marker missing: '{0}'." -f
            $markerText)
    }
}

if ($readme.IndexOf(
        "## v1.0 stability contract",
        [System.StringComparison]::Ordinal) -lt 0) {

    throw "README v1 stability section is missing."
}

$documentationParity =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "docs\Test-DocumentationParity.ps1"),
        [System.Text.Encoding]::UTF8)

if ($documentationParity.IndexOf(
        "# CURRENT-RELEASE-VERSION-CONTRACT",
        [System.StringComparison]::Ordinal) -lt 0) {

    throw "Documentation parity current-release version contract is missing."
}

$legacyParityLock =
    'version.version -ne "0.173.0"'

$legacyParityMessage =
    "version.json must be 0.173.0 for this release"

if ($documentationParity.IndexOf(
        $legacyParityLock,
        [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
    $documentationParity.IndexOf(
        $legacyParityMessage,
        [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {

    throw "Documentation parity regressed to a release-specific current-version literal."
}

Write-Host (
    "V1 COMPATIBILITY FREEZE GREEN: {0} baseline public signatures preserved; {1} frozen scientific IDs and {2} family IDs remain stable; additive 1.x extensions remain allowed." -f
    @($apiSignatures).Length,
    @($baselineAlgorithms).Length,
    @($baselineFamilies).Length) -ForegroundColor Green