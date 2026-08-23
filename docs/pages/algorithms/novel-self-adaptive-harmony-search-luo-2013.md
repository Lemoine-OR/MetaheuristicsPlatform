@page novel_self_adaptive_harmony_search_luo_2013 Novel Self-Adaptive Harmony Search

# Novel Self-Adaptive Harmony Search

## General description

**NSHS** reproduces the scientific identity introduced by K. Luo (2013).
NSHS removes PAR, computes HMCR=1-1/(D+1), switches behavior at fitness standard deviation 0.0001, and uses iteration-dependent coordinate perturbation.

This page distinguishes the publication mechanism from platform-only defensive completions.
No other Harmony Search identity is silently mixed into this implementation.

## Technical specifications

- Stable ID: `novel-self-adaptive-harmony-search-luo-2013`
- Runtime type: `NovelSelfAdaptiveHarmonySearchOptimizer`
- Search space: bounded continuous
- Scientific identity: NSHS
- DOI: `10.1155/2013/653749`

## Complexity

Harmony Memory storage is O(H*D). One improvisation uses O(D+H) work in the ordinary case,
plus the objective evaluation; variant-specific population statistics or adaptation may add O(H*D).

## Applicability

Bounded continuous derivative-free minimization.

## Detailed operation

NSHS dimension-derived HMCR, no PAR, fitness-standard-deviation threshold 0.0001, best-to-worst narrow randomization under low diversity and the paper's two perturbation branches.

## Parameters

The public parameters expose only quantities required by the publication or explicit platform
termination/defensive controls. Defaults follow the reported experimental settings when uniquely
specified; any platform completion is named in the source comments and this documentation.

## API example

The paper's update/replacement equations are reproduced as minimization-only.

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
    new NovelSelfAdaptiveHarmonySearchOptimizer().Optimize(
        problem,
        new NovelSelfAdaptiveHarmonySearchParameters(),
        new ArraySolutionCloner<double>(),
        new MaxEvaluationsStoppingCriterion(1000),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`novel-self-adaptive-harmony-search-luo-2013`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}HMCR&=1-\frac{1}{D+1},\quad PAR\ \text{removed},\\ \delta_i&=\frac{U_i-L_i}{100}(1-t/NI)U(-1,1)\ \text{if }f_{std}>10^{-4}\ \text{else }10^{-4}U(-1,1)\end{aligned}
\f]

### Assumptions

Bounded continuous minimization, finite objectives and positive dimensionality.

### Convergence conditions

The paper motivates self-adaptation through diversity/intensity balance; no universal finite-time global convergence theorem is asserted.

### Scientific references

K. Luo (2013), *A Novel Self-Adaptive Harmony Search Algorithm*, Journal of Applied Mathematics 2013, Article 653749.
DOI: `10.1155/2013/653749`.
