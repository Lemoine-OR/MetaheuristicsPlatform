@page differential_evolution Differential Evolution

# Differential Evolution

## General description

Flat parent/trial buffers; classical mutation and crossover strategies; deterministic per-target RNG; calibrated variation and independent evaluation parallelism.

## Technical specifications

- **Stable factory ID:** `differential-evolution`
- **Implementation class:** `DifferentialEvolutionOptimizer`
- **Family:** Evolutionary methods
- **Source:** `src/MetaheuristicsPlatform/Algorithms/DE/DifferentialEvolutionOptimizer.cs`
- **Runtime creation:** direct typed factory creation

## Complexity

- **Time:** O(ND) per generation for classical mutation/crossover, plus objective-evaluation cost
- **Space:** O(ND)

## Applicability

Continuous bounded search spaces

## Detailed operation

The implementation follows the cited scientific method while preserving the platform invariants: deterministic random streams where applicable, explicit ownership of mutable state, common stopping/callback lifecycle, and no avoidable hot-loop allocation.

## Parameters

Generic: seed, stopping criteria, callbacks, cancellation, evaluation execution. Specific parameters are exposed by the algorithm parameter object and documented by the generated API reference.

## API example


```csharp
var algorithm =
    MetaheuristicFactory.Create<DifferentialEvolutionOptimizer>(
        "differential-evolution");
```


## Stable factory ID

```text
differential-evolution
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
v_i=x_{r_1}+F(x_{r_2}-x_{r_3}),\qquad u_{i,j}=\begin{cases}v_{i,j},&r_j\le CR\ \text{or}\ j=j_{\rm rand}\\x_{i,j},&\text{otherwise}\end{cases}
\f]

### Assumptions

A population of distinct donor indices is required; the represented domain is continuous and bounded by the configured boundary policy.

### Convergence conditions

The implementation does not claim a general deterministic convergence rate. DE is stochastic; convergence analyses require additional assumptions on mutation, selection and persistent exploration.

### Scientific references

Storn & Price (1997), Differential Evolution — A Simple and Efficient Heuristic for Global Optimization over Continuous Spaces, Journal of Global Optimization 11(4), 341–359

DOI: `10.1023/A:1008202821328`

## Scientific references

- Storn & Price (1997), Differential Evolution — A Simple and Efficient Heuristic for Global Optimization over Continuous Spaces, Journal of Global Optimization 11(4), 341–359
- DOI: `10.1023/A:1008202821328`
