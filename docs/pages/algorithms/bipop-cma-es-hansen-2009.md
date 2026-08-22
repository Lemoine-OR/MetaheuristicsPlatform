@page bipop_cma_es_hansen_2009 BIPOP-CMA-ES

# BIPOP-CMA-ES

## General description

BIPOP-CMA-ES interlaces two restart regimes: the IPOP large-population regime
and randomized small-population runs. After the first default run, the regime
with the smaller cumulative objective-evaluation budget is selected.

## Technical specifications

- Stable ID: `bipop-cma-es-hansen-2009`
- Class: `BipopCmaEsOptimizer`
- Parameters: `RestartCmaEsParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Public since: v0.48.0
- Component ID: `cma.restart.bipop`

## Complexity

A run with population \f$\lambda\f$ costs
\f$O(\lambda n^2+n^3)\f$ internally per complete generation plus objective
evaluations and requires \f$O(\lambda n+n^2)\f$ memory. BIPOP allocates runs
between the two regimes according to their cumulative evaluation budgets.

## Applicability

Multimodal black-box optimization where both globally structured basins
(favoring large populations) and weakly structured/local basins
(favoring repeated small populations) may occur.

## Detailed operation

The initial run uses \f$\lambda_0\f$ and the default step size. Its evaluations
are accounted on the small-budget side. The first actual restart is therefore
the large/IPOP regime. Small runs randomize both population size and initial
step size according to the BIPOP equations. All runs share one
`OptimizationContext`.

## Parameters

The same `RestartCmaEsParameters` contract is used by IPOP. The canonical
large-regime multiplier is two. `MaximumRestarts` counts all restarts after
the initial run, including small BIPOP restarts.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<BipopCmaEsOptimizer>(
        MetaheuristicAlgorithmIds.BipopCmaEs);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new RestartCmaEsParameters
        {
            MaximumRestarts = 8,
            MaximumGenerationsPerRestart = 200
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`bipop-cma-es-hansen-2009`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subset\mathbb R^n}f(x).
\f]

### Update equations / iterations

For the large regime,

\f[
\lambda_{\mathrm{large}}=2^r\lambda_0,
\qquad
\sigma_{\mathrm{large}}^0=\sigma_0.
\f]

For a small restart with independent \f$U_1,U_2\sim\mathcal U[0,1]\f$,

\f[
\lambda_{\mathrm{small}}
=
\left\lfloor
\lambda_0
\left(
\frac{1}{2}
\frac{\lambda_{\mathrm{large}}}{\lambda_0}
\right)^{U_1^2}
\right\rfloor,
\qquad
\sigma_{\mathrm{small}}^0
=
\sigma_0\,10^{-2U_2}.
\f]

The next regime is the one with the smaller cumulative objective-evaluation
budget.

### Assumptions

The bounded continuous domain permits independent restart means. The same
objective and global stopping contract apply to both regimes.

### Convergence conditions

No universal finite-time global convergence guarantee is claimed. BIPOP is a
portfolio of two restart regimes designed to diversify population scales.

### Scientific references

Hansen (2009), *Benchmarking a BI-Population CMA-ES on the BBOB-2009
Function Testbed*, GECCO Companion, 2389-2396.
DOI: `10.1145/1570256.1570333`.
