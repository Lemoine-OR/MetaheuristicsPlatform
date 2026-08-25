[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),

    [string]$CommitId = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-NativeExecutable(
    [Parameter(Mandatory = $true)]
    [string]$CommandName
) {
    $commandObjects =
        @(
            Get-Command `
                -Name $CommandName `
                -CommandType Application `
                -All `
                -ErrorAction Stop
        )

    $resolvedPaths =
        New-Object System.Collections.Generic.List[string]

    foreach ($commandObject in $commandObjects) {
        $candidatePath =
            [string]$commandObject.Path

        if ([string]::IsNullOrWhiteSpace($candidatePath)) {
            $candidatePath =
                [string]$commandObject.Source
        }

        if ([string]::IsNullOrWhiteSpace($candidatePath)) {
            continue
        }

        $fullPath =
            [System.IO.Path]::GetFullPath($candidatePath)

        if (-not [System.IO.File]::Exists($fullPath)) {
            continue
        }

        if ($resolvedPaths -notcontains $fullPath) {
            [void]$resolvedPaths.Add($fullPath)
        }
    }

    if ($resolvedPaths.Count -eq 0) {
        throw (
            "Unable to resolve native executable '{0}' to an existing file." -f
            $CommandName)
    }

    return [string]$resolvedPaths[0]
}

function Resolve-RepositoryCommit(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
) {
    $gitExe =
        Resolve-NativeExecutable `
            -CommandName "git.exe"

    $gitStartInfo =
        New-Object System.Diagnostics.ProcessStartInfo

    $gitStartInfo.FileName =
        $gitExe

    $gitStartInfo.Arguments =
        "rev-parse HEAD"

    $gitStartInfo.WorkingDirectory =
        [System.IO.Path]::GetFullPath($RepositoryRoot)

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
                $RepositoryRoot,
                $gitProcess.ExitCode,
                $gitError)
        }

        if (-not [regex]::IsMatch(
                $gitOutput,
                '^[0-9a-fA-F]{40}$')) {

            throw (
                "git rev-parse HEAD returned an invalid commit id: '{0}'." -f
                $gitOutput)
        }

        return $gitOutput.ToLowerInvariant()
    }
    finally {
        $gitProcess.Dispose()
    }
}

$rootFull =
    [System.IO.Path]::GetFullPath($Root)

$versionJson =
    Get-Content (Join-Path $rootFull "version.json") -Raw |
    ConvertFrom-Json

$releaseVersion =
    [string]$versionJson.version

$tag =
    "v$releaseVersion"

$release =
    Join-Path $rootFull "Documentation\release"

if (Test-Path $release) {
    Remove-Item $release -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $release | Out-Null

$binaryRoot =
    Join-Path $rootFull "src\MetaheuristicsPlatform\bin\Release\net10.0"

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
    -Path (Join-Path $rootFull "Documentation\site\*") `
    -DestinationPath $documentationZip `
    -Force

$files =
    @(
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

if ([string]::IsNullOrWhiteSpace($CommitId)) {
    $commit =
        Resolve-RepositoryCommit `
            -RepositoryRoot $rootFull
}
else {
    if (-not [regex]::IsMatch(
            $CommitId,
            '^[0-9a-fA-F]{40}$')) {

        throw (
            "Prepare-ReleaseAssets CommitId must be exactly 40 hexadecimal characters; found '{0}'." -f
            $CommitId)
    }

    $commit =
        $CommitId.ToLowerInvariant()
}

[pscustomobject]@{
    ReleaseVersion = $releaseVersion
    BuildVersion = $releaseVersion
    Tag = $tag
    CommitId = $commit
    Prerelease = $releaseVersion.Contains("-")
}
