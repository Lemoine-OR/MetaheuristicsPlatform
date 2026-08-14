[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$errors = New-Object System.Collections.Generic.List[string]

$markdownFiles = @(
    Get-ChildItem `
        -Path (Join-Path $Root "docs") `
        -Recurse `
        -File `
        -Filter "*.md"
)

foreach ($file in $markdownFiles) {
    $content = Get-Content $file.FullName -Raw
    $matches = [regex]::Matches(
        $content,
        '\[[^\]]+\]\((?!https?://|mailto:|#)([^)]+)\)')

    foreach ($match in $matches) {
        $target = $match.Groups[1].Value.Split('#')[0]

        if ([string]::IsNullOrWhiteSpace($target)) {
            continue
        }

        $resolved =
            [System.IO.Path]::GetFullPath(
                (Join-Path $file.DirectoryName $target))

        if (-not (Test-Path $resolved)) {
            $errors.Add("$($file.FullName): broken local link '$target'")
        }
    }
}

$site =
    Join-Path $Root "Documentation\site"

if (Test-Path $site) {
    $htmlFiles = @(
        Get-ChildItem $site -Recurse -File -Filter "*.html"
    )

    foreach ($file in $htmlFiles) {
        $content = Get-Content $file.FullName -Raw
        $matches = [regex]::Matches(
            $content,
            '(?:href|src)="(?!https?://|mailto:|#|javascript:)([^"]+)"')

        foreach ($match in $matches) {
            $target = $match.Groups[1].Value.Split('#')[0].Split('?')[0]

            if ([string]::IsNullOrWhiteSpace($target)) {
                continue
            }

            $resolved =
                [System.IO.Path]::GetFullPath(
                    (Join-Path $file.DirectoryName $target))

            if (-not (Test-Path $resolved)) {
                $errors.Add("$($file.FullName): broken generated link '$target'")
            }
        }
    }
}

if ($errors.Count -gt 0) {
    throw ("Documentation link validation failed:`n" + ($errors -join "`n"))
}

Write-Host "Documentation link validation passed: no broken local href/src targets found." -ForegroundColor Green
