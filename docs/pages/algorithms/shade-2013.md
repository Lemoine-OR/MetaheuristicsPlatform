@page shade_2013 SHADE

# SHADE

## General description

Historical memories M_F/M_CR, random memory slot per target, improvement-weighted success learning and external archive.

## Technical specifications

- **Stable factory ID:** `shade-2013`
- **Implementation class:** `ShadeOptimizer`
- **Family:** Evolutionary methods
- **Source:** `src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/ShadeOptimizer.cs`
- **Runtime creation:** direct typed factory creation

## Complexity

- **Time:** O(ND + N log N) per generation plus objective-evaluation cost
- **Space:** O(ND + H)

## Applicability

Continuous bounded search spaces

## Detailed operation

The implementation follows the cited scientific method while preserving the platform invariants: deterministic random streams where applicable, explicit ownership of mutable state, common stopping/callback lifecycle, and no avoidable hot-loop allocation.

## Parameters

Generic: seed, stopping criteria, callbacks, cancellation, evaluation execution. Specific parameters are exposed by the algorithm parameter object and documented by the generated API reference.

## API example


```csharp
var algorithm =
    MetaheuristicFactory.Create<ShadeOptimizer>(
        "shade-2013");
```


## Stable factory ID

```text
shade-2013
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
w_k=\frac{\Delta f_k}{\sum_j\Delta f_j},\qquad M_F[h]\leftarrow\frac{\sum_k w_kF_k^2}{\sum_k w_kF_k},\qquad M_{CR}[h]\leftarrow\sum_k w_kCR_k
\f]

### Assumptions

Strict improvements define the success set; no-success generations leave historical memory unchanged.

### Convergence conditions

SHADE adapts sampling distributions from successful history but does not supply a universal deterministic convergence rate.

### Scientific references

Tanabe & Fukunaga (2013), Success-History Based Parameter Adaptation for Differential Evolution, IEEE CEC, 71–78

DOI: `10.1109/CEC.2013.6557555`

## Scientific references

- Tanabe & Fukunaga (2013), Success-History Based Parameter Adaptation for Differential Evolution, IEEE CEC, 71–78
- DOI: `10.1109/CEC.2013.6557555`
