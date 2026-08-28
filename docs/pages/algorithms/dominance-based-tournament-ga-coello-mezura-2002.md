@page dominance_based_tournament_ga_coello_mezura_2002 Dominance-Based Tournament Genetic Algorithm

# Dominance-Based Tournament Genetic Algorithm

## General description

Dominance-Based Tournament Genetic Algorithm (`DominanceTournamentGa`) is the public scientific identity associated with Coello Coello & Mezura-Montes (2002), *Constraint-handling in genetic algorithms through the use of dominance-based tournament selection*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `dominance-based-tournament-ga-coello-mezura-2002`
- Class: `DominanceTournamentGaOptimizer`
- Parameters: `DominanceTournamentGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.120.0
- Primary DOI/permanent identifier: `10.1016/S1474-0346(02)00011-3`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Penalty-free tournament selection compares objective quality and aggregate constraint violation through a dominance relation, preserving nondominated tradeoffs among infeasible candidates.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`DominanceTournamentGaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.DominanceTournamentGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new DominanceTournamentGaOptimizer().Optimize(problem, new DominanceTournamentGaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`dominance-based-tournament-ga-coello-mezura-2002`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}(f(x),V(x))\prec(f(y),V(y))&\Longrightarrow x\text{ wins the tournament}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Coello Coello & Mezura-Montes (2002), *Constraint-handling in genetic algorithms through the use of dominance-based tournament selection*, Advanced Engineering Informatics 16(3), 193-203. DOI/permanent identifier: `10.1016/S1474-0346(02)00011-3`.
