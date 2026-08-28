@page epsilon_constrained_de_takahama_sakai_iwane_2006 Epsilon-Constrained Differential Evolution

# Epsilon-Constrained Differential Evolution

## General description

Epsilon-Constrained Differential Evolution (`EpsilonConstrainedDe`) is the public scientific identity associated with Takahama, Sakai & Iwane (2006), *Solving Nonlinear Constrained Optimization Problems by the epsilon Constrained Differential Evolution*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `epsilon-constrained-de-takahama-sakai-iwane-2006`
- Class: `EpsilonConstrainedDeOptimizer`
- Parameters: `EpsilonConstrainedDeParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.125.0
- Primary DOI/permanent identifier: `10.1109/ICSMC.2006.385209`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

DE/rand/1/bin uses an epsilon-level ordering whose admissible violation threshold decreases to zero over a controlled number of generations.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`EpsilonConstrainedDeParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.EpsilonConstrainedDe;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new EpsilonConstrainedDeOptimizer().Optimize(problem, new EpsilonConstrainedDeParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`epsilon-constrained-de-takahama-sakai-iwane-2006`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}\varepsilon_t&=\varepsilon_0\left(1-\frac{t}{T_c}\right)^{c_p},\qquad x\preceq_{\varepsilon_t}y.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Takahama, Sakai & Iwane (2006), *Solving Nonlinear Constrained Optimization Problems by the epsilon Constrained Differential Evolution*, 2006 IEEE International Conference on Systems, Man and Cybernetics. DOI/permanent identifier: `10.1109/ICSMC.2006.385209`.
