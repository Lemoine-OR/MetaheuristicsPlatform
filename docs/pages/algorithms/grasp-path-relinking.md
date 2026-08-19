@page grasp_path_relinking GRASP with Path Relinking

# GRASP with Path Relinking

## General description

GRASP with Path Relinking (GRASP-PR) augments randomized greedy construction and local
search with a quality/diversity elite set and target-directed intensification trajectories.
Version 0.32.0 adds **Evolutionary Path Relinking (EvPR)** as an optional post-optimization
phase that evolves the elite population over generations.

The v0.31.0 pairwise strategies remain fully available and backward compatible.

@subpage path_relinking_strategies

## Technical specifications

- Stable ID: `grasp-path-relinking`.
- Public optimizer: `GraspPathRelinkingOptimizer<TSolution>`.
- Pairwise advanced engine:
  `AdvancedPathRelinkingProcedure<TSolution,TMove,TUndo,TEnumerator>`.
- Population-level engine: `EvolutionaryPathRelinkingProcedure<TSolution>`.
- Pairwise directions: forward, backward, back-and-forward, mixed.
- Pairwise move policies: greedy, greedy-randomized adaptive RCL.
- Truncation: orthogonal `PathFraction` in \f$(0,1]\f$.
- EvPR generation rule: exhaustive unordered elite-pair relinking into a fresh population.
- EvPR admission: best override or worst-quality + diversity threshold, then closest
  dominated replacement.
- Optional local search on every EvPR offspring before admission.
- Exact global objective accounting remains owned by `OptimizationContext`.

## Complexity

Let \f$E\f$ be elite capacity and \f$C_{PR}\f$ one pairwise relinking cost. A complete
EvPR generation performs at most

\f[
\binom{E}{2}
\f]

pairwise relinkings, hence

\f[
O(E^2 C_{PR})
\f]

before optional local-search cost. Elite memory remains \f$O(E\cdot|x|)\f$ for owned
solution snapshots.

The pairwise complexity remains

\f[
O\!\left(\sum_{k=1}^{D}P_kC_p + DC_\rho\right)
\f]

for one directional traversal.

## Applicability

The method targets finite combinatorial or mixed representations with:

- a non-negative integral path distance;
- target-directed move enumeration;
- reversible move application;
- compatible GRASP construction and reusable local search;
- optional exact objective-delta evaluation.

EvPR additionally benefits from a small, diverse elite pool; its exhaustive pairing cost is
quadratic in elite-set cardinality.

## Detailed operation

The ordinary GRASP-PR phase constructs and locally improves a solution, optionally relinks it
against an elite guide, then updates the elite set.

When `EvolutionaryPathRelinkingEnabled` is `true` and the ordinary outer loop ends without a
generic stopping criterion firing, the elite pool becomes generation zero. For every generation,
all unordered pairs are relinked. Each offspring may be locally improved, then is admitted into
a fresh elite population using the Resende-Werneck quality/diversity rule. The process ends at
the first generation whose best objective does not strictly improve its predecessor, or at the
configured evolutionary generation cap.

## Parameters

Existing pairwise parameters remain unchanged:

- `PathDirection`;
- `PathMoveSelection`;
- `PathFraction`;
- `PathRelinkingAlpha`;
- `MaximumPathSteps`.

EvPR adds:

- `EvolutionaryPathRelinkingEnabled` — default `false`;
- `MaximumEvolutionaryGenerations` — default `10`;
- `MaximumEvolutionaryPathSteps`;
- `ImproveEvolutionaryOffspring` — default `true`;
- `EvolutionaryPathDirection` — default `Mixed`;
- `EvolutionaryPathMoveSelection` — default `GreedyRandomizedAdaptive`;
- `EvolutionaryPathFraction` — default `1.0`;
- `EvolutionaryPathRelinkingAlpha` — default `0.2`.

The disabled default preserves v0.31.0 behavior exactly.

## Observable state

`GraspPathRelinkingState` retains its existing positional constructor and adds init-only
evolutionary statistics:

- `EvolutionaryGenerationsCompleted`;
- `EvolutionaryPairRelinkings`;
- `EvolutionaryPathSteps`;
- `EvolutionaryCandidateEvaluations`;
- `EvolutionaryAcceptedLocalMoves`;
- `EvolutionaryElitePoolUpdates`.

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
            PathMoveSelection =
                PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive,

            EvolutionaryPathRelinkingEnabled = true,
            MaximumEvolutionaryGenerations = 10,
            EvolutionaryPathDirection =
                PathRelinkingDirectionStrategy.Mixed,
            EvolutionaryPathMoveSelection =
                PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive,
            ImproveEvolutionaryOffspring = true
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
\rho(x^I,x^G)\in\mathbb Z_{\ge0}.
\f]

### Update equations / iterations

#### Pairwise update

For randomized minimization path selection,

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

#### Evolutionary generation

\f[
P^{k+1}
=
\operatorname{Elite}
\left(
\left\{
PR(x_i^k,x_j^k)
:
1\le i<j\le |P^k|
\right\}
\right).
\f]

Evolution stops when

\f[
f^*_{k+1}\not\prec f^*_k.
\f]

### Assumptions

- Path distance is non-negative and integral.
- Positive remaining distance exposes a target-directed move.
- Selected pairwise moves strictly reduce the relevant distance.
- Reversible operators restore exact pre-probe state.
- Optional delta evaluators are exact.
- Elite diversity is defined by `MinimumEliteDistance`.
- EvPR starts only with at least two retained elite solutions.

### Convergence conditions

Each individual full path terminates because a non-negative integral path distance strictly
decreases. The EvPR outer process terminates at the first non-improving generation or at
`MaximumEvolutionaryGenerations`. These are finite termination properties, not a proof of
global optimality.

### Scientific references

- Feo, T. A.; Resende, M. G. C. (1995).
  *Greedy Randomized Adaptive Search Procedures*.
  DOI: `10.1007/BF01096763`.
- Resende, M. G. C.; Werneck, R. F. (2004).
  *A Hybrid Heuristic for the p-Median Problem*,
  Journal of Heuristics 10(1), 59-88.
  DOI: `10.1023/B:HEUR.0000019986.96257.50`.
- Aiex, R. M.; Resende, M. G. C.; Pardalos, P. M.; Toraldo, G. (2005).
  *GRASP with Path Relinking for Three-Index Assignment*.
  DOI: `10.1287/ijoc.1030.0059`.
- Resende, M. G. C.; Marti, R.; Gallego, M.; Duarte, A. (2010).
  *GRASP and path relinking for the max-min diversity problem*.
  DOI: `10.1016/j.cor.2008.05.011`.
- Ribeiro, C. C.; Resende, M. G. C. (2012).
  *Path-relinking intensification methods for stochastic local search algorithms*.
  DOI: `10.1007/s10732-011-9167-1`.