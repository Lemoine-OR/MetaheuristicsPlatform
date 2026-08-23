@page self_adaptive_global_best_harmony_search_pan_suganthan_tasgetiren_liang_2010 Self-Adaptive Global-best Harmony Search - Pan, Suganthan, Tasgetiren and Liang 2010

# Self-Adaptive Global-best Harmony Search - Pan, Suganthan, Tasgetiren and Liang 2010

## General description

Self-Adaptive Global-best Harmony Search (SGHS) was proposed by Pan, Suganthan,
Tasgetiren and Liang in 2010 for continuous optimization. It is based on the GHS lineage but
does not simply combine previous HS variants. SGHS introduces a new improvisation scheme,
success-based learning of the mean HMCR/PAR parameters, and a piecewise dynamic bandwidth.

The stable public identity is separate from HS 2001, IHS 2007 and GHS 2008.

## Technical specifications

- Stable ID: `self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010`
- Class: `SelfAdaptiveGlobalBestHarmonySearchOptimizer`
- Parameters: `SelfAdaptiveGlobalBestHarmonySearchParameters`
- State: `SelfAdaptiveGlobalBestHarmonySearchState`
- Family: Other / music-inspired methods
- Solution model: population / Harmony Memory
- Search space: bounded continuous vectors
- Public since: v0.58.0
- Primary DOI: `10.1016/j.amc.2010.01.088`

## Complexity

Let \f$H\f$ be HMS, \f$D\f$ the dimension, \f$NI\f$ the maximum improvisations and
\f$LP\f$ the learning period. Initialization costs \f$O(HD)\f$ plus \f$H\f$ evaluations.
Each improvisation costs \f$O(D+H)\f$ plus one objective evaluation. Sampling HMCR/PAR from
truncated normals has constant expected cost for the published standard deviations and ranges.
At each learning-period boundary, averaging the successful parameter samples costs at most
\f$O(LP)\f$. Memory remains \f$O(HD+LP)\f$.

## Applicability

The public implementation targets bounded continuous derivative-free optimization.

The paper uses \f$HMCR_m=0.98\f$, \f$PAR_m=0.9\f$, standard deviations 0.01 and 0.05,
\f$LP=100\f$, \f$BW_{\min}=0.0005\f$, and in its continuous experiments
\f$BW_{\max}=(UB-LB)/10\f$. The platform represents the latter coordinate-wise as
`0.1 * (UB_i-LB_i)` for heterogeneous boxes. That coordinate-wise lifting is explicitly a
platform adaptation; it preserves the paper's per-range prescription instead of silently using
one inappropriate scalar bandwidth across coordinates.

## Detailed operation

1. Initialize HMS harmonies and evaluate them.
2. Initialize \f$HMCR_m=0.98\f$ and \f$PAR_m=0.9\f$ by default.
3. At each improvisation sample
   \f$HMCR\sim N(HMCR_m,0.01)\f$ restricted to \f$[0.9,1]\f$ and
   \f$PAR\sim N(PAR_m,0.05)\f$ restricted to \f$[0,1]\f$.
4. Compute the coordinate bandwidth
   \f$BW_i(t)\f$: linearly decrease from \f$BW_{i,\max}\f$ toward
   \f$BW_{\min}\f$ during the first half of the run, then keep \f$BW_{\min}\f$.
5. For each coordinate, if memory consideration is selected, choose the corresponding
   coordinate from a random Harmony Memory row and perturb it by
   \f$\pm U(0,1)BW_i(t)\f$.
6. If pitch adjustment is then selected, overwrite that value with the **corresponding**
   coordinate of the current best harmony:
   \f$x_i^{new}=x_i^{best}\f$.
7. Otherwise use the ordinary bounded random-generation branch.
8. Apply the platform boundary clamp, evaluate once and replace the current worst harmony
   only on strict improvement.
9. Only after a successful replacement, record the sampled HMCR and PAR values.
10. Every LP generated solutions, set the mean HMCR/PAR to the averages of successful
    samples recorded in that period and clear the records.
11. Repeat until a common stopping criterion or NI improvisations are reached.

If a complete learning period contains no successful replacement, the paper's instruction to
average recorded successful values has an empty-set corner case. The platform leaves the two
means unchanged in that situation. This is an explicit defensive completion of an underspecified
corner case, not a claim about an additional SGHS equation.

## Parameters

