@page ipop_cma_es_auger_hansen_2005 IPOP-CMA-ES

# IPOP-CMA-ES

## General description

IPOP-CMA-ES is the restart CMA-ES of Auger and Hansen. The first run uses the
canonical population size \f$\lambda_0\f$; each subsequent restart increases the
population geometrically. The canonical multiplier is two.

## Technical specifications

- Stable ID: `ipop-cma-es-auger-hansen-2005`
- Class: `IpopCmaEsOptimizer`
- Parameters: `RestartCmaEsParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Public since: v0.48.0
- Component ID: `cma.restart.ipop`

## Complexity

For restart \f$r\f$, the population is \f$\lambda_r=2^r\lambda_0\f$ with the
canonical multiplier. A complete generation costs
\f$O(\lambda_r n^2+n^3)\f$ internally plus objective evaluations and uses
\f$O(\lambda_r n+n^2)\f$ memory.

## Applicability

Multimodal derivative-free continuous optimization where the population size
needed to expose global structure is not known in advance.

## Detailed operation

The platform owns one `OptimizationContext` for the entire multistart run.
Every offspring evaluation across every restart therefore contributes to the
same exact global evaluation count, best-so-far state, callback stream and
stopping criterion. Restart means after the first run are sampled uniformly
from the bounded domain.

## Parameters

`InitialPopulationSize` selects \f$\lambda_0\f$; zero uses the canonical
dimension-dependent default. `PopulationMultiplier` defaults to two.
`MaximumRestarts` limits the number of restarts after the initial run.
`MaximumGenerationsPerRestart` is a local restart trigger, while the supplied
platform stopping criterion remains globally authoritative.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<IpopCmaEsOptimizer>(
        MetaheuristicAlgorithmIds.IpopCmaEs);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new RestartCmaEsParameters
        {
            MaximumRestarts = 6,
            MaximumGenerationsPerRestart = 200
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`ipop-cma-es-auger-hansen-2005`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subset\mathbb R^n}f(x).
\f]

### Update equations / iterations

\f[
\lambda_r=\rho_{\mathrm{inc}}^r\lambda_0,
\qquad
\rho_{\mathrm{inc}}=2
\quad\text{canonically}.
\f]

Each restart reinitializes the CMA distribution while preserving the common
optimization lifecycle and its global best.

### Assumptions

The objective is finite on a bounded continuous domain. The local CMA run uses
the canonical full-covariance adaptation and a positive numerical eigenvalue floor.

### Convergence conditions

No finite-time global convergence guarantee is claimed. Increasing population
size makes later runs progressively more global, but success remains
problem-dependent.

### Scientific references

Auger & Hansen (2005), *A Restart CMA Evolution Strategy with Increasing
Population Size*, IEEE Congress on Evolutionary Computation, vol. 2,
1769-1776. DOI: `10.1109/CEC.2005.1554902`.
