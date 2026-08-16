@page grasp_path_relinking GRASP with Path Relinking

# GRASP with Path Relinking

## General description

GRASP with Path Relinking (GRASP-PR) augments the independent multistart GRASP cycle with
memory and intensification. Locally improved GRASP solutions populate a small elite set.
A newly generated local optimum may then be used as an initiating solution and an earlier
elite solution as a guide. Path relinking explores intermediate solutions obtained by
progressively introducing attributes of the guiding solution.

The v0.30.0 implementation provides the reusable scientific foundation required by the
GRASP family: a fixed-capacity quality/diversity elite pool, an explicit domain distance,
an allocation-free target-directed move cursor and a greedy forward path-relinking engine.

## Technical specifications

- Stable ID: `grasp-path-relinking`.
- Public optimizer: `GraspPathRelinkingOptimizer<TSolution>`.
- Reusable engine: `GreedyForwardPathRelinkingProcedure<TSolution,TMove,TUndo,TEnumerator>`.
- Elite memory: `EliteSolutionPool<TSolution>`.
- Path cursor: value-type `INeighborhoodEnumerator<TMove>`.
- Objective fast path: optional `IMoveObjectiveDeltaEvaluator<TSolution,TMove>`.
- Fallback: reversible apply/evaluate/undo through `IReversibleMoveOperator`.
- Common callbacks, accounting, deterministic RNG ownership and stopping use `OptimizationContext`.
- The elite pool owns clones; guide selection uses allocation-free reservoir sampling.

## Complexity

Let \f$E\f$ be the elite-pool capacity, \f$D\f$ the number of selected path moves,
\f$P_k\f$ the number of target-directed candidate moves at path step \f$k\f$,
\f$C_\delta\f$ the exact candidate-objective cost, \f$C_f\f$ a full objective evaluation cost,
and \f$C_\rho\f$ the domain distance cost.

With an exact move evaluator, one relinking call is

\f[
O\!\left(\sum_{k=1}^{D} P_k C_\delta + D C_\rho\right).
\f]

Without an exact move evaluator it is

\f[
O\!\left(\sum_{k=1}^{D} P_k(C_{\mathrm{apply}}+C_f+C_{\mathrm{undo}})
+ D C_\rho\right).
\f]

Elite insertion and guide selection require \f$O(E C_\rho)\f$ distance work and no
temporary candidate list. Memory is \f$O(E|x|+|x|)\f$ for owned elite snapshots and
the active/best path solutions, excluding domain move-cursor workspace.

## Applicability

The method targets finite combinatorial or mixed representations for which the domain can:

1. construct complete feasible GRASP solutions;
2. provide compatible local search;
3. define a non-negative integral distance between path-relinking attributes;
4. enumerate target-directed moves toward a guiding solution;
5. apply those moves reversibly;
6. optionally evaluate their exact objective values without applying them.

Continuous interpolation is deliberately not claimed by this contract: v0.30.0 models
attribute-introduction path relinking rather than arithmetic interpolation.

## Detailed operation

For each GRASP outer iteration:

1. construct a randomized greedy feasible solution with the canonical threshold RCL;
2. evaluate it through the common runtime;
3. apply the configured local-search procedure;
4. if a distinct earlier elite exists, choose one uniformly as the guiding solution;
5. start from the new local optimum and enumerate target-directed moves;
6. probe every candidate objective and select the best candidate at the current path step;
7. apply only that selected move and verify that the distance to the guide strictly decreases;
8. retain the best solution actually visited on the path;
9. insert the resulting best path solution into the elite pool if quality/diversity rules allow it;
10. complete exactly one common outer iteration.

Unvisited path candidates are registered as probe evaluations. They can consume evaluation
budgets and emit evaluation callbacks, but they are never promoted to global best. A selected
visited candidate is promoted without double-counting its already registered probe evaluation.

## Parameters

`GraspPathRelinkingParameters` exposes:

