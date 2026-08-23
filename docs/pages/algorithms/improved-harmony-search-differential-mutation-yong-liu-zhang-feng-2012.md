@page improved_harmony_search_differential_mutation_yong_liu_zhang_feng_2012 Improved Harmony Search Based on Differential Mutation Operator

# Improved Harmony Search Based on Differential Mutation Operator

## General description

**IHSDE** reproduces the scientific identity introduced by L. Yong, S. Liu, J. Zhang, Q. Feng (2012).
IHSDE uses differential mutation inside memory consideration and samples F uniformly from [0.6,1], the defining delta from the earlier differential-HS construction.

This page distinguishes the publication mechanism from platform-only defensive completions.
No other Harmony Search identity is silently mixed into this implementation.

## Technical specifications

- Stable ID: `improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012`
- Runtime type: `ImprovedHarmonySearchDifferentialMutationOptimizer`
- Search space: bounded continuous
- Scientific identity: IHSDE
- DOI: `10.1155/2012/147950`

## Complexity

Harmony Memory storage is O(H*D). One improvisation uses O(D+H) work in the ordinary case,
plus the objective evaluation; variant-specific population statistics or adaptation may add O(H*D).

## Applicability

Bounded continuous derivative-free minimization.

## Detailed operation

IHSDE memory branch chooses j,r1,r2 distinct and generates x'_i=x^j_i+F(x^r1_i-x^r2_i), F~U[0.6,1]; random branch remains uniform over bounds.

## Parameters

The public parameters expose only quantities required by the publication or explicit platform
termination/defensive controls. Defaults follow the reported experimental settings when uniquely
specified; any platform completion is named in the source comments and this documentation.

## API example

The published algorithm and benchmark formulation are treated as minimization-only; no silent objective-sense generalization is introduced.

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
    new ImprovedHarmonySearchDifferentialMutationOptimizer().Optimize(
        problem,
        new ImprovedHarmonySearchDifferentialMutationParameters(),
        new ArraySolutionCloner<double>(),
        new MaxEvaluationsStoppingCriterion(1000),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}x_i'&=x_i^j+F(x_i^{r1}-x_i^{r2}),\\ F&\sim U(0.6,1),\quad j\ne r1\ne r2\end{aligned}
\f]

### Assumptions

Bounded continuous minimization, HMS>=3, finite objective values.

### Convergence conditions

The paper derives population mean/variance behavior and reports empirical convergence; no universal finite-time global convergence theorem is claimed.

### Scientific references

L. Yong, S. Liu, J. Zhang, Q. Feng (2012), *Theoretical and Empirical Analyses of an Improved Harmony Search Algorithm Based on Differential Mutation Operator*, Journal of Applied Mathematics 2012, Article 147950.
DOI: `10.1155/2012/147950`.
