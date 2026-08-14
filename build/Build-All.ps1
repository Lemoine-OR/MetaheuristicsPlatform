[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"

& (Join-Path $Root "build\Build-Validated.ps1") -Root $Root
& (Join-Path $Root "docs\build-documentation.ps1") -Root $Root

Write-Host "Full build completed." -ForegroundColor Green
