@page global_best_harmony_search_omran_mahdavi_2008 Global-best Harmony Search - Omran and Mahdavi 2008

# Global-best Harmony Search - Omran and Mahdavi 2008

## General description

Global-best Harmony Search (GHS) is the 2008 variant proposed by Omran and Mahdavi. It
borrows a global-best influence concept from swarm intelligence while retaining the Harmony
Memory lifecycle. GHS uses the dynamic PAR schedule already introduced by IHS, removes the
bandwidth parameter entirely, and changes the pitch-adjustment rule so that the destination
coordinate receives a value from a randomly selected coordinate of the current best harmony.

v0.57.0 keeps GHS as a third public scientific identity, separate from fixed-parameter HS 2001
and IHS 2007. It does not silently add SGHS/NGHS/IGHS learning, mutation, differential or
self-adaptive mechanisms.

## Technical specifications

- Stable ID: `global-best-harmony-search-omran-mahdavi-2008`
- Class: `GlobalBestHarmonySearchOptimizer`
- Parameters: `GlobalBestHarmonySearchParameters`
- State: `GlobalBestHarmonySearchState`
- Family: Other / music-inspired methods
- Solution model: population / Harmony Memory
- Search space: bounded continuous vectors
- Public since: v0.57.0
- Primary DOI: `10.1016/j.amc.2007.09.004`

## Complexity

Let \f$H\f$ be HMS, \f$D\f$ the decision-vector dimension and \f$NI\f$ the maximum number of
improvisations. Initialization costs \f$O(HD)\f$ plus \f$H\f$ objective evaluations. Each
improvisation costs \f$O(D+H)\f$ plus one objective evaluation; the dynamic PAR computation is
\f$O(1)\f$. Memory remains \f$O(HD)\f$.

## Applicability

The public platform identity targets finite bounded continuous derivative-free optimization.
GHS itself removes `bw`, which avoids bandwidth tuning. Its cross-coordinate pitch rule can,
however, copy a value from dimension \f$k\f$ of the best harmony into a different destination
dimension \f$i\f$. When coordinate domains differ, that value may be infeasible for dimension
\f$i\f$. The platform therefore applies its existing final component-wise `Clamp` as an
explicit bounded-domain repair. This repair is a platform adaptation and is not attributed to
the Omran-Mahdavi pitch equation.

## Detailed operation

1. Initialize HMS harmonies uniformly in the bounded search space and evaluate them.
2. Keep HMCR fixed.
3. At improvisation \f$t\f$, compute
   \f$PAR(t)=PAR_{\min}+(PAR_{\max}-PAR_{\min})t/NI\f$.
4. Determine the current best harmony in Harmony Memory.
5. For coordinate \f$i\f$, use the standard Harmony Memory branch with probability HMCR;
   otherwise draw uniformly from the bounds of coordinate \f$i\f$.
6. If memory was considered, apply pitch adjustment with probability \f$PAR(t)\f$.
7. GHS pitch adjustment has no bandwidth: choose
   \f$k\sim\mathcal U\{1,\ldots,D\}\f$ and set
   \f$x_i^{new}=x_k^{best}\f$.
8. Apply the explicit platform bounded-domain clamp to the completed candidate.
9. Evaluate once and replace the current worst harmony only if the candidate is strictly better.
10. Repeat until a stopping criterion or NI improvisations are reached.

## Parameters

- `HarmonyMemorySize`: HMS. The paper's main GHS experiments use HMS = 5.
- `MaximumImprovisations`: NI.
- `HarmonyMemoryConsiderationRate`: fixed HMCR. The main experiments use HMCR = 0.9.
- `MinimumPitchAdjustmentRate`: \f$PAR_{\min}\f$, 0.01 in the main experiments.
- `MaximumPitchAdjustmentRate`: \f$PAR_{\max}\f$, 0.99 in the main experiments.

There is deliberately **no bandwidth parameter** in `GlobalBestHarmonySearchParameters`.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<GlobalBestHarmonySearchOptimizer>(
        MetaheuristicAlgorithmIds.GlobalBestHarmonySearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new GlobalBestHarmonySearchParameters
        {
            HarmonyMemorySize = 5,
            HarmonyMemoryConsiderationRate = 0.9,
            MinimumPitchAdjustmentRate = 0.01,
            MaximumPitchAdjustmentRate = 0.99,
            MaximumImprovisations = 1000
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`global-best-harmony-search-omran-mahdavi-2008`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),
\qquad
\mathcal X=\prod_{i=1}^{D}[l_i,u_i].
\f]

### Update equations / iterations

For \f$t\in\{1,\ldots,NI\}\f$,

\f[
PAR_t
=
PAR_{\min}
+
\frac{PAR_{\max}-PAR_{\min}}{NI}t.
\f]

Let \f$b\f$ be the index of the current best harmony, \f$R_i\f$ a uniformly selected Harmony
Memory row, \f$K_i\sim\mathcal U\{1,\ldots,D\}\f$, \f$U_i,V_i\sim\mathcal U(0,1)\f$, and
\f$B_{i,t}\sim\operatorname{Bernoulli}(PAR_t)\f$. The GHS improvisation rule is represented by

\f[
\begin{aligned}
x_i^{new}
&=
\begin{cases}
x_{K_i}^{(b)},
    & U_i<HMCR,\ B_{i,t}=1,\\
x_i^{(R_i)},
    & U_i<HMCR,\ B_{i,t}=0,\\
l_i+V_i(u_i-l_i),
    & U_i\ge HMCR.
\end{cases}
\end{aligned}
\f]

The defining GHS pitch equation is therefore
\f$x_i^{new}=x_k^{best}\f$ with a randomly selected decision-variable index \f$k\f$.
No `bw` term appears.

### Assumptions

The represented platform domain is a finite bounded continuous box and objective values must
be finite. HMS, HMCR, PAR bounds and NI remain fixed configuration values. The published
cross-coordinate best-harmony rule is reproduced; final component-wise clamping is explicitly
identified as platform boundary repair for heterogeneous bounds.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The global-best pitch rule
increases exploitation pressure and later literature has discussed premature-convergence and
heterogeneous-bound issues. Those observations are documented without retroactively modifying
the 2008 GHS equation.

### Scientific references

Omran, M. G. H.; Mahdavi, M. (2008),
*Global-best harmony search*, Applied Mathematics and Computation 198(2), 643-656.
DOI: `10.1016/j.amc.2007.09.004`.

Mahdavi, M.; Fesanghary, M.; Damangir, E. (2007),
*An improved harmony search algorithm for solving optimization problems*,
Applied Mathematics and Computation 188(2), 1567-1579.
DOI: `10.1016/j.amc.2006.11.033`.

Geem, Z. W.; Kim, J. H.; Loganathan, G. V. (2001),
*A New Heuristic Optimization Algorithm: Harmony Search*, SIMULATION 76(2), 60-68.
DOI: `10.1177/003754970107600201`.
