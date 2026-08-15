# API stability

MetaheuristicsPlatform follows the same repository discipline as ULSAlgorithms.

## Stable contract now

Even during the 0.x development line, the following are treated as compatibility
contracts:
- public algorithm catalog IDs;
- public Simulated Annealing cooling-schedule IDs (`sa.cooling.*`);
- public Tabu Search stable ID (`tabu-search-glover`);
- public Reactive Tabu Search stable ID (`reactive-tabu-search-battiti-tecchiolli-1994`);
- public Tabu Search component IDs (`ts.*`);
- scientific method identity;
- serialized/reproducibility-facing identifiers;
- documentation URLs generated from stable IDs.

## Before 1.0

Type-level APIs may still evolve while the generic architecture is being completed.
Breaking changes must be documented in `CHANGELOG.md` and must not silently reuse a
stable ID for a different scientific method.

## 1.x target

The 1.x line will freeze the public common lifecycle and factory/catalog conventions in
the same spirit as ULSAlgorithms.
