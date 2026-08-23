@page large_neighborhood_search_shaw_1998 Large Neighborhood Search - Shaw 1998

# Large Neighborhood Search - Shaw 1998

## General description

Large Neighborhood Search (LNS) was introduced by Paul Shaw as a local-search method that
explores a large implicitly defined neighborhood by destroying part of the incumbent solution
and reconstructing a complete candidate. The original vehicle-routing implementation removes
related customer visits and reinserts them with constraint-based search.

MetaheuristicsPlatform exposes the representation-independent destroy/repair foundation.
Problem-specific relatedness measures and exact reinsertion searches remain outside the
canonical v0.52 identity. Adaptive operator selection is implemented separately by the public
v0.53 ALNS identity `adaptive-large-neighborhood-search-ropke-pisinger-2006`.

## Technical specifications

- Stable ID: `large-neighborhood-search-shaw-1998`
- Class: `LargeNeighborhoodSearchOptimizer<TSolution,TRemoved>`
- Parameters: `LargeNeighborhoodSearchParameters`
- Family: Trajectory-based methods
- Search spaces: binary, integer, permutation, combinatorial and mixed
- Public since: v0.52.0
- Primary DOI: `10.1007/3-540-49481-2_30`
- Supporting overview DOI: `10.1007/978-1-4419-1665-5_13`

## Complexity

For destruction size \f$q\f$, one complete iteration costs

\f[
O(C_D(q)+C_R(q)+C_f+C_A),
\f]

where \f$C_D\f$ is domain destruction cost, \f$C_R\f$ repair cost, \f$C_f\f$ objective
evaluation and \f$C_A\f$ acceptance cost. Core storage is one owned candidate clone plus the
domain-owned removed-component representation.

## Applicability

LNS is appropriate when a problem admits meaningful partial destruction and powerful
reconstruction of a complete feasible solution. Typical applications are routing, scheduling
and other structured combinatorial problems. The generic core does not assume a specific
representation of removed components.

## Detailed operation

1. Generate and evaluate one initial complete solution.
2. Clone the current incumbent.
3. Apply the configured destroy operator with the configured destruction size.
4. Pass the resulting partial state and removed-component token to the repair operator.
5. Evaluate only after repair has restored a complete candidate.
6. Apply the configured incumbent-acceptance policy.
7. Record one completed iteration only after the acceptance decision.

The default constructor uses strict-improvement acceptance, matching a canonical local-search
interpretation. Alternative acceptance policies can be composed explicitly and are reused
by the separate public ALNS layer introduced in v0.53.

If generic stopping fires after the repaired candidate evaluation, that visited candidate can
still update the common best-so-far state, but the incomplete destroy-repair-accept cycle is not
counted as a completed LNS iteration.

## Parameters

- `DestructionSize`: positive domain-defined intensity supplied to the destroy operator.
- `MaximumIterations`: positive cap on complete destroy-repair-accept cycles.

The generic engine does not interpret `DestructionSize` as a percentage or a universal number
of variables. Its exact semantics belong to the domain destroy operator.

## API example

```csharp
var algorithm =
    new LargeNeighborhoodSearchOptimizer<MySolution,RemovedSet>(
        initialSolutionGenerator,
        destroyOperator,
        repairOperator);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.LargeNeighborhoodSearch,
    () => algorithm,
    replace: true);

OptimizationResult<MySolution> result =
    algorithm.Optimize(
        problem,
        new LargeNeighborhoodSearchParameters
        {
            DestructionSize = 10,
            MaximumIterations = 500
        },
        solutionCloner,
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`large-neighborhood-search-shaw-1998`

## Mathematical details

### Problem formulation

\f[
\operatorname*{opt}_{x\in\mathcal X} f(x),
\f]

with problem-defined feasibility contained in \f$\mathcal X\f$.

### Update equations / iterations

Let \f$D_q\f$ destroy an owned incumbent clone and return partial information, \f$R\f$ repair
it to a complete candidate, and \f$A\f$ denote incumbent acceptance:

\f[
\begin{aligned}
(p_k,\rho_k)
&=D_q(x_k),\\
y_k
&=R(p_k,\rho_k),\\
x_{k+1}
&=
\begin{cases}
y_k,&A(x_k,y_k)=1,\\
x_k,&\text{otherwise}.
\end{cases}
\end{aligned}
\f]

For the built-in strict-improvement policy,

\f[
A(x,y)=\mathbf 1[y\prec_f x],
\f]

where \f$\prec_f\f$ follows the configured minimization or maximization sense.

### Assumptions

The destroy operator receives an owned clone and may create a domain-specific partial state.
The repair operator must restore a complete evaluable solution before the common evaluator is
called. Objective values must be finite. The cloner must preserve independent ownership for
mutable solution representations.

### Convergence conditions

The library makes no universal finite-time global-optimum claim for generic LNS. Under strict
improvement on a finite search space, accepted incumbent objective values are monotone and
the run cannot accept an infinite strictly improving cycle. Reaching a global optimum
requires additional problem-specific assumptions about destroy/repair reachability and search
coverage.

### Scientific references

Shaw (1998), *Using Constraint Programming and Local Search Methods to Solve Vehicle Routing
Problems*, Principles and Practice of Constraint Programming - CP98, LNCS 1520, 417-431.
DOI: `10.1007/3-540-49481-2_30`.

Pisinger & Ropke (2010), *Large Neighborhood Search*, Handbook of Metaheuristics,
2nd edition, 399-419.
DOI: `10.1007/978-1-4419-1665-5_13`.

The adaptive multi-operator extension is scientifically distinct and is implemented
separately by the v0.53 ALNS identity under Ropke & Pisinger (2006),
DOI `10.1287/trsc.1050.0135`.
