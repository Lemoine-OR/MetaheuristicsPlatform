@page adaptive_harmony_search_differential_evolution_zhao_li_hao_liu_yuan_2020 Adaptive Harmony Search with Differential Evolution

# Adaptive Harmony Search with Differential Evolution

## General description

**aHSDE** reproduces the scientific identity introduced by X. Zhao, R. Li, J. Hao, Z. Liu, J. Yuan (2020).
aHSDE is one paper-defined identity combining DE/best/2 pitch adjustment, periodic PAR/F learning by weighted Lehmer means, and linear Harmony Memory size reduction.

This page distinguishes the publication mechanism from platform-only defensive completions.
No other Harmony Search identity is silently mixed into this implementation.

## Technical specifications

- Stable ID: `adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020`
- Runtime type: `AdaptiveHarmonySearchDifferentialEvolutionOptimizer`
- Search space: bounded continuous
- Scientific identity: aHSDE
- DOI: `10.3390/app10082916`

## Complexity

Harmony Memory storage is O(H*D). One improvisation uses O(D+H) work in the ordinary case,
plus the objective evaluation; variant-specific population statistics or adaptation may add O(H*D).

## Applicability

Bounded continuous derivative-free minimization.

## Detailed operation

Exact paper-defined aHSDE mechanism bundle: DE/best/2 pitch adjustment, Gaussian PAR/F sampling around learned means, successful-parameter weighted Lehmer updates, and linear HMS reduction against NFE.

## Parameters

The public parameters expose only quantities required by the publication or explicit platform
termination/defensive controls. Defaults follow the reported experimental settings when uniquely
specified; any platform completion is named in the source comments and this documentation.

## API example

The three mechanisms are kept together because they define the single aHSDE identity in the publication; the minimization-specific improvement learning is not generalized.

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
    new AdaptiveHarmonySearchDifferentialEvolutionOptimizer().Optimize(
        problem,
        new AdaptiveHarmonySearchDifferentialEvolutionParameters(),
        new ArraySolutionCloner<double>(),
        new MaxEvaluationsStoppingCriterion(1000),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}x_i'&=x_i^{best}+F[(x_i^{r1}-x_i^{r2})+(x_i^{r3}-x_i^{r4})]+U(-1,1)bw,\\ HMS&=\operatorname{round}\!\left(HMS_{max}-\frac{HMS_{max}-HMS_{min}}{MAXNFE}NFE\right),\\ \mu_S&=\frac{\sum_k w_kS_k^2}{\sum_k w_kS_k}\end{aligned}
\f]

### Assumptions

Bounded continuous minimization, active HMS>=5, finite objectives and finite positive NFE budget.

### Convergence conditions

The paper evaluates cooperation of its adaptive mechanisms empirically; no universal finite-time global convergence theorem is asserted.

### Scientific references

X. Zhao, R. Li, J. Hao, Z. Liu, J. Yuan (2020), *A New Differential Mutation Based Adaptive Harmony Search Algorithm for Global Optimization*, Applied Sciences 10(8), 2916.
DOI: `10.3390/app10082916`.
