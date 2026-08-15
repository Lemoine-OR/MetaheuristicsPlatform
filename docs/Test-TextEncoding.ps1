[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string[]]$AdditionalPath = @()
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Join-Chars([int[]]$Codes) {
    $builder = New-Object System.Text.StringBuilder
    foreach ($code in $Codes) { [void]$builder.Append([char]$code) }
    return $builder.ToString()
}

$badPatterns = @(
    (Join-Chars @(0x00C2,0x00B7)),
    (Join-Chars @(0x00E2,0x20AC,0x201D)),
    (Join-Chars @(0x00E2,0x20AC,0x201C)),
    (Join-Chars @(0x00E2,0x20AC,0x2122)),
    (Join-Chars @(0x00C3,0x00A7)),
    (Join-Chars @(0x00C3,0x00BC)),
    (Join-Chars @(0x00C3,0x00AD)),
    (Join-Chars @(0x00C3,0x00A9)),
    (Join-Chars @(0x00C3,0x00A8)),
    (Join-Chars @(0x00C3,0x00A0))
)

$extensions = @(
    ".md", ".json", ".ps1", ".cs", ".yml", ".yaml", ".cff", ".props",
    ".targets", ".sln", ".csproj", ".svg", ".css", ".html", ".txt"
)

$roots = @(
    (Join-Path $Root "README.md"),
    (Join-Path $Root "CHANGELOG.md"),
    (Join-Path $Root "CITATION.cff"),
    (Join-Path $Root "docs"),
    (Join-Path $Root "src"),
    (Join-Path $Root "tests"),
    (Join-Path $Root "build"),
    (Join-Path $Root "tools"),
    (Join-Path $Root ".github")
) + $AdditionalPath

$files = New-Object System.Collections.Generic.List[System.IO.FileInfo]
foreach ($candidate in $roots) {
    if (-not (Test-Path -LiteralPath $candidate)) { continue }
    $item = Get-Item -LiteralPath $candidate
    if ($item.PSIsContainer) {
        Get-ChildItem -LiteralPath $candidate -Recurse -File | ForEach-Object {
            if ($extensions -contains $_.Extension.ToLowerInvariant()) { $files.Add($_) }
        }
    }
    elseif ($extensions -contains $item.Extension.ToLowerInvariant()) {
        $files.Add($item)
    }
}

$failures = New-Object System.Collections.Generic.List[string]
foreach ($file in $files) {
    $text = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    foreach ($bad in $badPatterns) {
        if ($text.Contains($bad)) {
            $failures.Add($file.FullName)
            break
        }
    }
}

if ($failures.Count -gt 0) {
    $unique = @($failures | Sort-Object -Unique)
    throw "Text encoding validation failed; common mojibake markers found in: $($unique -join '; ')"
}

Write-Host "Text encoding validation passed: no common UTF-8 mojibake markers found." -ForegroundColor Green
