# Repository & Documentation Parity Audit — v0.19.0

Normative reference: `Lemoine-OR/ULSAlgorithms`.

## Repository-level parity

| ULSAlgorithms element | MetaheuristicsPlatform v0.19 |
|---|---|
| README logo + badges + project links | Present |
| Family panels | Present |
| Per-algorithm panels with stable IDs | Present |
| `API-STABILITY.md` | Present |
| `CITATION.cff` | Present |
| `.github/workflows/build.yml` | Present |
| `.github/workflows/documentation.yml` | Present |
| `.github/workflows/release.yml` | Present |
| `docs/Doxyfile` | Present |
| `docs/mainpage.md` | Present |
| `docs/algorithm-catalog.json` | Present |
| `docs/build-documentation.ps1` | Present |
| documentation link validation | Present |
| project-specific brand assets | Present |
| common Lemoine-OR Algorithms icon language | Present |

## Stronger mathematical contract

Every public algorithm page is additionally required to contain:

1. General description
2. Technical specifications
3. Complexity
4. Applicability
5. Detailed operation
6. Parameters
7. API example
8. Stable factory ID
9. Mathematical details
   - problem formulation
   - update equations / iterations
   - assumptions
   - convergence conditions
   - scientific references
10. DOI / publication provenance

`docs/Test-DocumentationParity.ps1` fails when any required section is missing.

## Current public inventory

- Particle Swarm Optimization
- Differential Evolution
- jDE
- JADE
- SHADE
- L-SHADE
- Simulated Annealing

Future algorithms must enter the catalog and documentation in the same commit as their
implementation.
