[CmdletBinding()]
param(
    [ValidateSet(
        "VariationCrossover",
        "VariationShape",
        "EndToEnd",
        "All")]
    [string]$Suite = "VariationCrossover"
)

$ErrorActionPreference = "Stop"

$root =
    Split-Path `
        -Parent `
        $PSScriptRoot

$project =
    Join-Path `
        $PSScriptRoot `
        "MetaheuristicsPlatform.Benchmarks\MetaheuristicsPlatform.Benchmarks.csproj"

$timestamp =
    Get-Date -Format "yyyyMMdd-HHmmss"

$report =
    Join-Path `
        $PSScriptRoot `
        "Reports\DeCalibration\$timestamp"

New-Item `
    -ItemType Directory `
    -Force `
    -Path $report |
    Out-Null

$filters =
    switch ($Suite) {
        "VariationCrossover" {
            @("*DeVariationCrossoverBenchmarks*")
        }

        "VariationShape" {
            @("*DeVariationShapeSensitivityBenchmarks*")
        }

        "EndToEnd" {
            @("*DeEndToEndCalibrationBenchmarks*")
        }

        "All" {
            @(
                "*DeVariationCrossoverBenchmarks*",
                "*DeVariationShapeSensitivityBenchmarks*",
                "*DeEndToEndCalibrationBenchmarks*"
            )
        }
    }

Push-Location $root

try {
    foreach ($filter in $filters) {
        dotnet run `
            -c Release `
            --project $project `
            -- `
            --filter $filter

        if ($LASTEXITCODE -ne 0) {
            throw "BenchmarkDotNet failed for filter $filter."
        }
    }

    $artifacts =
        Join-Path `
            $root `
            "BenchmarkDotNet.Artifacts\results"

    if (Test-Path $artifacts) {
        Copy-Item `
            -Path (Join-Path $artifacts "*De*") `
            -Destination $report `
            -Force `
            -ErrorAction SilentlyContinue
    }

    [System.IO.File]::WriteAllText(
        (Join-Path $report "machine.txt"),
        @"
Date: $(Get-Date -Format o)
Suite: $Suite
OS: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription)
Framework: $([System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription)
Processors: $([Environment]::ProcessorCount)
"@,
        [System.Text.UTF8Encoding]::new($false))

    Write-Host ""
    Write-Host "DE calibration completed." -ForegroundColor Green
    Write-Host "Report: $report"
}
finally {
    Pop-Location
}