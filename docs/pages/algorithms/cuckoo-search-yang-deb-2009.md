@page cuckoo_search_yang_deb_2009 Cuckoo Search via Levy Flights

# Cuckoo Search via Levy Flights

## General description

Cuckoo Search via Levy Flights (`CS`) is the scientific identity introduced by
Yang & Deb in 2009. This page documents the canonical bounded-continuous
platform implementation corresponding to that publication, without silently mixing later
variants or hybridizations.

## Technical specifications

- Stable ID: `cuckoo-search-yang-deb-2009`
- Class: `CuckooSearchOptimizer`
- Parameters: `CuckooSearchParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.69.0
- Primary DOI: `10.1109/NABIC.2009.5393690`

## Complexity

O(D + N log N + p_a N D) per published generation plus objective-evaluation cost. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization with Levy-flight exploration and nest abandonment.

## Detailed operation

Published Yang-Deb pseudocode: one randomly chosen cuckoo Levy-flight proposal, random host-nest comparison, replacement of a p_a fraction of worst nests by new random nests, elitist retention and ranking. The Levy-flight operator is realized internally with Mantegna's symmetric beta=1.5 construction; beta is intentionally not exposed as a configurable public parameter for this canonical identity.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is
performed by the platform continuous search space. Completed-iteration accounting is kept
separate from partial iterations stopped by an evaluation or external stopping criterion.

## Parameters

The public parameter object `CuckooSearchParameters` exposes only controls used by the
canonical scientific mechanism. Validation rejects non-finite probabilities/scales and
population sizes that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<CuckooSearchOptimizer>(
        MetaheuristicAlgorithmIds.CuckooSearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new CuckooSearchParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`cuckoo-search-yang-deb-2009`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}
x_i^{t+1}&=x_i^t+\alpha L,\\
L&\sim\operatorname{Levy},\\
n_{\mathrm{abandon}}&=\left\lceil p_aN\right\rceil\end{aligned}
\f]

### Assumptions

Finite bounded continuous box; finite objective values; Levy steps use a fixed internal Mantegna beta=1.5 realization; abandoned nests are resampled from the search space.

### Convergence conditions

No universal finite-time convergence guarantee is claimed; global exploration follows the heavy-tailed Levy-flight mechanism and random abandonment.

### Scientific references

Yang & Deb (2009), Cuckoo Search via Levy Flights, NaBIC 2009, 210-214.
DOI: `10.1109/NABIC.2009.5393690`.
