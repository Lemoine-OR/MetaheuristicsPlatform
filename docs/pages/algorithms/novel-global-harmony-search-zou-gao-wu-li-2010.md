@page novel_global_harmony_search_zou_gao_wu_li_2010 Novel Global Harmony Search - Zou, Gao, Wu and Li 2010

# Novel Global Harmony Search - Zou, Gao, Wu and Li 2010

## General description

Novel Global Harmony Search (NGHS) was introduced by Zou, Gao, Wu and Li in 2010. It
combines the Harmony Memory framework with a PSO-inspired global-best position update and
GA-style low-probability mutation.

NGHS is not an incremental parameter schedule for HS. It removes HMCR, PAR and BW entirely.
A new harmony is generated from the current best and worst harmonies, mutated coordinate-wise
with probability \f$p_m\f$, and then **unconditionally replaces the current worst harmony**.
The unconditional replacement is part of canonical NGHS even when the candidate is worse.

The stable v0.59.0 identity is based on the original Computers & Industrial Engineering paper
that proposed NGHS for reliability problems. The later Neurocomputing paper applies the
recently proposed NGHS to unconstrained continuous benchmarks and is retained as supporting
provenance rather than replacing the original DOI.

## Technical specifications

- Stable ID: `novel-global-harmony-search-zou-gao-wu-li-2010`
- Class: `NovelGlobalHarmonySearchOptimizer`
- Parameters: `NovelGlobalHarmonySearchParameters`
- State: `NovelGlobalHarmonySearchState`
- Family: Other / music-inspired methods
- Solution model: population / Harmony Memory
- Search space: bounded continuous vectors
- Public since: v0.59.0
- Primary DOI: `10.1016/j.cie.2009.11.003`

## Complexity

Let \f$H\f$ be HMS and \f$D\f$ the dimension. Initialization costs \f$O(HD)\f$ plus
\f$H\f$ objective evaluations. Each improvisation identifies the best/worst Harmony Memory
members in \f$O(H)\f$, constructs one candidate in \f$O(D)\f$, evaluates it once and replaces
the current worst harmony. Time is therefore \f$O(HD)\f$ initialization and
\f$O(H+D)\f$ per improvisation, excluding objective cost. Memory is \f$O(HD)\f$.

## Applicability

The original proposal targets reliability optimization and uses the same continuous bounded
position update later evaluated on unconstrained benchmark problems. The platform public
identity exposes the continuous bounded search mechanism and leaves problem-specific reliability
constraints to the problem/evaluation layer.

No external mathematics package is required: NGHS uses only basic .NET floating-point
arithmetic and uniform random sampling.

## Detailed operation

1. Initialize HMS bounded continuous harmonies and evaluate them.
2. Identify the current best harmony \f$x^{best}\f$ and worst harmony \f$x^{worst}\f$.
3. For coordinate \f$i\f$, compute the reflected trust-region endpoint
   \f$x_{R,i}=2x_i^{best}-x_i^{worst}\f$.
4. Truncate \f$x_{R,i}\f$ to the coordinate bounds.
5. Position-update from the current worst coordinate:
   \f$x_i^{new}=x_i^{worst}+U_i(x_{R,i}-x_i^{worst})\f$.
6. With probability \f$p_m\f$, replace that coordinate by a fresh uniform value in its bounds.
7. Evaluate the completed new harmony once.
8. Replace the current worst harmony **without a fitness precondition**.
9. Repeat until the common stopping criterion or NI improvisations are completed.

## Parameters

- `HarmonyMemorySize`: HMS; default 5.
- `MaximumImprovisations`: NI.
- `MutationProbability`: \f$p_m\f$; default 0.005 for the continuous platform identity.

There are deliberately no HMCR, PAR or BW parameters.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<NovelGlobalHarmonySearchOptimizer>(
        MetaheuristicAlgorithmIds.NovelGlobalHarmonySearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new NovelGlobalHarmonySearchParameters
        {
            HarmonyMemorySize = 5,
            MutationProbability = 0.005,
            MaximumImprovisations = 1000
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`novel-global-harmony-search-zou-gao-wu-li-2010`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),
\qquad
\mathcal X=\prod_{i=1}^{D}[l_i,u_i].
\f]

### Update equations / iterations

At iteration \f$t\f$, let \f$x^{best}\f$ and \f$x^{worst}\f$ be the best and worst
Harmony Memory members under the configured optimization sense.

For each coordinate,

\f[
x_{R,i}
=
\operatorname{clip}_{[l_i,u_i]}
\left(
2x_i^{best}-x_i^{worst}
\right),
\f]

and, with \f$U_i\sim\mathcal U(0,1)\f$,

\f[
\widetilde{x}_i
=
x_i^{worst}
+
U_i
\left(
x_{R,i}-x_i^{worst}
\right).
\f]

Let \f$M_i\sim\operatorname{Bernoulli}(p_m)\f$ and
\f$V_i\sim\mathcal U(0,1)\f$. The final coordinate is

\f[
x_i^{new}
=
\begin{cases}
l_i+V_i(u_i-l_i), & M_i=1,\\
\widetilde{x}_i, & M_i=0.
\end{cases}
\f]

After evaluation,

\f[
x^{worst}\leftarrow x^{new}
\f]

**unconditionally**. There is no
\f$f(x^{new})<f(x^{worst})\f$ acceptance condition in canonical NGHS.

### Assumptions

The represented public domain is a finite bounded continuous box with finite objective values.
The reflected endpoint is explicitly truncated before interpolation, as specified by NGHS.
The interpolation segment and mutation branch are therefore already bounded and no additional
post-hoc clamp is needed.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The unconditional
replacement deliberately permits deterioration of Harmony Memory and distinguishes canonical
NGHS from later selective-acceptance or fitter-only variants.

### Scientific references

Zou, D. X.; Gao, L. Q.; Wu, J. H.; Li, S.; Li, Y. (2010),
*A novel global harmony search algorithm for reliability problems*,
Computers & Industrial Engineering 58(2), 307-316.
DOI: `10.1016/j.cie.2009.11.003`.

Zou, D.; Gao, L.; Wu, J.; Li, S. (2010),
*Novel global harmony search algorithm for unconstrained problems*,
Neurocomputing 73(16-18), 3308-3318.
DOI: `10.1016/j.neucom.2010.07.010`.
