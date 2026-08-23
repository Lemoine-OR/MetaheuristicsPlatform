@page adaptive_large_neighborhood_search_ropke_pisinger_2006 Adaptive Large Neighborhood Search - Ropke-Pisinger 2006

# Adaptive Large Neighborhood Search - Ropke-Pisinger 2006

## General description

Adaptive Large Neighborhood Search (ALNS) extends Large Neighborhood Search by maintaining
multiple competing destroy and repair subheuristics. A destroy operator and a repair operator
are selected independently by roulette-wheel probabilities proportional to adaptive weights.
Their historical performance is accumulated over a segment of iterations and converted into
new weights using the reaction-factor update of Ropke and Pisinger.

The canonical v0.53 identity reuses the generic LNS destroy, repair and acceptance contracts
introduced in v0.52. It adds the adaptive control layer rather than duplicating the LNS core.

## Technical specifications

- Stable ID: `adaptive-large-neighborhood-search-ropke-pisinger-2006`
- Class: `AdaptiveLargeNeighborhoodSearchOptimizer<TSolution,TRemoved>`
- Parameters: `AdaptiveLargeNeighborhoodSearchParameters`
- Family: Trajectory-based methods
- Search spaces: binary, integer, permutation, combinatorial and mixed
- Public since: v0.53.0
- Primary DOI: `10.1287/trsc.1050.0135`
- Supporting DOI: `10.1016/j.cor.2005.09.012`

## Complexity

With \f$D\f$ destroy operators, \f$R\f$ repair operators and segment length \f$L\f$, each
iteration adds \f$O(D+R)\f$ roulette selection in the straightforward implementation plus
domain destruction, repair and one objective evaluation. Segment-end weight adaptation costs
\f$O(D+R)\f$. Novelty tracking uses a user-supplied equality comparer and a hash set.

## Applicability

ALNS is appropriate when several structurally different destroy and repair heuristics are
available and their relative usefulness varies across instances or search phases. The framework
is especially effective for highly structured discrete problems such as routing and scheduling.

## Detailed operation

1. Generate and evaluate one initial complete solution.
2. Initialize all destroy and repair weights equally.
3. Select one destroy and one repair independently by roulette-wheel probabilities.
4. Clone the incumbent, destroy it and repair it to a complete candidate.
5. Evaluate the repaired candidate exactly once.
6. Detect whether the candidate is novel using the supplied solution comparer.
7. Accept or reject the candidate. The canonical default is geometric simulated annealing.
8. Assign the selected operators a reward for a novel global best, novel improving accepted
   solution, or novel accepted non-improving solution.
9. At the end of each segment, update every used operator weight from average segment score
   and reset scores/usages.

A generic stopping condition after the candidate evaluation leaves the adaptive cycle
incomplete: the candidate can update best-so-far, but no reward, usage count, acceptance
decision or completed iteration is recorded.

## Parameters

- `DestructionSize`: domain-defined destruction intensity passed to the selected destroy.
- `MaximumIterations`: maximum number of complete adaptive cycles.
- `SegmentLength`: complete iterations between weight updates.
- `ReactionFactor`: \f$r\in[0,1]\f$.
- `InitialOperatorWeight`: common initial destroy/repair weight.
- `GlobalBestReward`: \f$\sigma_1\f$, default 33.
- `ImprovingReward`: \f$\sigma_2\f$, default 9.
- `AcceptedReward`: \f$\sigma_3\f$, default 13.
- `InitialTemperature`: explicit Metropolis starting temperature.
- `CoolingRate`: geometric temperature factor, default 0.99975.

The original routing implementation determines its starting temperature from the initial
objective scale. MetaheuristicsPlatform keeps the starting temperature explicit because a
generic objective may be zero, negative or expressed in arbitrary units.

## API example

```csharp
var algorithm =
    new AdaptiveLargeNeighborhoodSearchOptimizer<MySolution,RemovedSet>(
        initialSolutionGenerator,
        destroyOperators,
        repairOperators,
        solutionComparer);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.AdaptiveLargeNeighborhoodSearch,
    () => algorithm,
    replace: true);

OptimizationResult<MySolution> result =
    algorithm.Optimize(
        problem,
        new AdaptiveLargeNeighborhoodSearchParameters
        {
            DestructionSize = 10,
            SegmentLength = 100,
            ReactionFactor = 0.1,
            GlobalBestReward = 33.0,
            ImprovingReward = 9.0,
            AcceptedReward = 13.0,
            InitialTemperature = 100.0,
            CoolingRate = 0.99975,
            MaximumIterations = 10000
        },
        solutionCloner,
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`adaptive-large-neighborhood-search-ropke-pisinger-2006`

## Mathematical details

### Problem formulation

\f[
\operatorname*{opt}_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

For destroy pool \f$\mathcal D\f$, repair pool \f$\mathcal R\f$, segment \f$s\f$ and operator
performance \f$(\pi_h,\theta_h)\f$:

\f[
\begin{aligned}
\Pr(D_i)
&=\frac{w_{D_i}^{s}}{\sum_{D_j\in\mathcal D}w_{D_j}^{s}},\\
\Pr(R_i)
&=\frac{w_{R_i}^{s}}{\sum_{R_j\in\mathcal R}w_{R_j}^{s}},\\
w_h^{s+1}
&=
\begin{cases}
(1-r)w_h^s+r\,\dfrac{\pi_h}{\theta_h},&\theta_h>0,\\
w_h^s,&\theta_h=0,
\end{cases}\\
T_k
&=T_0\alpha^{k-1},\\
\Pr(\operatorname{accept}\ y\mid \Delta>0)
&=\exp\!\left(-\frac{\Delta}{T_k}\right).
\end{aligned}
\f]

The reward accumulated by both selected operators is \f$\sigma_1\f$ for a novel new global
best, \f$\sigma_2\f$ for a novel accepted current improvement, \f$\sigma_3\f$ for a novel
accepted non-improving candidate, and zero otherwise.

### Assumptions

Destroy/repair operators share a compatible removed-component representation. The supplied
solution comparer defines stable equality for cloned solution snapshots. Objective values are
finite. Every repair must restore a complete evaluable solution. Operator IDs are non-empty
and unique within each pool.

### Convergence conditions

No universal finite-time global-optimum guarantee is claimed. The adaptive weights influence
operator frequencies rather than creating global reachability by themselves. Global
convergence requires additional assumptions on destroy/repair reachability, acceptance and
continued exploration.

### Scientific references

Ropke & Pisinger (2006), *An Adaptive Large Neighborhood Search Heuristic for the Pickup and
Delivery Problem with Time Windows*, Transportation Science 40(4), 455-472.
DOI: `10.1287/trsc.1050.0135`.

Pisinger & Ropke (2007), *A General Heuristic for Vehicle Routing Problems*,
Computers & Operations Research 34(8), 2403-2435.
DOI: `10.1016/j.cor.2005.09.012`.
