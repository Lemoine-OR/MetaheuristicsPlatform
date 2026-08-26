@page crow_search_algorithm_askarzadeh_2016 Crow Search Algorithm

# Crow Search Algorithm

## General description

Crow Search Algorithm (`CSA`) is the scientific identity introduced by Askarzadeh in 2016.
This page documents the canonical bounded-continuous platform implementation corresponding to that publication,
without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `crow-search-algorithm-askarzadeh-2016`
- Class: `CrowSearchOptimizer`
- Parameters: `CrowSearchParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.81.0
- Primary DOI: `10.1016/j.compstruc.2016.03.001`

## Complexity

O(ND) per iteration plus N objective evaluations. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free optimization using personal hiding-place memory, crow following, flight length and awareness-controlled random relocation.

## Detailed operation

Canonical CSA with each crow following another crow memory when the target is unaware, random relocation when it is aware, and personal-memory greedy update; paper benchmark controls fl=2 and AP=0.1 are the defaults.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `CrowSearchParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<CrowSearchOptimizer>(
        MetaheuristicAlgorithmIds.CrowSearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new CrowSearchParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`crow-search-algorithm-askarzadeh-2016`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}x_i^{t+1}&=\begin{cases}x_i^t+r_i^t\,\mathrm{fl}\,(m_j^t-x_i^t),&q_j^t\ge \mathrm{AP},\\U(\mathcal X),&q_j^t<\mathrm{AP},\end{cases}\quad j\ne i,\\m_i^{t+1}&=\begin{cases}x_i^{t+1},&f(x_i^{t+1})\prec f(m_i^t),\\m_i^t,&\text{otherwise}.\end{cases}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values and at least two crows. Flight length and awareness probability are fixed per run; random relocation samples uniformly from the bounded search space.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; awareness probability controls diversification while flight length controls the scale of directed movement toward remembered hiding places.

### Scientific references

Askarzadeh (2016), A novel metaheuristic method for solving constrained engineering optimization problems: Crow search algorithm, Computers & Structures 169, 1-12.
DOI: `10.1016/j.compstruc.2016.03.001`.
