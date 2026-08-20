# API stability

MetaheuristicsPlatform follows the same repository discipline as ULSAlgorithms.

## Stable contract now

Even during the 0.x development line, the following are treated as compatibility
contracts:
- public algorithm catalog IDs;
- public Simulated Annealing cooling-schedule IDs (`sa.cooling.*`);
- public Threshold Accepting ID (`threshold-accepting-dueck-scheuer-1990`) introduced in v0.33.0;
- public Great Deluge ID (`great-deluge-dueck-1993`) introduced in v0.34.0;
- public Record-to-Record Travel ID (`record-to-record-travel-dueck-1993`) introduced in v0.34.0;
- public Late Acceptance Hill Climbing ID (`late-acceptance-hill-climbing-burke-bykov-2017`) introduced in v0.35.0;
- public Tabu Search stable ID (`tabu-search-glover`);
- public Reactive Tabu Search stable ID (`reactive-tabu-search-battiti-tecchiolli-1994`);
- public Tabu Search component IDs (`ts.*`);
- public Local Search Foundation algorithm IDs introduced in v0.23.0;
- public Multi-Start Local Search ID (`multi-start-local-search`) introduced in v0.24.0;
- public Iterated Local Search ID (`iterated-local-search-lourenco-martin-stutzle`) introduced in v0.24.0;
- public Variable Neighborhood Descent ID (`variable-neighborhood-descent`) introduced in v0.25.0;
- public canonical Variable Neighborhood Search ID (`variable-neighborhood-search-mladenovic-hansen`) introduced in v0.25.0;
- public Guided Local Search ID (`guided-local-search-voudouris-tsang-1999`) introduced in v0.26.0;
- public Reduced Variable Neighborhood Search ID (`reduced-variable-neighborhood-search`) introduced in v0.27.0;
- public General Variable Neighborhood Search ID (`general-variable-neighborhood-search`) introduced in v0.27.0;
- public Skewed Variable Neighborhood Search ID (`skewed-variable-neighborhood-search-hansen-mladenovic-2001`) introduced in v0.27.0;
- public canonical GRASP ID (`grasp-feo-resende-1995`) introduced in v0.28.0;
- public Reactive GRASP ID (`reactive-grasp-prais-ribeiro-2000`) introduced in v0.29.0;
- public GRASP with Path Relinking ID (`grasp-path-relinking`) introduced in v0.30.0;
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
