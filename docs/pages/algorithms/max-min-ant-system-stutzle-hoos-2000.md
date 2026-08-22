@page max_min_ant_system_stutzle_hoos_2000 MAX-MIN Ant System - Stutzle-Hoos

# MAX-MIN Ant System - Stutzle-Hoos

## General description

MAX-MIN Ant System (MMAS) is the Stutzle-Hoos Ant System descendant that combines
selective best-solution reinforcement with explicit lower and upper pheromone bounds.
The implementation supports iteration-best or best-so-far reinforcement and an
optional stagnation restart.

## Technical specifications

- Stable ID: `max-min-ant-system-stutzle-hoos-2000`
- Class: `MaxMinAntSystemOptimizer<TSolution,TComponent,TPheromoneKey,TEnumerator>`
- Family: Swarm intelligence / constructive
- Public since: v0.45.0
- Scientific component page: @subpage advanced_ant_colony_optimization

## Complexity

Construction has the same candidate-scan complexity as Ant System. Global evaporation
is lazy O(1); only the selected best path is explicitly reinforced.

## Applicability

Finite constructive discrete search spaces with typed feasible components, heuristic
information and pheromone keys.

## Detailed operation

Each colony uses the shared Ant System proportional construction engine. After all ants
are evaluated, trails evaporate lazily. Exactly one best source (iteration-best or
best-so-far) deposits pheromone, and every materialized trail is clamped to the configured
\f$[\tau_{\min},\tau_{\max}]\f$ interval. Optional stagnation restart resets sparse trail
memory to \f$\tau_0\f$.

## Parameters

`AntCount`, `MaximumIterations`, `Alpha`, `Beta`, `EvaporationRate`,
`InitialPheromone`, `MinimumPheromone`, `MaximumPheromone`, `BestSource`,
`RestartAfterNonImprovingIterations`, and `MaximumConstructionSteps`.

## API example

```csharp
var mmas =
    new MaxMinAntSystemOptimizer<MySolution, MyComponent, MyKey, MyEnumerator>(
        constructionModel,
        depositPolicy);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.MaxMinAntSystem,
    () => mmas,
    replace: true);

var parameters =
    new MaxMinAntSystemParameters
    {
        AntCount = 20,
        MaximumIterations = 200,
        EvaporationRate = 0.2,
        InitialPheromone = 1.0,
        MinimumPheromone = 0.01,
        MaximumPheromone = 1.0,
        BestSource = MaxMinAntSystemBestSource.BestSoFar
    };
```

## Stable factory ID

`max-min-ant-system-stutzle-hoos-2000`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

\f[
\begin{aligned}
\tau_e'&=(1-\rho)\tau_e+\Delta\tau_e^{best},\\
\tau_e&\leftarrow
\min\{\tau_{\max},\max\{\tau_{\min},\tau_e'\}\},\\
s^{best}&\in\{s_t^{ib},s^{gb}\}.
\end{aligned}
\f]

### Assumptions

The selected deposit policy must produce a finite nonnegative reinforcement value.
The initial trail lies inside the configured bounds and the construction model remains
feasible until completion.

### Convergence conditions

The bounds prevent trail values from collapsing to zero or diverging, preserving
continued sampling support under stochastic proportional construction. No universal
finite-time optimum guarantee is asserted.

### Scientific references

Stutzle & Hoos (2000), *MAX-MIN Ant System*, Future Generation Computer Systems
16(8), 889-914. DOI: `10.1016/S0167-739X(00)00043-1`.
