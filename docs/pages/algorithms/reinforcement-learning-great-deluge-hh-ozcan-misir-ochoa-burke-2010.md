@page reinforcement_learning_great_deluge_hh_ozcan_misir_ochoa_burke_2010 Reinforcement Learning Great-Deluge Hyper-Heuristic

# Reinforcement Learning Great-Deluge Hyper-Heuristic

## General description

Reinforcement Learning Great-Deluge Hyper-Heuristic (`ReinforcementLearningGreatDelugeHyperHeuristic`) is the public scientific identity associated with
Ozcan, Misir, Ochoa & Burke (2010), *A Reinforcement Learning - Great-Deluge Hyper-Heuristic for Examination Timetabling*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010`
- Class: `ReinforcementLearningGreatDelugeHyperHeuristicOptimizer`
- Parameters: `ReinforcementLearningGreatDelugeHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.150.0
- Primary DOI/permanent identifier: `10.4018/jamc.2010102603`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

Reinforcement-learning utility values adapt low-level heuristic selection online, while Great Deluge supplies move acceptance through a decreasing water level.

## Parameters

`ReinforcementLearningGreatDelugeHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new ReinforcementLearningGreatDelugeHyperHeuristicOptimizer().Optimize(
        domain,
        new ReinforcementLearningGreatDelugeHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}u_h^{t+1}&=(1-\alpha)u_h^t+\alpha r_t,\\B_{t+1}&=B_t-\Delta B,\qquad x_{t+1}=\operatorname{GD}(x_t,h_t(x_t),B_t).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Ozcan, Misir, Ochoa & Burke (2010), *A Reinforcement Learning - Great-Deluge Hyper-Heuristic for Examination Timetabling*, International Journal of Applied Metaheuristic Computing 1(1), 39-59.
DOI/permanent identifier: `10.4018/jamc.2010102603`.
