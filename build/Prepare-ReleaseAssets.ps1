[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$versionJson =
    Get-Content (Join-Path $Root "version.json") -Raw |
    ConvertFrom-Json

$releaseVersion =
    [string]$versionJson.version

$tag =
    "v$releaseVersion"

$release =
    Join-Path $Root "Documentation\release"

if (Test-Path $release) {
    Remove-Item $release -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $release | Out-Null

$binaryRoot =
    Join-Path $Root "src\MetaheuristicsPlatform\bin\Release\net10.0"

if (-not (Test-Path $binaryRoot)) {
    throw "Release binary directory does not exist. Run build/Build-All.ps1 first."
}

$binaryZip =
    Join-Path $release "MetaheuristicsPlatform-$releaseVersion-binaries.zip"

$documentationZip =
    Join-Path $release "MetaheuristicsPlatform-$releaseVersion-documentation.zip"

Compress-Archive `
    -Path (Join-Path $binaryRoot "*") `
    -DestinationPath $binaryZip `
    -Force

Compress-Archive `
    -Path (Join-Path $Root "Documentation\site\*") `
    -DestinationPath $documentationZip `
    -Force

$files = @(
    Get-ChildItem $release -File
)

foreach ($file in $files) {
    $hash =
        (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()

    "$hash  $($file.Name)" |
        Set-Content `
            -Path "$($file.FullName).sha256" `
            -Encoding utf8
}

# Release asset Git commit proof via System.Diagnostics.Process.
$gitExe =
    (Get-Command git.exe -CommandType Application -ErrorAction Stop).Source

$gitStartInfo =
    New-Object System.Diagnostics.ProcessStartInfo

$gitStartInfo.FileName =
    $gitExe

$gitStartInfo.Arguments =
    "rev-parse HEAD"

$gitStartInfo.WorkingDirectory =
    [System.IO.Path]::GetFullPath($Root)

$gitStartInfo.UseShellExecute =
    $false

$gitStartInfo.RedirectStandardOutput =
    $true

$gitStartInfo.RedirectStandardError =
    $true

$gitStartInfo.CreateNoWindow =
    $true

$gitProcess =
    New-Object System.Diagnostics.Process

$gitProcess.StartInfo =
    $gitStartInfo

try {
    if (-not $gitProcess.Start()) {
        throw "Unable to start git.exe for release commit proof."
    }

    $gitOutputTask =
        $gitProcess.StandardOutput.ReadToEndAsync()

    $gitErrorTask =
        $gitProcess.StandardError.ReadToEndAsync()

    $gitProcess.WaitForExit()

    $gitOutput =
        ([string]$gitOutputTask.Result).Trim()

    $gitError =
        ([string]$gitErrorTask.Result).Trim()

    if ($gitProcess.ExitCode -ne 0) {
        throw (
            "Unable to prove release commit from repository root '{0}'. Exit={1}. {2}" -f
            $Root,
            $gitProcess.ExitCode,
            $gitError)
    }

    if ([string]::IsNullOrWhiteSpace($gitOutput)) {
        throw "git rev-parse HEAD returned an empty release commit."
    }

    $commit =
        $gitOutput
}
finally {
    $gitProcess.Dispose()
}

[pscustomobject]@{
    ReleaseVersion = $releaseVersion
    BuildVersion = $releaseVersion
    Tag = $tag
    CommitId = $commit
    Prerelease = $releaseVersion.Contains("-")
}
