[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Push-Location $Root
try {
    $target = & .\tools\Get-BuildTarget.ps1

    if ($null -eq $target) {
        throw "No build target found."
    }

    & dotnet restore $target
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed." }

    & dotnet build $target -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

    & dotnet test .\tests\MetaheuristicsPlatform.Tests\MetaheuristicsPlatform.Tests.csproj -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed." }

    & .\docs\Test-DocumentationParity.ps1 -Root $Root
}
finally {
    Pop-Location
}