- `HarmonyMemorySize`: HMS; canonical experimental default 5.
- `MaximumImprovisations`: NI.
- `InitialMeanHarmonyMemoryConsiderationRate`: initial \f$HMCR_m\f$, default 0.98.
- `InitialMeanPitchAdjustmentRate`: initial \f$PAR_m\f$, default 0.9.
- `LearningPeriod`: LP, default 100.
- `MinimumPitchAdjustmentBandwidth`: \f$BW_{\min}\f$, default 0.0005.
- `MaximumPitchAdjustmentBandwidthFractionOfRange`: platform representation of
  \f$BW_{\max}=(UB-LB)/10\f$, default 0.1.

The normal standard deviations 0.01 (HMCR) and 0.05 (PAR), and sampling ranges
[0.9,1] / [0,1], are fixed scientific constants of this stable identity rather than extra
user-tunable parameters.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<SelfAdaptiveGlobalBestHarmonySearchOptimizer>(
        MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new SelfAdaptiveGlobalBestHarmonySearchParameters
        {
            HarmonyMemorySize = 5,
            InitialMeanHarmonyMemoryConsiderationRate = 0.98,
            InitialMeanPitchAdjustmentRate = 0.9,
            LearningPeriod = 100,
            MinimumPitchAdjustmentBandwidth = 0.0005,
            MaximumPitchAdjustmentBandwidthFractionOfRange = 0.1,
            MaximumImprovisations = 1000
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),
\qquad
\mathcal X=\prod_{i=1}^{D}[l_i,u_i].
\f]

### Update equations / iterations

For each learning period, SGHS samples

\f[
HMCR_t\sim\mathcal N(HMCR_m,0.01^2)
\quad\text{restricted to }[0.9,1],
\f]

\f[
PAR_t\sim\mathcal N(PAR_m,0.05^2)
\quad\text{restricted to }[0,1].
\f]

The platform evaluates the paper's bandwidth rule coordinate-wise:

\f[
BW_i(t)=
\begin{cases}
BW_{i,\max}
-\dfrac{BW_{i,\max}-BW_{\min}}{NI}\,2t,
& 2t<NI,\\
BW_{\min},
& 2t\ge NI,
\end{cases}
\qquad
BW_{i,\max}=0.1(u_i-l_i).
\f]

For memory consideration, with a random row \f$R_i\f$,

\f[
\widetilde{x}_i
=
x_i^{(R_i)}
+
S_iU_iBW_i(t),
\qquad
S_i\in\{-1,+1\}.
\f]

If pitch adjustment succeeds, the final value is

\f[
x_i^{new}=x_i^{best},
\f]

using the corresponding coordinate \f$i\f$, not a random best-harmony coordinate as in GHS.

Let \f$\mathcal S_q\f$ be the successful improvisations in learning period \f$q\f$. At the
period boundary, when \f$\mathcal S_q\neq\varnothing\f$,

\f[
HMCR_m
=
\frac{1}{|\mathcal S_q|}
\sum_{t\in\mathcal S_q} HMCR_t,
\qquad
PAR_m
=
\frac{1}{|\mathcal S_q|}
\sum_{t\in\mathcal S_q} PAR_t.
\f]

### Assumptions

The objective is finite over a finite bounded continuous box. The published HMCR/PAR normal
ranges and standard deviations are retained. The platform uses rejection sampling to realize
the bounded normal distributions without replacing them by clipped distributions.

Final component-wise clamping is the platform's explicit boundary-repair policy for perturbed
continuous coordinates.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. SGHS modifies parameter
control and exploitation/exploration balance but remains a stochastic population-based
metaheuristic.

### Scientific references

Pan, Q.-K.; Suganthan, P. N.; Tasgetiren, M. F.; Liang, J. J. (2010),
*A self-adaptive global best harmony search algorithm for continuous optimization problems*,
Applied Mathematics and Computation 216(3), 830-848.
DOI: `10.1016/j.amc.2010.01.088`.

Omran, M. G. H.; Mahdavi, M. (2008),
*Global-best harmony search*, Applied Mathematics and Computation 198(2), 643-656.
DOI: `10.1016/j.amc.2007.09.004`.

Mahdavi, M.; Fesanghary, M.; Damangir, E. (2007),
*An improved harmony search algorithm for solving optimization problems*,
Applied Mathematics and Computation 188(2), 1567-1579.
DOI: `10.1016/j.amc.2006.11.033`.
