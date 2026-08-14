[CmdletBinding()]
param(
    [ValidateSet("Parallel", "Crossover", "Shape", "Objective", "Social", "All")]
    [string]$Suite = "Parallel"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $PSScriptRoot "MetaheuristicsPlatform.Benchmarks\MetaheuristicsPlatform.Benchmarks.csproj"

if (-not (Test-Path $project)) {
    throw "Benchmark project not found: $project"
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportRoot = Join-Path $PSScriptRoot "Reports\PsoCalibration\$timestamp"
New-Item -ItemType Directory -Force -Path $reportRoot | Out-Null

Push-Location $root

try {
    Write-Host ""
    Write-Host "=== BUILD RELEASE ===" -ForegroundColor Cyan
    & dotnet build "MetaheuristicsPlatform.sln" -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed."
    }

    $filters = switch ($Suite) {
        "Parallel" {
            @("*PsoParallelCalibrationBenchmarks*")
        }
        "Crossover" {
            @("*PsoParallelCrossoverBenchmarks*")
        }
        "Shape" {
            @("*PsoParallelShapeSensitivityBenchmarks*")
        }
        "Objective" {
            @("*PsoObjectiveCostCalibrationBenchmarks*")
        }
        "Social" {
            @("*PsoSocialTopologyCalibrationBenchmarks*")
        }
        "All" {
            @(
                "*PsoParallelCalibrationBenchmarks*",
                "*PsoParallelCrossoverBenchmarks*",
                "*PsoParallelShapeSensitivityBenchmarks*",
                "*PsoObjectiveCostCalibrationBenchmarks*",
                "*PsoSocialTopologyCalibrationBenchmarks*"
            )
        }
    }

    foreach ($filter in $filters) {
        Write-Host ""
        Write-Host "=== BENCHMARK $filter ===" -ForegroundColor Cyan

        & dotnet run `
            -c Release `
            --no-build `
            --project $project `
            -- `
            --filter $filter

        if ($LASTEXITCODE -ne 0) {
            throw "BenchmarkDotNet failed for filter $filter."
        }
    }

    $artifactRoot = Join-Path $root "BenchmarkDotNet.Artifacts"

    if (Test-Path $artifactRoot) {
        Copy-Item `
            $artifactRoot `
            (Join-Path $reportRoot "BenchmarkDotNet.Artifacts") `
            -Recurse `
            -Force
    }

    $dotnetSdk = (& dotnet --version | Select-Object -First 1).Trim()

    $machineInfo = @()
    $machineInfo += "Timestamp: $(Get-Date -Format o)"
    $machineInfo += "Computer: $env:COMPUTERNAME"
    $machineInfo += "OS: $([System.Environment]::OSVersion.VersionString)"
    $machineInfo += "ProcessorCount: $([System.Environment]::ProcessorCount)"
    $machineInfo += "EnvironmentRuntimeVersion: $([System.Environment]::Version)"
    $machineInfo += "DotnetSdk: $dotnetSdk"
    $machineInfo += "PowerShell: $($PSVersionTable.PSVersion)"
    $machineInfo += "Architecture: $env:PROCESSOR_ARCHITECTURE"

    $machineInfo |
        Set-Content `
            (Join-Path $reportRoot "machine-info.txt") `
            -Encoding utf8

    Write-Host ""
    Write-Host "Calibration completed." -ForegroundColor Green
    Write-Host "Report: $reportRoot"
}
finally {
    Pop-Location
}