- `MaximumIterations`: GRASP outer-iteration safety limit;
- `Alpha`: canonical threshold-RCL parameter in \f$[0,1]\f$;
- `MaximumConstructionSteps`: safety cap for one construction;
- `ElitePoolSize`: maximum number of elite snapshots;
- `MinimumEliteDistance`: required distance between distinct retained elites;
- `MaximumPathSteps`: safety cap for one relinking trajectory.

A minimum elite distance of one removes exact duplicates while allowing every distinct
integral path configuration.

## API example

```csharp
var relinking =
    new GreedyForwardPathRelinkingProcedure<
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
            MinimumEliteDistance = 1
        },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`grasp-path-relinking`

This optimizer requires domain composition, so the stable ID is used with the same explicit
typed registration pattern as the other generic composed algorithms.

## Mathematical details

### Problem formulation

For an optimization problem

\f[
\min_{x\in\mathcal X} f(x),
\f]

let \f$x^I\f$ be an initiating locally optimal solution, \f$x^G\f$ a guiding elite solution,
and \f$\rho(x,x^G)\in\mathbb Z_{\ge 0}\f$ the domain path distance.

At path position \f$x_k\f$, let

\f[
M(x_k,x^G)=
\{m:\rho(m(x_k),x^G)<\rho(x_k,x^G)\}
\f]

denote target-directed moves supplied by the domain.

### Update equations / iterations

Greedy forward path relinking selects

\f[
m_k^* \in
\operatorname*{arg\,min}_{m\in M(x_k,x^G)}
f(m(x_k))
\f]

for minimization, with the optimization-sense-consistent maximizing form for maximization,
then updates

\f[
x_{k+1}=m_k^*(x_k).
\f]

The implementation requires the strict progress invariant

\f[
\rho(x_{k+1},x^G)<\rho(x_k,x^G).
\f]

The elite pool stores at most \f$E\f$ owned solutions. Exact duplicates are merged by quality.
When full, a candidate can replace the current worst elite only when it is strictly better
and remains at least `MinimumEliteDistance` from every surviving elite.

### Assumptions

- The path distance is non-negative and returns zero for solutions with identical
  path-relinking attributes.
- The target-directed neighborhood is nonempty whenever the remaining distance is positive.
- The selected path move strictly decreases the remaining distance.
- Reversible move operators restore the exact pre-move solution.
- Optional delta evaluators return the exact objective of the corresponding moved solution.
- Construction, local search, distance and relinking contracts operate on one compatible
  solution representation.

### Convergence conditions

For a finite path distance and strict decrease at every accepted path step, an uncapped
relinking trajectory reaches the guiding attribute configuration in finitely many steps.
This termination property is not a global-optimality theorem.

GRASP-PR inherits the usual stochastic-search qualification: no unconditional finite-time
global convergence guarantee is claimed. If the GRASP construction/local-search mechanism
has positive probability of reaching a globally optimal basin and the search continues
indefinitely under suitable independence/reachability assumptions, failure probability can
tend to zero. Path relinking changes intensification and memory, not that fundamental
qualification.

### Scientific references

- Feo, T. A.; Resende, M. G. C. (1995). *Greedy Randomized Adaptive Search Procedures*,
  Journal of Global Optimization 6(2), 109-133. DOI: `10.1007/BF01096763`.
- Resende, M. G. C.; Ribeiro, C. C. (2005). *GRASP with path-relinking: Recent advances
  and applications*, in *Metaheuristics: Progress as Real Problem Solvers*, pp. 29-63.
  DOI: `10.1007/0-387-25383-1_2`.
- Aiex, R. M.; Resende, M. G. C.; Pardalos, P. M.; Toraldo, G. (2005).
  *GRASP with Path Relinking for Three-Index Assignment*, INFORMS Journal on Computing
  17(2), 224-247. DOI: `10.1287/ijoc.1030.0059`.

The v0.30.0 public implementation is intentionally the greedy **forward** foundation.
Backward, back-and-forward, mixed, truncated and evolutionary path-relinking policies can
reuse the explicit contracts in later releases without changing the stable GRASP-PR identity.