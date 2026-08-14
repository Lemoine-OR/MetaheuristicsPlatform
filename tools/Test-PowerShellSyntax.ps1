[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$errors = New-Object System.Collections.Generic.List[string]

foreach ($file in @(Get-ChildItem $Root -Recurse -File -Filter "*.ps1")) {
    $tokens = $null
    $parseErrors = $null

    [System.Management.Automation.Language.Parser]::ParseFile(
        $file.FullName,
        [ref]$tokens,
        [ref]$parseErrors) | Out-Null

    foreach ($error in @($parseErrors)) {
        $errors.Add("$($file.FullName): $($error.Message)")
    }
}

if ($errors.Count -gt 0) {
    throw ("PowerShell syntax validation failed:`n" + ($errors -join "`n"))
}

Write-Host "PowerShell syntax validation passed." -ForegroundColor Green
