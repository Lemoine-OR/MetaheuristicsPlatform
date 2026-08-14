$root = Split-Path -Parent $PSScriptRoot

if (Test-Path (Join-Path $root "MetaheuristicsPlatform.sln")) {
    return "MetaheuristicsPlatform.sln"
}

if (Test-Path (Join-Path $root "MetaheuristicsPlatform.slnx")) {
    return "MetaheuristicsPlatform.slnx"
}

$project =
    Get-ChildItem `
        -Path (Join-Path $root "src") `
        -Recurse `
        -File `
        -Filter "*.csproj" |
    Select-Object -First 1

if ($null -ne $project) {
    return $project.FullName
}

return $null
