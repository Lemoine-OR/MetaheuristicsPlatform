# Variable Neighborhood Search - Mladenovic-Hansen

## General description

Variable Neighborhood Search (VNS) is a metaheuristic based on systematic changes of neighborhood. The canonical basic scheme shakes the incumbent in an ordered neighborhood, applies local search to the shaken candidate, and restarts at the first shaking neighborhood whenever a strict improvement is accepted.

v0.25.0 implements the generic basic VNS framework of Mladenovic and Hansen rather than a problem-specific neighborhood family.

## Technical specifications

- Stable algorithm ID: `variable-neighborhood-search-mladenovic-hansen`.
- Public class: `VariableNeighborhoodSearchOptimizer<TSolution>`.
- Composition requirements: initial-solution generator, ordered shaking neighborhoods represented by `ISolutionPerturbation<TSolution>`, and one reusable `ILocalSearchProcedure<TSolution>`.
- A `VariableNeighborhoodDescentProcedure<TSolution>` can be injected as the local-search phase, yielding a direct VNS/VND composition.
- Every shaking step works on an owned clone of the incumbent.
- Only strict local-search improvement replaces the incumbent.
- Best-so-far remains independently protected by `OptimizationContext`.

## Complexity

Let `K` be the number of shaking neighborhoods, `C_{S,k}` the cost of shaking neighborhood `k`, `C_E` the objective-evaluation cost, and `C_L` the cost of the injected local search. A complete non-improving VNS neighborhood sweep costs

\f[
O\!\left(\sum_{k=1}^{K}(C_{S,k}+C_E+C_L)\right).
\f]

Strict improvements reset the neighborhood index, so the total runtime depends on the number and position of successful improvements. Memory is

\f[
O(|solution| + W_L),
\f]

in addition to the best-so-far snapshot maintained by the common context.

## Applicability

Basic VNS is suitable when the application can define a sequence of shaking neighborhoods with increasing or otherwise complementary reach and a compatible local-search procedure. The injected components make the framework representation independent across continuous, binary, integer, permutation, combinatorial and mixed search spaces.

## Detailed operation

1. Generate and evaluate an incumbent `x`.
2. Start with the first shaking neighborhood `N_1`.
3. Clone `x` to an owned candidate `x'`.
4. Shake `x'` using the current neighborhood.
5. Evaluate `x'`.
6. Apply the injected local-search procedure to obtain `x''`.
7. If `x''` strictly improves `x`, accept `x''` and restart at `N_1`.
8. Otherwise advance to the next shaking neighborhood.
9. After a complete non-improving sweep, begin a new VNS cycle until the common stopping criterion or `MaximumCycles` is reached.

## Parameters

`VariableNeighborhoodSearchParameters` exposes:

- `MaximumCycles`: maximum number of complete VNS cycles; default `100`.

Shaking strength is intentionally owned by each injected perturbation. The library does not reduce heterogeneous neighborhood structures to a misleading universal numeric radius.

## API example

```csharp
var vnd = new VariableNeighborhoodDescentProcedure<MySolution>(
    new ILocalSearchProcedure<MySolution>[]
    {
        localSearchN1,
        localSearchN2
    });

var vns = new VariableNeighborhoodSearchOptimizer<MySolution>(
    initialSolutionGenerator,
    new ISolutionPerturbation<MySolution>[]
    {
        shakeN1,
        shakeN2,
        shakeN3
    },
    vnd);

var result = vns.Optimize(
    problem,
    new VariableNeighborhoodSearchParameters
    {
        MaximumCycles = 200
    },
    solutionCloner,
    stoppingCriterion,
    options);
```

## Stable factory ID

`variable-neighborhood-search-mladenovic-hansen`

The method is catalogued as a composition because both the shaking neighborhoods and local-search structure are domain dependent.

## Mathematical details

### Problem formulation

For

\f[
\min_{x\in X} f(x),
\f]

let `N_1,\ldots,N_K` be the ordered shaking neighborhoods and let `L` denote the injected local-search operator.

### Update equations / iterations

At neighborhood `k`, sample

\f[
x' \in_R N_k(x),
\qquad
\widehat{x}=L(x').
\f]

For minimization,

\f[
(x,k)\leftarrow
\begin{cases}
(\widehat{x},1), & f(\widehat{x})<f(x),\\
(x,k+1), & \text{otherwise}.
\end{cases}
\f]

For maximization the strict comparison is reversed.

### Assumptions

Each shaking operator must return a valid representation or one that the objective/local-search pipeline can validly evaluate. Effective VNS requires neighborhoods that alter the search basin structure in a useful way; their ordering and strength are problem dependent.

### Convergence conditions

Basic VNS is a stochastic heuristic and does not imply a universal finite-time global convergence theorem. Under additional reachability and persistent-sampling assumptions, repeated random shaking can provide asymptotic opportunities to reach globally optimal basins, but such properties depend on the domain-specific neighborhood system. v0.25.0 therefore makes no unsupported universal convergence claim.

### Scientific references

- N. Mladenovic, P. Hansen (1997), *Variable neighborhood search*, Computers & Operations Research 24(11), 1097-1100. DOI: `10.1016/S0305-0548(97)00031-2`.
- P. Hansen, N. Mladenovic (2001), *Variable neighborhood search: Principles and applications*, European Journal of Operational Research 130(3), 449-467. DOI: `10.1016/S0377-2217(00)00100-4`.
