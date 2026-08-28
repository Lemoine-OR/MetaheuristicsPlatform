@page stochastic_ranking_es_runarsson_yao_2000 Stochastic Ranking Evolution Strategy

# Stochastic Ranking Evolution Strategy

## General description

Stochastic Ranking Evolution Strategy (`StochasticRankingEs`) is the public scientific identity associated with Runarsson & Yao (2000), *Stochastic ranking for constrained evolutionary optimization*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `stochastic-ranking-es-runarsson-yao-2000`
- Class: `StochasticRankingEsOptimizer`
- Parameters: `StochasticRankingEsParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.119.0
- Primary DOI/permanent identifier: `10.1109/4235.873238`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Stochastic ranking repeatedly orders an evolution-strategy population using objective comparison with probability P_f and violation comparison otherwise, avoiding a fixed penalty coefficient.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`StochasticRankingEsParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.StochasticRankingEs;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new StochasticRankingEsOptimizer().Optimize(problem, new StochasticRankingEsParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`stochastic-ranking-es-runarsson-yao-2000`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}\Pr[\text{objective comparison}]&=P_f,\\ &\Pr[\text{violation comparison}]=1-P_f.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Runarsson & Yao (2000), *Stochastic ranking for constrained evolutionary optimization*, IEEE Transactions on Evolutionary Computation 4(3), 284-294. DOI/permanent identifier: `10.1109/4235.873238`.
