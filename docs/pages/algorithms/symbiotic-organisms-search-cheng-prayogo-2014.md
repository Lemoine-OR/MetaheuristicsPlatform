@page symbiotic_organisms_search_cheng_prayogo_2014 Symbiotic Organisms Search

# Symbiotic Organisms Search

## General description

Symbiotic Organisms Search (`SOS`) is the scientific identity introduced by Cheng and Prayogo in 2014. This page documents the canonical bounded-continuous platform implementation corresponding to that publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `symbiotic-organisms-search-cheng-prayogo-2014`
- Class: `SymbioticOrganismsSearchOptimizer`
- Parameters: `SymbioticOrganismsSearchParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.85.0
- Primary DOI: `10.1016/j.compstruc.2014.03.007`

## Complexity

O(ND) per cycle plus 4N objective evaluations. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free optimization with parameter-free mutualism, commensalism and parasitism phases.

## Detailed operation

Canonical SOS with mutualism benefit factors in {1,2}, commensalism random factor in [-1,1], parasite-vector mutation, and greedy replacement after each published interaction.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed by the platform continuous search space before objective evaluation.

## Parameters

The public parameter object `SymbioticOrganismsSearchParameters` exposes only controls used by the canonical scientific mechanism. Validation rejects controls or objective domains that would silently alter the published equations.

## Stable factory ID

`symbiotic-organisms-search-cheng-prayogo-2014`

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<SymbioticOrganismsSearchOptimizer>(
        MetaheuristicAlgorithmIds.SymbioticOrganismsSearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new SymbioticOrganismsSearchParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}MV&=(X_i+X_j)/2,\quad BF_1,BF_2\in\{1,2\},\\X_i^\prime&=X_i+r_1(X_{best}-MV\,BF_1),\quad X_j^\prime=X_j+r_2(X_{best}-MV\,BF_2),\\X_i^\prime&=X_i+r(X_{best}-X_j)\quad\text{(commensalism)}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values and at least two organisms. SOS exposes no algorithm-specific tuning coefficient beyond generic population size and iteration budget.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the three canonical symbiosis phases combine directed exploitation and stochastic replacement without algorithm-specific coefficients.

### Scientific references

Cheng and Prayogo (2014), Symbiotic Organisms Search: A new metaheuristic optimization algorithm, Computers & Structures 139, 98-112. DOI: `10.1016/j.compstruc.2014.03.007`.
