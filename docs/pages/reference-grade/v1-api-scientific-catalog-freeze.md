@page reference_grade_v1_api_catalog_freeze v1.0 Public API and Scientific Catalog Freeze

# v1.0 Public API and Scientific Catalog Freeze

MetaheuristicsPlatform v1.0.0 establishes the first stable Semantic Versioning
compatibility baseline after the v0.166.0-v0.173.0 reference-grade consolidation.

## Frozen compatibility surface

The v1 baseline records:

- every public signature exported by the `MetaheuristicsPlatform` assembly at the
  v0.173.0 source commit `7ac478247fc88052296565f22a2eb2d2809f0b5f`;
- all 155 stable scientific algorithm IDs and their class, DOI, primary family,
  factory mode and canonical documentation mapping;
- all 8 v1 scientific family IDs and names.

Future compatible 1.x releases may add public APIs, algorithm identities and families.
They must not remove or silently remap any v1 baseline identity or public signature.

## Machine-checkable evidence

The baseline is enforced by:

- `docs/v1-public-api-baseline.json`;
- `docs/v1-scientific-catalog-baseline.json`;
- `docs/v1-compatibility-freeze-manifest.json`;
- `V1PublicApiFreezeTests`;
- `docs/Test-V1CompatibilityFreeze.ps1`.

The public API check is executed inside the .NET 10 test process. This avoids loading a
net10.0 assembly through Windows PowerShell 5.1 / .NET Framework.

## Release invariant

The v1.0.0 release itself introduces no new algorithm identity and changes no file under
`src/MetaheuristicsPlatform`. The scientific catalog is byte-identical to v0.173.0.
Only release metadata, documentation, compatibility snapshots, tests and validation
automation are added or updated.

## Semantic Versioning rule

For 1.x, additive compatible extensions are allowed. Removing a v1 baseline public
signature, deleting a baseline stable ID, or changing its frozen scientific mapping is
a breaking change and requires a major-version decision.

## Navigation

Return to @ref reference_grade_consolidation "Reference-Grade Consolidation" or
@ref md_mainpage "MetaheuristicsPlatform".