@page advanced_parameter_setting_free_harmony_search_object_jeong_park_geem_sim_2020 Advanced Parameter-Setting-Free Harmony Search - Object Scheme - Jeong et al. 2020

# Advanced Parameter-Setting-Free Harmony Search - Object Scheme

## General description

This v0.62.0 identity implements the **Object PSF** branch of Jeong, Park, Geem and Sim
(2020), separately from the iteration-dependent v0.61.0 identity.

Object PSF requires a known target objective. It uses the mean objective currently stored in
Harmony Memory, the target `Loss_obj`, and `Loss_start` measured after the first HMS
improvisations. HMCR follows Equation (7), PAR follows Equation (8), and this identity also
implements the object-only adaptive pitch bandwidth of Equation (9).

The publication states Equation (7) for finding the global minimum. The platform therefore
does not reinterpret it for maximization.

## Technical specifications

- Stable ID: `advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020`
- Class: `AdvancedParameterSettingFreeHarmonySearchObjectOptimizer`
- Public since: v0.62.0
- Primary DOI: `10.3390/app10072586`
- Family: Other / music-inspired
- Search space: bounded continuous vectors
- Sense: minimization only
- Scientific scope: minimization-only Object PSF Equation (7)
- OTM: none
- External math package: none

## Complexity

Harmony Memory is \f$O(HD)\f$. Each improvisation constructs one candidate in \f$O(D)\f$
and scans HM in \f$O(H)\f$. Mean-HM evaluation is \f$O(H)\f$. Equation (9) is scalar and
is recomputed at HMS-sized block boundaries.

## Applicability

Object PSF is applicable when a meaningful minimum target objective is known. The paper
explicitly contrasts this with iteration PSF for problems where no object value is known.

The paper leaves the fixed HMCR/PAR used in the initial HMS rehearsal as "specific values".
The platform defaults these to 0.5, consistent with the paper's PSF rehearsal examples, but
exposes both values rather than pretending the publication uniquely specifies them.

Before Equation (9) can be computed, the paper says to set bandwidth to any value within the
input range and notes 0.1% of full range as typical HS practice. The platform therefore uses
0.001 of each coordinate range as its documented default for this underspecified initial block.

For heterogeneous bounded boxes, the scalar \f$(U-L)\f$ factor in Equation (9) is lifted
coordinate-wise. The loss-dependent scalar fraction is unchanged.

## Detailed operation

1. Initialize HMS harmonies and evaluate them.
2. If the target is already reached, stop.
3. Perform exactly HMS rehearsal improvisations using fixed rehearsal HMCR/PAR and the initial bandwidth.
4. Set `Loss_start` to the mean objective currently stored in HM.
5. Compute object-dependent HMCR from Equation (7).
6. Compute PAR from Equation (8).
7. Improvise by ordinary HS memory/random choice and pitch adjustment.
8. Strictly replace the current worst harmony only when the candidate is better.
9. At each completed HMS-sized performance block, compute the new HM mean and update bandwidth for the next block using Equation (9).
10. Stop when the target is reached, a common stopping rule fires, or the platform safety ceiling is reached.

## Parameters

- `HarmonyMemorySize`
- `MaximumImprovisations`: platform safety ceiling.
- `TargetObjective`: published `Loss_obj`.
- `RehearsalHarmonyMemoryConsiderationRate`
- `RehearsalPitchAdjustmentRate`
- `InitialPitchAdjustmentBandwidthFractionOfRange`

## API example

The publication's Object PSF equation is minimization-specific; this API example therefore uses
`OptimizationSense.Minimize`.

```csharp
using MetaheuristicsPlatform.Algorithms.HarmonySearch;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

var problem =
    new ContinuousOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(
            dimension: 30,
            lowerBound: -5.0,
            upperBound: 5.0),
        OptimizationSense.Minimize,
        x =>
        {
            double sum = 0.0;

            for (int i = 0;
                 i < x.Length;
                 i++)
            {
                sum += x[i] * x[i];
            }

            return sum;
        });

var parameters =
    new AdvancedParameterSettingFreeHarmonySearchObjectParameters
    {
        HarmonyMemorySize = 50,
        MaximumImprovisations = 20_000,
        TargetObjective = 0.0,
        InitialPitchAdjustmentBandwidthFractionOfRange = 0.001
    };

OptimizationResult<double[]> result =
    new AdvancedParameterSettingFreeHarmonySearchObjectOptimizer()
        .Optimize(
            problem,
            parameters,
            new ArraySolutionCloner<double>(),
            new MaxEvaluationsStoppingCriterion(25_000),
            new OptimizationOptions
            {
                Seed = 123456UL
            });
```

