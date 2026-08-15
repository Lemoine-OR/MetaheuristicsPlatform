[CmdletBinding()]
param(
    [string]$Repository = "Lemoine-OR/MetaheuristicsPlatform"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$gh = Get-Command gh -ErrorAction SilentlyContinue
if ($null -eq $gh) {
    throw "GitHub CLI (gh) is required for repository metadata configuration."
}

& gh auth status
if ($LASTEXITCODE -ne 0) { throw "GitHub CLI authentication is not ready." }

$description = "High-performance scientific C#/.NET metaheuristics with a common generic architecture, stable IDs and literature-backed documentation."
$homepage = "https://lemoine-or.github.io/MetaheuristicsPlatform/"

& gh api --method PATCH "repos/$Repository" -f "description=$description" -f "homepage=$homepage" | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Unable to update repository description/homepage." }

$topicArgs = @(
    "api", "--method", "PUT", "repos/$Repository/topics",
    "-H", "Accept: application/vnd.github+json",
    "-f", "names[]=csharp",
    "-f", "names[]=dotnet",
    "-f", "names[]=metaheuristics",
    "-f", "names[]=optimization",
    "-f", "names[]=operations-research",
    "-f", "names[]=particle-swarm-optimization",
    "-f", "names[]=differential-evolution",
    "-f", "names[]=simulated-annealing",
    "-f", "names[]=tabu-search",
    "-f", "names[]=local-search"
)
& gh @topicArgs | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Unable to update repository topics." }

Write-Host "Repository metadata updated successfully." -ForegroundColor Green
Write-Host "Description: $description"
Write-Host "Homepage: $homepage"
Write-Host "Note: GitHub does not expose a per-repository browser favicon. Configure Social preview manually in Settings if desired."
