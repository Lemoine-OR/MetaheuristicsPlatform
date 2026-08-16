@page grasp_path_relinking GRASP with Path Relinking

# GRASP with Path Relinking

## General description

GRASP with Path Relinking (GRASP-PR) augments randomized greedy construction and local search
with a quality/diversity elite set and target-directed intensification trajectories. Version
0.31.0 retains the canonical greedy-forward engine from v0.30.x and adds a configurable advanced
engine implementing forward, backward, back-and-forward, mixed, truncated and greedy-randomized
adaptive path-relinking strategies.

@subpage path_relinking_strategies

## Technical specifications

- Stable ID: `grasp-path-relinking`.
- Public optimizer: `GraspPathRelinkingOptimizer<TSolution>`.
- Compatibility engine: `GreedyForwardPathRelinkingProcedure<TSolution,TMove,TUndo,TEnumerator>`.
- Advanced engine: `AdvancedPathRelinkingProcedure<TSolution,TMove,TUndo,TEnumerator>`.
- Advanced capability: `IAdvancedPathRelinkingProcedure<TSolution>`.
- Direction policies: forward, backward, back-and-forward, mixed.
- Move policies: greedy, greedy-randomized adaptive RCL.
- Truncation: orthogonal `PathFraction` in \f$(0,1]\f$.
- Objective fast path: optional exact `IMoveObjectiveDeltaEvaluator<TSolution,TMove>`.
- Randomized path candidate storage: pooled arrays, not per-step `List<T>` allocation.
- Elite guides expose their stored fitness so backward/mixed paths do not duplicate objective evaluations.
- Common callbacks, probe accounting, stopping, cancellation and RNG ownership remain in `OptimizationContext`.

## Complexity

Let \f$D\f$ be the number of accepted path moves, \f$P_k\f$ the number of target-directed
candidates at step \f$k\f$, \f$C_p\f$ their objective-probe cost, \f$C_\rho\f$ the distance
cost, and \f$E\f$ the elite-set capacity. One directional path costs

\f[
O\!\left(\sum_{k=1}^{D}P_kC_p + DC_\rho\right).
\f]

Back-and-forward performs up to two such traversals. Mixed performs one candidate scan per
accepted alternating endpoint move. Elite insertion/selection remains \f$O(EC_\rho)\f$.
Greedy move selection uses constant extra candidate memory; greedy-randomized selection uses
\f$O(P_k)\f$ pooled temporary storage at the active path position.

## Applicability

The method targets finite combinatorial or mixed representations with a non-negative integral
attribute distance, restartable target-directed move enumeration, reversible move application,
compatible GRASP construction/local search and, optionally, exact objective-delta evaluation.

## Detailed operation

For each outer GRASP iteration the optimizer constructs and locally improves a solution, selects
a distinct elite guide, then dispatches path relinking. With the advanced engine, the direction,
move selection and path fraction are taken from `GraspPathRelinkingParameters`. All unvisited
candidate moves are registered as objective probes; only a selected, actually visited move can
be promoted from its existing probe evaluation to global best. Every accepted path move must
strictly decrease the relevant endpoint distance.

Backward uses the already-stored elite fitness. Back-and-forward executes backward then forward.
Mixed alternates the active endpoint while continuously shrinking the distance between the two
active endpoint states. Greedy-randomized mode builds a GRASP-style RCL from the candidate
objective values and samples uniformly. Truncation stops after the requested fraction of initial
path distance has been eliminated.

## Parameters

`GraspPathRelinkingParameters` exposes the existing GRASP/elite limits plus:

- `PathDirection`: `Forward`, `Backward`, `BackAndForward`, `Mixed`;
- `PathMoveSelection`: `Greedy` or `GreedyRandomizedAdaptive`;
- `PathFraction`: \f$(0,1]\f$, where values below one activate truncation;
- `PathRelinkingAlpha`: RCL parameter in \f$[0,1]\f$ for randomized path moves;
- `MaximumPathSteps`: safety limit for each directional or mixed traversal.