## Stable factory ID

`advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020`

## Mathematical details

### Problem formulation

The published Object PSF scheme is used for bounded continuous minimization with a known target
objective:

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),
\f]

where `Loss_obj` is a known target value for the minimum and Harmony Memory contains HMS
candidate vectors in the bounded search domain.

### Update equations / iterations

After the first HMS rehearsal improvisations, let `Loss_start` be the mean objective value
currently stored in Harmony Memory, and let `Loss_mean` be its current mean.

For minimization, Equation (7) is

\f[
HMCR
=
0.5+
0.5\,\sigma\left(
10
-
10\frac{Loss_{mean}-Loss_{obj}}
        {Loss_{start}-Loss_{obj}}
-
\frac{5}{\ln D}
\right),
\f]

with

\f[
\sigma(z)=\frac{1}{1+e^{-z}}.
\f]

For \f$D=1\f$, the platform uses the explicit right-hand dimensional limit
\f$HMCR=0.5\f$.

Equation (8) is

\f[
PAR
=
HMCR\,
\sigma\left(
\frac{4}{D}-2
\right).
\f]

Let \f$Loss_i\f$ be the HM mean at HMS-sized block \f$i\f$. Equation (9) gives the adaptive
object bandwidth. When

\f[
(U-L)
\frac{Loss_{i-1}-Loss_i}
     {Loss_{start}-Loss_{obj}}
\ge
(U-L)\,0.0001,
\f]

the bandwidth is

\f[
b_i
=
(U-L)
\frac{Loss_{i-1}-Loss_i}
     {Loss_{start}-Loss_{obj}}.
\f]

Otherwise,

\f[
b_i
=
(U-L)
\left(
1-
\frac{Loss_{start}-Loss_i}
     {Loss_{start}-Loss_{obj}}
\right)
0.1.
\f]

For heterogeneous bounded boxes, the platform preserves the scalar loss-dependent fraction and
multiplies it coordinate-wise by each span \f$u_j-l_j\f$.

### Assumptions

- Equation (7) is used only for minimization.
- `TargetObjective` is finite.
- `Loss_start > Loss_obj` while adaptive Object PSF updates continue.
- Search-space coordinate spans are positive and finite.
- The first HMS improvisations use fixed rehearsal HMCR/PAR because the paper requests specific
  values but does not uniquely prescribe them.
- The platform defaults those rehearsal values to 0.5 and 0.5 and exposes them explicitly.
- Before Equation (9) is available, the initial bandwidth defaults to 0.001 of each coordinate
  range, following the paper's stated typical 0.1% full-range HS bandwidth.
- No Operation Type Matrix and no iteration-dependent HMCR equation are mixed into this identity.
- Replacement of the current worst harmony is strict under minimization.

### Convergence conditions

The 2020 publication proposes parameter adaptation mechanisms rather than a universal finite-time
global convergence theorem. The platform therefore does **not** claim a general convergence
guarantee beyond the implemented stopping conditions.

Execution terminates when one of the following occurs:

- the target objective is reached;
- a common platform stopping criterion requests termination;
- the configured `MaximumImprovisations` safety ceiling is reached.

Strict replace-worst under minimization makes the Harmony Memory mean nonincreasing whenever a
replacement occurs, which is compatible with the progress quantities used by Equation (9), but
this monotonicity is not presented as a proof of global convergence.

### Scientific references

Jeong, Y.-W.; Park, S.-M.; Geem, Z. W.; Sim, K.-B. (2020),
*Advanced Parameter-Setting-Free Harmony Search Algorithm*,
Applied Sciences 10(7), 2586.
DOI: `10.3390/app10072586`.
