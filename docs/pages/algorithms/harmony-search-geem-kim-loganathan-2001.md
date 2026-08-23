@page harmony_search_geem_kim_loganathan_2001 Harmony Search - Geem, Kim and Loganathan 2001

# Harmony Search - Geem, Kim and Loganathan 2001

## General description

Harmony Search (HS) is the heuristic optimization algorithm introduced by Geem, Kim and
Loganathan in 2001. Its analogy is musical improvisation: a Harmony Memory (HM) stores a
population of candidate harmonies, and a new harmony is improvised coordinate by coordinate
by reusing values from the memory or drawing a fresh value from the feasible domain. A
memory-derived pitch may additionally be adjusted locally.

v0.55.0 implements the fixed-parameter foundation only. The Improved Harmony Search (IHS)
of Mahdavi, Fesanghary and Damangir (2007), which schedules PAR and bandwidth, and the
Global-best Harmony Search (GHS) of Omran and Mahdavi (2008) are scientifically distinct
variants and are not implemented in v0.55.0.

## Technical specifications

- Stable ID: `harmony-search-geem-kim-loganathan-2001`
- Class: `HarmonySearchOptimizer`
- Parameters: `HarmonySearchParameters`
- Family: Other / music-inspired methods
- Solution model: population / Harmony Memory
- Search space: bounded continuous vectors
- Public since: v0.55.0
- Primary DOI: `10.1177/003754970107600201`

## Complexity

Let \f$H\f$ be the Harmony Memory Size (HMS) and \f$D\f$ the dimension. Uniform
initialization costs \f$O(HD)\f$ plus \f$H\f$ objective evaluations. Each improvisation
costs \f$O(D)\f$ for coordinate construction and \f$O(H)\f$ to identify the worst/best
memory entries, plus one objective evaluation. Memory storage is \f$O(HD)\f$.

## Applicability

The public v0.55.0 implementation targets finite bounded continuous derivative-free
optimization. The original HS concept is broader and was illustrated on both combinatorial
and engineering problems; this stable platform identity deliberately exposes the
bounded-continuous realization without claiming that it exhausts all representations of HS.

## Detailed operation

1. Initialize HMS harmonies uniformly in the bounded search space and evaluate them.
2. For each coordinate of a new harmony, use Harmony Memory consideration with probability
   HMCR. If memory is not considered, draw the coordinate uniformly from its bounds.
3. Only after memory consideration, apply pitch adjustment with probability PAR.
4. Pitch adjustment uses an absolute bandwidth `bw` in coordinate units:
   \f$x'_j=x'_j\pm U(0,1)bw\f$.
5. Clamp the complete improvised harmony to the bounded domain.
6. Evaluate it once.
7. Replace the current worst harmony only when the improvised harmony is strictly better
   under the configured minimization/maximization sense.
8. Repeat until the stopping criterion or maximum improvisation count is reached.

The platform does not silently normalize `bw` by the variable range. This is an explicit
fidelity choice: `PitchAdjustmentBandwidth` is an absolute bandwidth.

## Parameters

- `HarmonyMemorySize`: HMS, number of stored harmonies.
- `MaximumImprovisations`: maximum number of completed improvisations.
- `HarmonyMemoryConsiderationRate`: HMCR in \f$[0,1]\f$.
- `PitchAdjustmentRate`: PAR in \f$[0,1]\f$.
- `PitchAdjustmentBandwidth`: non-negative absolute bandwidth \f$bw\f$.

The default numerical values are platform defaults, not a claim of universally optimal
settings.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<HarmonySearchOptimizer>(
        MetaheuristicAlgorithmIds.HarmonySearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new HarmonySearchParameters
        {
            HarmonyMemorySize = 20,
            HarmonyMemoryConsiderationRate = 0.9,
            PitchAdjustmentRate = 0.3,
            PitchAdjustmentBandwidth = 0.1,
            MaximumImprovisations = 1000
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`harmony-search-geem-kim-loganathan-2001`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),
\qquad
\mathcal X=\prod_{j=1}^{D}[l_j,u_j].
\f]

The implementation is objective-sense symmetric and therefore applies the same Harmony
Memory lifecycle to maximization through `OptimizationSense`.

### Update equations / iterations

For coordinate \f$j\f$ of a newly improvised harmony, let
\f$K_j\sim\mathcal U\{1,\ldots,H\}\f$,
\f$U_j,V_j,W_j\sim\mathcal U(0,1)\f$, and
\f$S_j\in\{-1,+1\}\f$ with equal probabilities. Then

\f[
\begin{aligned}
x'_j
&=
\begin{cases}
x_j^{(K_j)} + B_jS_jW_jbw,
    & U_j < HMCR,\\
l_j + V_j(u_j-l_j),
    & U_j \ge HMCR,
\end{cases}\\
B_j
&\sim \operatorname{Bernoulli}(PAR).
\end{aligned}
\f]

The pitch term is therefore active only on the Harmony-Memory branch. After all coordinates
are improvised, component-wise clamping enforces the bounded platform domain.

Let \f$w\f$ be the current worst Harmony Memory index. The replacement rule is

\f[
HM_w^+
=
\begin{cases}
x', & f(x') \prec f(HM_w),\\
HM_w, & \text{otherwise},
\end{cases}
\f]

where \f$\prec\f$ denotes the configured optimization sense.

### Assumptions

The public implementation assumes a finite-dimensional bounded continuous box, finite
objective values, finite bandwidth, and fixed HMCR/PAR/bandwidth during one run. Random
sampling is owned by the common deterministic `OptimizationContext`.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. Practical behavior
depends on HMS, HMCR, PAR, absolute bandwidth, objective landscape, coordinate scaling and
evaluation budget. Later adaptive schedules or global-best mechanisms must not be
retroactively attributed to the 2001 stable identity.

### Scientific references

Geem, Z. W.; Kim, J. H.; Loganathan, G. V. (2001),
*A New Heuristic Optimization Algorithm: Harmony Search*, SIMULATION 76(2), 60-68.
DOI: `10.1177/003754970107600201`.

Mahdavi, M.; Fesanghary, M.; Damangir, E. (2007),
*An improved harmony search algorithm for solving optimization problems*,
Applied Mathematics and Computation 188(2), 1567-1579.
DOI: `10.1016/j.amc.2006.11.033`.
This IHS variant is reviewed but not implemented in v0.55.0.

Omran, M. G. H.; Mahdavi, M. (2008),
*Global-best harmony search*, Applied Mathematics and Computation 198(2), 643-656.
DOI: `10.1016/j.amc.2007.09.004`.
This GHS variant is reviewed but not implemented in v0.55.0.