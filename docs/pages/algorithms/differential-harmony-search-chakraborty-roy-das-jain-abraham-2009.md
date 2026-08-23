@page differential_harmony_search_chakraborty_roy_das_jain_abraham_2009 Differential Harmony Search

# Differential Harmony Search

## General description

**DHS** reproduces the scientific identity introduced by P. Chakraborty, G. G. Roy, S. Das, D. Jain, A. Abraham (2009).
Primary text: DHS forms an intermediate harmony by memory/random selection and replaces classical pitch adjustment with Eq. (5), z=y+F(x_r1-x_r2), F~U[0,1].

This page distinguishes the publication mechanism from platform-only defensive completions.
No other Harmony Search identity is silently mixed into this implementation.

## Technical specifications

- Stable ID: `differential-harmony-search-chakraborty-roy-das-jain-abraham-2009`
- Runtime type: `DifferentialHarmonySearchOptimizer`
- Search space: bounded continuous
- Scientific identity: DHS
- DOI: `10.3233/FI-2009-157`

## Complexity

Harmony Memory storage is O(H*D). One improvisation uses O(D+H) work in the ordinary case,
plus the objective evaluation; variant-specific population statistics or adaptation may add O(H*D).

## Applicability

Bounded continuous derivative-free minimization or maximization.

## Detailed operation

Exact DHS structure: HS memory/random intermediate vector followed by DE/rand/1-style differential mutation with one F~U[0,1] and two distinct HM members; strict objective-sense replacement.

## Parameters

The public parameters expose only quantities required by the publication or explicit platform
termination/defensive controls. Defaults follow the reported experimental settings when uniquely
specified; any platform completion is named in the source comments and this documentation.

## API example

The publication explicitly formulates minimization or maximization; the platform therefore preserves both objective senses.

```csharp
using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

var problem =
    new ContinuousOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(5, -5.0, 5.0),
        OptimizationSense.Minimize,
        x =>
        {
            double sum = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                sum += x[i] * x[i];
            }
            return sum;
        });

var result =
    new DifferentialHarmonySearchOptimizer().Optimize(
        problem,
        new DifferentialHarmonySearchParameters(),
        new ArraySolutionCloner<double>(),
        new MaxEvaluationsStoppingCriterion(1000),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`differential-harmony-search-chakraborty-roy-das-jain-abraham-2009`

## Mathematical details

### Problem formulation

\f[
\operatorname{opt}_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}y_i&\sim HM_i\ \text{with probability }HMCR\ \text{else }U(L_i,U_i),\\ z&=y+F(x_{r1}-x_{r2}),\quad F\sim U(0,1),\ r1\ne r2\end{aligned}
\f]

### Assumptions

Bounded continuous search, at least two harmonies, finite objective values.

### Convergence conditions

No universal finite-time global convergence theorem is claimed in the paper.

### Scientific references

P. Chakraborty, G. G. Roy, S. Das, D. Jain, A. Abraham (2009), *An Improved Harmony Search Algorithm with Differential Mutation Operator*, Fundamenta Informaticae 95(4), 401-426.
DOI: `10.3233/FI-2009-157`.

Bibliographic note: the official publisher metadata assigns DOI `10.3233/FI-2009-157`; an author-hosted PDF header displays `10.3233/FI-2009-181`. This implementation uses the official publisher DOI and does not silently hide that discrepancy.
