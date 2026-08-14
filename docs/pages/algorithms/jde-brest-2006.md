@page jde_brest_2006 jDE — Self-Adaptive Differential Evolution

# jDE — Self-Adaptive Differential Evolution

## General description

Per-individual inherited F_i/CR_i proposals; trial parameters are committed only after strict successful selection.

## Technical specifications

- **Stable factory ID:** `jde-brest-2006`
- **Implementation class:** `SelfAdaptiveDifferentialEvolutionOptimizer`
- **Family:** Evolutionary methods
- **Source:** `src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/SelfAdaptiveDifferentialEvolutionOptimizer.cs`
- **Runtime creation:** direct typed factory creation

## Complexity

- **Time:** O(ND) per generation plus objective-evaluation cost
- **Space:** O(ND + N)

## Applicability

Continuous bounded search spaces; canonical DE/rand/1/bin self-adaptation

## Detailed operation

The implementation follows the cited scientific method while preserving the platform invariants: deterministic random streams where applicable, explicit ownership of mutable state, common stopping/callback lifecycle, and no avoidable hot-loop allocation.

## Parameters

Generic: seed, stopping criteria, callbacks, cancellation, evaluation execution. Specific parameters are exposed by the algorithm parameter object and documented by the generated API reference.

## API example


```csharp
var algorithm =
    MetaheuristicFactory.Create<SelfAdaptiveDifferentialEvolutionOptimizer>(
        "jde-brest-2006");
```


## Stable factory ID

```text
jde-brest-2006
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
F_i'=\begin{cases}F_\ell+u_1F_u,&u_2<\tau_1\\F_i,&\text{otherwise}\end{cases},\qquad CR_i'=\begin{cases}u_3,&u_4<\tau_2\\CR_i,&\text{otherwise}\end{cases}
\f]

### Assumptions

Canonical DE/rand/1/bin donor feasibility and a continuous bounded decision vector.

### Convergence conditions

Self-adaptation changes the stochastic parameter process but does not introduce a universal finite-time convergence guarantee.

### Scientific references

Brest et al. (2006), Self-Adapting Control Parameters in Differential Evolution: A Comparative Study on Numerical Benchmark Problems, IEEE TEC 10(6), 646–657

DOI: `10.1109/TEVC.2006.872133`

## Scientific references

- Brest et al. (2006), Self-Adapting Control Parameters in Differential Evolution: A Comparative Study on Numerical Benchmark Problems, IEEE TEC 10(6), 646–657
- DOI: `10.1109/TEVC.2006.872133`
