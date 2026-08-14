@page getting_started Getting started

# Getting started

MetaheuristicsPlatform separates the optimization problem, algorithm, parameters,
stopping criteria, callbacks and randomization policy.

For algorithms that require no external domain components, prefer a stable factory ID:

```csharp
var algorithm =
    MetaheuristicFactory.Create<DifferentialEvolutionOptimizer>(
        MetaheuristicAlgorithmIds.DifferentialEvolution);
```

For composed algorithms such as generic Simulated Annealing, construct the typed
components once and register the same stable public ID.
