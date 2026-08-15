# Iterated Local Search — Lourenço-Martin-Stützle

## General description

Iterated Local Search (ILS) performs local search on an initial solution, perturbs the resulting local optimum, locally improves the perturbed candidate, and applies an acceptance criterion to choose the incumbent for the next cycle. v0.24.0 implements the generic framework described by Lourenço, Martin and Stützle rather than a problem-specific perturbation.

## Technical specifications

- Stable algorithm ID: `iterated-local-search-lourenco-martin-stutzle`.
- Public class: `IteratedLocalSearchOptimizer<TSolution>`.
- Composition requirements: initial-solution generator, reusable local-search procedure and domain-owned `ISolutionPerturbation<TSolution>`.
- Built-in acceptance kinds: strict improvement, improvement-or-equality, and unconditional finite-candidate acceptance.
- Candidate perturbations operate on a clone of the incumbent, so rejection restores the incumbent without an inverse perturbation contract.
- Best-so-far remains independent from incumbent acceptance and is owned by `OptimizationContext`.

## Complexity

For `K` ILS cycles,

\[
O\!\left(C_{\mathrm{LS},0}+\sum_{k=1}^{K}(C_{\mathrm{perturb},k}+C_{\mathrm{eval},k}+C_{\mathrm{LS},k})\right).
\]

Memory is `O(|solution| + W_LS)`: one incumbent, one candidate clone, the best-so-far snapshot managed by the common context, and the workspace of the composed local-search procedure.

## Applicability

ILS is suitable for solution spaces where a meaningful local search exists and the application can define a perturbation strong enough to leave a local basin while preserving exploitable structure. The framework supports continuous, binary, integer, permutation, combinatorial and mixed representations through domain-owned components.

## Detailed operation

1. Generate and evaluate an initial solution.
2. Apply local search to obtain the first incumbent local optimum.
3. Clone the incumbent.
4. Perturb the clone through `ISolutionPerturbation<TSolution>`.
5. Evaluate the perturbed candidate and apply local search again.
6. Apply `NeighborhoodAcceptanceKind` to decide whether the candidate becomes the new incumbent.
7. Preserve the global best independently through `OptimizationContext`.
8. Repeat until a common stopping criterion fires or `MaximumIterations` ILS cycles are completed.

## Parameters

`IteratedLocalSearchParameters` exposes:

- `MaximumIterations`: finite number of perturbation/local-search cycles after the initial descent (default 100);
- `Acceptance`: `ImprovingOnly`, `ImprovingOrEqual`, or `Always`.

Perturbation strength is intentionally not represented by a fake universal scalar. It belongs to the injected domain-specific perturbation implementation, where permutation exchanges, destroy/repair sizes, integer moves, continuous displacements, or other structures can be modeled correctly.

## API example

```csharp
var descent = new MoveLocalSearchProcedure<MySolution, MyMove, MyUndo, MyEnumerator>(
    neighborhood,
    moveOperator,
    LocalSearchSelectionPolicy.BestImprovement,
    deltaEvaluator);

var perturbation = new DelegateSolutionPerturbation<MySolution>(
    (ref MySolution x, IOptimizationProblem<MySolution> p, IRandomSource rng) =>
        PerturbDomainSolution(ref x, rng));

var ils = new IteratedLocalSearchOptimizer<MySolution>(
    initialSolutionGenerator,
    descent,
    perturbation);

var result = ils.Optimize(
    problem,
    new IteratedLocalSearchParameters
    {
        MaximumIterations = 250,
        Acceptance = NeighborhoodAcceptanceKind.ImprovingOnly
    },
    solutionCloner,
    stoppingCriterion,
    options);
```

## Stable factory ID

`iterated-local-search-lourenco-martin-stutzle`

The method is catalogued as a composition because the local search and perturbation are representation-dependent.

## Mathematical details

### Problem formulation

Let `L` be a local-search operator, `P` a perturbation operator and `A` an acceptance rule. Starting from `x_0`,

\[
x_0^{\star}=L(x_0),\qquad
x_{k}^{\prime}=P(x_{k-1}^{\star}),\qquad
\widehat{x}_{k}=L(x_k^{\prime}),\qquad
x_k^{\star}=A(x_{k-1}^{\star},\widehat{x}_k).
\]

The returned solution is the best solution observed over the complete run, not necessarily the final incumbent when an exploratory acceptance rule is used.

### Update equations / iterations

For minimization with strict-improvement acceptance,

\[
x_k^{\star}=\begin{cases}
\widehat{x}_k,& f(\widehat{x}_k)<f(x_{k-1}^{\star}),\\
x_{k-1}^{\star},&\text{otherwise}.
\end{cases}
\]

For maximization, the comparison is reversed. `ImprovingOrEqual` also accepts equality; `Always` accepts every non-NaN candidate and therefore allows the incumbent trajectory to worsen while best-so-far remains protected.

### Assumptions

The perturbation must produce a representation that the objective function and local search can evaluate. The solution cloner must provide an owned candidate snapshot because rejection relies on incumbent/candidate separation. Effective ILS additionally requires a perturbation/local-search interaction that can move between useful basins; this is problem dependent.

### Convergence conditions

The generic ILS framework does not imply a universal finite-time global convergence theorem. Asymptotic global convergence requires additional assumptions such as reachability/ergodicity of the induced basin-level process and an acceptance mechanism that does not permanently exclude the globally optimal basin. v0.24.0 therefore documents the mechanism without making an unsupported convergence claim.

### Scientific references

- H. R. Lourenço, O. C. Martin, T. Stützle (2003), *Iterated Local Search*, in *Handbook of Metaheuristics*, pp. 320-353. DOI: `10.1007/0-306-48056-5_11`.
- E.-G. Talbi (2009), *Metaheuristics: From Design to Implementation*, Wiley. DOI: `10.1002/9780470496916`.
