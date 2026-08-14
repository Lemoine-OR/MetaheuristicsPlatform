@page lshade_2014 L-SHADE

# L-SHADE

## General description

SHADE 1.1 success-history semantics with linear population-size reduction, fixed physical capacity and shrinking active prefix.

## Technical specifications

- **Stable factory ID:** `lshade-2014`
- **Implementation class:** `LShadeOptimizer`
- **Family:** Evolutionary methods
- **Source:** `src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/LShadeOptimizer.cs`
- **Runtime creation:** direct typed factory creation

## Complexity

- **Time:** O(N_kD + N_k log N_k) at generation k plus objective-evaluation cost
- **Space:** O(N_init D + H)

## Applicability

Continuous bounded search spaces with an evaluation budget driving LPSR

## Detailed operation

The implementation follows the cited scientific method while preserving the platform invariants: deterministic random streams where applicable, explicit ownership of mutable state, common stopping/callback lifecycle, and no avoidable hot-loop allocation.

## Parameters

Generic: seed, stopping criteria, callbacks, cancellation, evaluation execution. Specific parameters are exposed by the algorithm parameter object and documented by the generated API reference.

## API example


```csharp
var algorithm =
    MetaheuristicFactory.Create<LShadeOptimizer>(
        "lshade-2014");
```


## Stable factory ID

```text
lshade-2014
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
N(\mathrm{NFE})=\operatorname{round}\!\left(\frac{N_{\min}-N_{\rm init}}{\mathrm{MAX\_NFE}}\,\mathrm{NFE}+N_{\rm init}\right)
\f]

### Assumptions

An explicit evaluation budget defines the population-reduction schedule; the active population never falls below four.

### Convergence conditions

LPSR is a resource-allocation mechanism layered on SHADE; the implementation makes no universal finite-time convergence claim.

### Scientific references

Tanabe & Fukunaga (2014), Improving the Search Performance of SHADE Using Linear Population Size Reduction, IEEE CEC, 1658–1665

DOI: `10.1109/CEC.2014.6900380`

## Scientific references

- Tanabe & Fukunaga (2014), Improving the Search Performance of SHADE Using Linear Population Size Reduction, IEEE CEC, 1658–1665
- DOI: `10.1109/CEC.2014.6900380`
