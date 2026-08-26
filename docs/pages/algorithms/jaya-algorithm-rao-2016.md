@page jaya_algorithm_rao_2016 Jaya Algorithm

# Jaya Algorithm

## General description

Jaya Algorithm (`Jaya`) is the scientific identity introduced by Rao in 2016.
This page documents the canonical bounded-continuous platform implementation corresponding to that publication,
without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `jaya-algorithm-rao-2016`
- Class: `JayaOptimizer`
- Parameters: `JayaParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.82.0
- Primary DOI: `10.5267/j.ijiec.2015.8.004`

## Complexity

O(ND) per iteration plus N objective evaluations. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free population optimization that moves every variable toward the current best and away from the current worst without algorithm-specific control parameters.

## Detailed operation

Canonical Jaya best/worst update with per-variable r1/r2 draws, the published absolute-value terms and greedy replacement; population size and iteration budget are the only controls.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `JayaParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<JayaOptimizer>(
        MetaheuristicAlgorithmIds.Jaya);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new JayaParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`jaya-algorithm-rao-2016`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}y_{i,d}^t&=x_{i,d}^t+r_{1,i,d}^t\bigl(x_{b,d}^t-|x_{i,d}^t|\bigr)-r_{2,i,d}^t\bigl(x_{w,d}^t-|x_{i,d}^t|\bigr),\\x_i^{t+1}&=\begin{cases}y_i^t,&f(y_i^t)\prec f(x_i^t),\\x_i^t,&\text{otherwise}.\end{cases}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values and at least two candidates. The best and worst candidates are frozen while candidate updates for one iteration are generated; boundary repair is component-wise.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. Jaya has no algorithm-specific tuning coefficients: stochastic movement is defined only by the current best/worst candidates and uniform random factors.

### Scientific references

Rao (2016), Jaya: A simple and new optimization algorithm for solving constrained and unconstrained optimization problems, International Journal of Industrial Engineering Computations 7(1), 19-34.
DOI: `10.5267/j.ijiec.2015.8.004`.
