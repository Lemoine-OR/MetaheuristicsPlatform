@page improved_harmony_search_mahdavi_fesanghary_damangir_2007 Improved Harmony Search - Mahdavi, Fesanghary and Damangir 2007

# Improved Harmony Search - Mahdavi, Fesanghary and Damangir 2007

## General description

Improved Harmony Search (IHS) is the 2007 variant proposed by Mahdavi, Fesanghary and
Damangir to improve the fine-tuning behavior of the original Harmony Search. The Harmony
Memory lifecycle, fixed HMCR, memory consideration, random generation, pitch-adjustment
branch and worst-harmony replacement remain those of HS. The scientific change is the
deterministic scheduling of PAR and bandwidth over the generation counter.

This public identity remains separate from fixed-parameter HS 2001 and Global-best Harmony
Search (GHS) of Omran and Mahdavi (2008). GHS is exposed as a separate public identity since
v0.57.0; IHS itself retains bandwidth-based pitch adjustment and does not absorb global-best,
PSO-like, feedback self-adaptive or local-search mechanisms.

## Technical specifications

- Stable ID: `improved-harmony-search-mahdavi-fesanghary-damangir-2007`
- Class: `ImprovedHarmonySearchOptimizer`
- Parameters: `ImprovedHarmonySearchParameters`
- Family: Other / music-inspired methods
- Solution model: population / Harmony Memory
- Search space: bounded continuous vectors
- Public since: v0.56.0
- Primary DOI: `10.1016/j.amc.2006.11.033`

## Complexity

Let \f$H\f$ be HMS, \f$D\f$ the dimension and \f$NI\f$ the maximum number of
improvisations. Initialization costs \f$O(HD)\f$ plus \f$H\f$ objective evaluations.
Each IHS improvisation costs \f$O(D+H)\f$ plus one objective evaluation. The deterministic
PAR/bw schedule adds only \f$O(1)\f$ work per improvisation. Memory remains \f$O(HD)\f$.

## Applicability

The v0.56.0 implementation targets finite bounded continuous derivative-free optimization.
The scientific schedule requires user-selected positive `bw_min` and `bw_max`; the literature
explicitly treats these values as problem-dependent. The platform therefore exposes them as
absolute coordinate-unit parameters and does not silently normalize them by variable ranges.

## Detailed operation

1. Initialize HMS harmonies uniformly in the bounded continuous search space and evaluate them.
2. Keep HMCR fixed during the run.
3. For generation \f$t\f$, compute the published IHS schedules:
   \f$PAR(t)=PAR_{\min}+(PAR_{\max}-PAR_{\min})t/NI\f$ and
   \f$bw(t)=bw_{\max}\exp((t/NI)\ln(bw_{\min}/bw_{\max}))\f$.
4. Improvise each coordinate exactly as in HS: memory consideration with HMCR, otherwise
   uniform random generation in the coordinate bounds.
5. On the memory branch only, perform pitch adjustment with probability \f$PAR(t)\f$ using
   \f$x'_j=x'_j\pm U(0,1)bw(t)\f$.
6. Clamp the completed candidate to the bounded platform domain and evaluate it once.
7. Replace the current worst harmony only if the new harmony is strictly better under the
   configured optimization sense.
8. Repeat until the stopping criterion or NI improvisations are reached.

The paper expresses the schedules using a generation number and NI. The platform maps the
first configured improvisation to \f$t=1\f$ and the last to \f$t=NI\f$, which preserves the
published equations and reaches \f$PAR_{\max}\f$ and \f$bw_{\min}\f$ on the final configured
improvisation. This is an explicit indexing convention, not an additional search mechanism.

## Parameters

- `HarmonyMemorySize`: HMS.
- `MaximumImprovisations`: NI.
- `HarmonyMemoryConsiderationRate`: fixed HMCR in \f$[0,1]\f$.
- `MinimumPitchAdjustmentRate`: \f$PAR_{\min}\f$.
- `MaximumPitchAdjustmentRate`: \f$PAR_{\max}\f$.
- `MinimumPitchAdjustmentBandwidth`: positive \f$bw_{\min}\f$.
- `MaximumPitchAdjustmentBandwidth`: positive \f$bw_{\max}\f$.

The numerical defaults are platform convenience defaults. The 2007 method itself requires
problem-dependent tuning of the bandwidth limits; no universal paper-derived bandwidth
default is claimed.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ImprovedHarmonySearchOptimizer>(
        MetaheuristicAlgorithmIds.ImprovedHarmonySearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new ImprovedHarmonySearchParameters
        {
            HarmonyMemorySize = 5,
            HarmonyMemoryConsiderationRate = 0.9,
            MinimumPitchAdjustmentRate = 0.01,
            MaximumPitchAdjustmentRate = 0.99,
            MinimumPitchAdjustmentBandwidth = 0.0001,
            MaximumPitchAdjustmentBandwidth = 1.0,
            MaximumImprovisations = 1000
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`improved-harmony-search-mahdavi-fesanghary-damangir-2007`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),
\qquad
\mathcal X=\prod_{j=1}^{D}[l_j,u_j].
\f]

### Update equations / iterations

For generation \f$t\in\{1,\ldots,NI\}\f$,

\f[
\begin{aligned}
PAR_t
&=PAR_{\min}
+\frac{PAR_{\max}-PAR_{\min}}{NI}t,\\
bw_t
&=bw_{\max}
\exp\!\left(
\frac{t}{NI}
\ln\frac{bw_{\min}}{bw_{\max}}
\right).
\end{aligned}
\f]

For coordinate \f$j\f$, let \f$K_j\sim\mathcal U\{1,\ldots,H\}\f$,
\f$U_j,V_j,W_j\sim\mathcal U(0,1)\f$ and
\f$S_j\in\{-1,+1\}\f$ with equal probabilities. The platform realization is

\f[
\begin{aligned}
x'_j
&=
\begin{cases}
x_j^{(K_j)}+B_{j,t}S_jW_jbw_t,
    & U_j<HMCR,\\
l_j+V_j(u_j-l_j),
    & U_j\ge HMCR,
\end{cases}\\
B_{j,t}
&\sim\operatorname{Bernoulli}(PAR_t).
\end{aligned}
\f]

The strict Harmony Memory replacement rule is unchanged from canonical HS.

### Assumptions

The public implementation assumes a finite bounded continuous box, finite objective values,
fixed HMS/HMCR/NI during a run, \f$0\le PAR_{\min}\le PAR_{\max}\le1\f$, and
\f$0<bw_{\min}\le bw_{\max}\f$. Randomness remains owned by the common
`OptimizationContext`.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. IHS changes deterministic
parameter control, not the fundamental stochastic replacement process. Performance remains
problem- and scale-dependent, especially through the user-selected bandwidth limits.

### Scientific references

Mahdavi, M.; Fesanghary, M.; Damangir, E. (2007),
*An improved harmony search algorithm for solving optimization problems*,
Applied Mathematics and Computation 188(2), 1567-1579.
DOI: `10.1016/j.amc.2006.11.033`.

Geem, Z. W.; Kim, J. H.; Loganathan, G. V. (2001),
*A New Heuristic Optimization Algorithm: Harmony Search*, SIMULATION 76(2), 60-68.
DOI: `10.1177/003754970107600201`.

Omran, M. G. H.; Mahdavi, M. (2008),
*Global-best harmony search*, Applied Mathematics and Computation 198(2), 643-656.
DOI: `10.1016/j.amc.2007.09.004`.
GHS is a separate public identity since v0.57.0.