The defaults reproduce v0.30.x behavior exactly: greedy forward, full path.

## API example

```csharp
var relinking =
    new AdvancedPathRelinkingProcedure<
        MySolution,
        MyMove,
        MyUndo,
        MyPathMoveEnumerator>(
            pathNeighborhood,
            pathDistance,
            reversibleMoveOperator,
            exactDeltaEvaluator);

var algorithm =
    new GraspPathRelinkingOptimizer<MySolution>(
        graspConstruction,
        localSearch,
        relinking,
        pathDistance);

OptimizationResult<MySolution> result =
    algorithm.Optimize(
        problem,
        new GraspPathRelinkingParameters
        {
            Alpha = 0.2,
            ElitePoolSize = 10,
            MinimumEliteDistance = 1,
            PathDirection = PathRelinkingDirectionStrategy.Mixed,
            PathMoveSelection = PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive,
            PathFraction = 0.75,
            PathRelinkingAlpha = 0.2
        },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`grasp-path-relinking`

The stable algorithm identity is unchanged. This optimizer requires explicit domain composition.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X}f(x),
\qquad
x^I,x^G\in\mathcal X,
\qquad
\rho(x^I,x^G)\in\mathbb Z_{\ge0}.
\f]

At an active endpoint \f$x_k\f$ with target \f$g_k\f$,

\f[
M(x_k,g_k)=\{m:\rho(m(x_k),g_k)<\rho(x_k,g_k)\}.
\f]

### Update equations / iterations

Greedy selection uses the sense-consistent best member of \f$M(x_k,g_k)\f$. For randomized
minimization selection,

\f[
\begin{aligned}
 f_{best}&=\min_{m\in M}f(m(x_k)),\\
 f_{worst}&=\max_{m\in M}f(m(x_k)),\\
 \tau_\alpha&=f_{best}+\alpha(f_{worst}-f_{best}),\\
 RCL_\alpha&=\{m\in M:f(m(x_k))\le\tau_\alpha\},\\
 m_k&\sim U(RCL_\alpha),\\
 x_{k+1}&=m_k(x_k).
\end{aligned}
\f]

Every accepted move satisfies strict progress. Truncation with fraction \f$\theta\f$ stops once

\f[
\rho_0-\rho_k\ge\lceil\theta\rho_0\rceil.
\f]

### Assumptions

- Path distance is non-negative and zero identifies equal path-relinking attribute configurations.
- A positive remaining distance exposes at least one target-directed move.
- Every selected move strictly reduces the relevant endpoint distance.
- Reversible operators restore the exact pre-probe state.
- Optional delta evaluators are exact.
- Greedy-randomized RCL construction requires finite candidate objective values.

### Convergence conditions

Strict decrease of a non-negative integral distance gives finite full-path termination in the
absence of external caps. Truncation deliberately terminates earlier after the requested distance
fraction. These are termination properties, not global-optimality theorems. GRASP-PR retains the
usual stochastic-search qualification: no unconditional finite-time global convergence guarantee
is claimed.

### Scientific references

- Feo, T. A.; Resende, M. G. C. (1995). *Greedy Randomized Adaptive Search Procedures*.
  DOI: `10.1007/BF01096763`.
- Resende, M. G. C.; Ribeiro, C. C. (2005). *GRASP with path-relinking: Recent advances and applications*.
  DOI: `10.1007/0-387-25383-1_2`.
- Aiex, R. M.; Resende, M. G. C.; Pardalos, P. M.; Toraldo, G. (2005).
  *GRASP with Path Relinking for Three-Index Assignment*. DOI: `10.1287/ijoc.1030.0059`.
- Ribeiro, C. C.; Resende, M. G. C. (2012).
  *Path-relinking intensification methods for stochastic local search algorithms*,
  Journal of Heuristics 18(2), 193-214. DOI: `10.1007/s10732-011-9167-1`.

Evolutionary path relinking remains reviewed/deferred because it evolves an elite population over
multiple generations and therefore deserves a distinct population-level intensification contract.