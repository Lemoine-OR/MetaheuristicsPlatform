@page sms_emoa_beume_naujoks_emmerich_2007 SMS-EMOA

# SMS-EMOA

## General description

SMS-EMOA (`SmsEmoa`) is the public scientific identity associated with
Beume, Naujoks & Emmerich (2007), SMS-EMOA: Multiobjective Selection Based on Dominated Hypervolume, European Journal of Operational Research 181(3), 1653-1669. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `sms-emoa-beume-naujoks-emmerich-2007`
- Class: `SmsEmoaOptimizer`
- Parameters: `SmsEmoaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.104.0
- Primary DOI: `10.1016/j.ejor.2006.08.008`

## Complexity

Steady-state nondominated sorting plus dominated-hypervolume contribution selection. Memory usage is O(N(D+M)) plus hypervolume recursion.

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Steady-state environmental selection removes the minimum dominated-hypervolume contributor from the worst front. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`SmsEmoaParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.SmsEmoa;
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

SmsEmoaOptimizer algorithm =
    MetaheuristicFactory.Create<SmsEmoaOptimizer>(
        MetaheuristicAlgorithmIds.SmsEmoa);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new SmsEmoaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`sms-emoa-beume-naujoks-emmerich-2007`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\Delta S(x)&=HV(F_\ell)-HV(F_\ell\setminus\{x\}),\\x^-&=\arg\min_{x\in F_\ell}\Delta S(x),\qquad P_{t+1}=(P_t\cup\{y\})\setminus\{x^-\}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Beume, Naujoks & Emmerich (2007), SMS-EMOA: Multiobjective Selection Based on Dominated Hypervolume, European Journal of Operational Research 181(3), 1653-1669. DOI: `10.1016/j.ejor.2006.08.008`.
