# Variable Neighborhood Descent

## General description

Variable Neighborhood Descent (VND) systematically changes the neighborhood used by a local-descent method. The neighborhoods are ordered. If the current neighborhood produces a strict improvement, VND returns to the first neighborhood; otherwise it advances to the next neighborhood. The procedure terminates when every configured neighborhood fails to improve the incumbent.

## Technical specifications

- Stable algorithm ID: `variable-neighborhood-descent`.
- Public optimizer: `VariableNeighborhoodDescentOptimizer<TSolution>`.
- Reusable procedure: `VariableNeighborhoodDescentProcedure<TSolution>`.
- Composition requirements: initial-solution generator plus an ordered list of `ILocalSearchProcedure<TSolution>`.
- The constituent local-search procedures retain their own exact-delta and reversible full-evaluation fast paths.
- Best-so-far, stopping, callbacks, cancellation and evaluation accounting remain owned by the common `OptimizationContext`.

## Complexity

Let `K` be the number of configured neighborhoods and let `C_k(x)` be the cost of fully applying the local-search procedure associated with neighborhood `k` from solution `x`. One complete non-improving sweep costs

\[
O\!\left(\sum_{k=1}^{K} C_k(x)\right).
\]

Whenever a strict improvement occurs, the sequence restarts at neighborhood 1. Therefore total runtime is problem dependent and equals the accumulated costs of all local-search invocations until a complete non-improving sweep or a stopping criterion is reached.

Memory is

\[
O(|solution| + \max_k W_k),
\]

where `W_k` is the workspace of the active local-search procedure.

## Applicability

VND is applicable whenever several meaningful neighborhood structures are available for the same solution representation. The generic implementation supports continuous, binary, integer, permutation, combinatorial and mixed representations through injected local-search procedures.

## Detailed operation

1. Generate and evaluate an initial solution.
2. Set neighborhood index `k = 1`.
3. Apply the local-search procedure associated with `N_k`.
4. If the resulting solution strictly improves the incumbent objective, set `k = 1`.
5. Otherwise increment `k`.
6. Stop when all neighborhoods have been explored without improvement, a common stopping criterion fires, or the configured restart safety cap is reached.

## Parameters

`VariableNeighborhoodDescentParameters` exposes:

- `MaximumNeighborhoodRestarts`: safety cap on strict-improvement resets to the first neighborhood; default `10000`.

Neighborhood order and the behavior of each local search are explicit composition choices rather than hidden scalar parameters.

## API example

```csharp
var n1 = new MoveLocalSearchProcedure<MySolution, Move1, Undo1, Enumerator1>(
    neighborhood1,
    moveOperator1,
    LocalSearchSelectionPolicy.FirstImprovement,
    deltaEvaluator1);

var n2 = new MoveLocalSearchProcedure<MySolution, Move2, Undo2, Enumerator2>(
    neighborhood2,
    moveOperator2,
    LocalSearchSelectionPolicy.BestImprovement,
    deltaEvaluator2);

var vnd = new VariableNeighborhoodDescentOptimizer<MySolution>(
    initialSolutionGenerator,
    new ILocalSearchProcedure<MySolution>[] { n1, n2 });

var result = vnd.Optimize(
    problem,
    new VariableNeighborhoodDescentParameters(),
    solutionCloner,
    stoppingCriterion,
    options);
```

## Stable factory ID

`variable-neighborhood-descent`

The method is catalogued as a composition because the neighborhood-specific local-search procedures are representation dependent.

## Mathematical details

### Problem formulation

For

\[
\min_{x\in X} f(x),
\]

let `N_1,\ldots,N_K` be an ordered set of neighborhood structures and let `L_k(x)` denote local improvement using `N_k`.

### Update equations / iterations

Starting with `k=1`, compute

\[
x' = L_k(x).
\]

For minimization,

\[
(x,k)\leftarrow
\begin{cases}
(x',1), & f(x') < f(x),\\
(x,k+1), & \text{otherwise}.
\end{cases}
\]

For maximization the strict comparison is reversed.

### Assumptions

All local-search procedures must operate on the same solution representation and objective. Strict improvement must be meaningful under the problem optimization sense. Practical performance depends strongly on neighborhood ordering and complementarity.

### Convergence conditions

On a finite solution space with strictly improving accepted transitions and exact objective comparisons, VND terminates after finitely many improvements at a solution that is locally optimal with respect to every configured neighborhood, unless an external stopping criterion or restart cap stops the run earlier. No claim of global optimality is implied.

### Scientific references

- N. Mladenovic, P. Hansen (1997), *Variable neighborhood search*, Computers & Operations Research 24(11), 1097-1100. DOI: `10.1016/S0305-0548(97)00031-2`.
- P. Hansen, N. Mladenovic (2001), *Variable neighborhood search: Principles and applications*, European Journal of Operational Research 130(3), 449-467. DOI: `10.1016/S0377-2217(00)00100-4`.
