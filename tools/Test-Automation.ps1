[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"

& (Join-Path $Root "tools\Test-PowerShellSyntax.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DocumentationParity.ps1") -Root $Root

Write-Host "Automation preflight passed." -ForegroundColor Green
