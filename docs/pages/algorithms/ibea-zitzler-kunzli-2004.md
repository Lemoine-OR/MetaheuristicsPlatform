@page ibea_zitzler_kunzli_2004 Indicator-Based Evolutionary Algorithm

# Indicator-Based Evolutionary Algorithm

## General description

Indicator-Based Evolutionary Algorithm (`Ibea`) is the public scientific identity associated with
Zitzler & Kunzli (2004), Indicator-Based Selection in Multiobjective Search, PPSN VIII, LNCS 3242, 832-842. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `ibea-zitzler-kunzli-2004`
- Class: `IbeaOptimizer`
- Parameters: `IbeaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.99.0
- Primary DOI: `10.1007/978-3-540-30217-9_84`

## Complexity

O(MN^2) binary-indicator fitness per environmental selection. Memory usage is O(MN).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Binary additive-epsilon indicator fitness directly drives environmental selection. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`IbeaParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Ibea;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem =
    new ContinuousMultiobjectiveOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(
            4,
            0.0,
            1.0),
        new[]
        {
            OptimizationSense.Minimize,
            OptimizationSense.Minimize
        },
        static (
            ReadOnlySpan<double> x,
            Span<double> f) =>
        {
            f[0] = x[0];
            f[1] =
                1.0 -
                Math.Sqrt(x[0]) +
                x[1] +
                x[2] +
                x[3];
        });

IbeaOptimizer algorithm =
    MetaheuristicFactory.Create<IbeaOptimizer>(
        MetaheuristicAlgorithmIds.Ibea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new IbeaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`ibea-zitzler-kunzli-2004`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}F(x)&=\sum_{y\ne x}\exp\!\left(-I_{\epsilon+}(y,x)/\kappa\right),\\P_{t+1}&=\operatorname{EnvironmentalSelect}_{I_{\epsilon+}}(P_t\cup Q_t,N).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Zitzler & Kunzli (2004), Indicator-Based Selection in Multiobjective Search, PPSN VIII, LNCS 3242, 832-842. DOI: `10.1007/978-3-540-30217-9_84`.
