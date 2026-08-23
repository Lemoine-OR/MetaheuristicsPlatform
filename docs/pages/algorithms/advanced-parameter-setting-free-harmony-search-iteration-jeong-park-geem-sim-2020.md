@page advanced_parameter_setting_free_harmony_search_iteration_jeong_park_geem_sim_2020 Advanced Parameter-Setting-Free Harmony Search - Iteration Scheme - Jeong et al. 2020

# Advanced Parameter-Setting-Free Harmony Search - Iteration Scheme

## General description

Jeong, Park, Geem and Sim (2020) proposed an advanced parameter-setting-free (PSF) scheme
to avoid the conventional PSF Operation Type Matrix and its tendency to drive learned
probabilities to 0 or 1.

The publication defines two advanced schemes. This v0.61.0 identity implements only the
**iteration PSF scheme**, in which HMCR is a sigmoid function of current/max improvisation and
problem dimension, and PAR is derived from HMCR and dimension. The paper's object-dependent
scheme, including object-dependent bandwidth, is intentionally not mixed into this identity.

Unlike conventional PSF-HS, the iteration advanced scheme requires no additional operation
memory.

## Technical specifications

- Stable ID: `advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020`
- Class: `AdvancedParameterSettingFreeHarmonySearchIterationOptimizer`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.61.0
- Primary DOI: `10.3390/app10072586`
- External mathematics packages: none

## Complexity

Let HMS be \f$H\f$ and dimension be \f$D\f$. Harmony Memory is \f$O(HD)\f$. HMCR/PAR
calculation is \f$O(1)\f$ per improvisation and candidate construction is \f$O(D)\f$.
Best/worst scans are \f$O(H)\f$. No OTM of size \f$H\times D\f$ is allocated.

## Applicability

The scheme is appropriate when a maximum improvisation budget is known. The paper explicitly
distinguishes this from its object PSF scheme, which requires a target objective value.

Pitch bandwidth is not made parameter-free by the iteration scheme. The paper states that its
object-dependent adaptive bandwidth method is possible only with the object-based HMCR. The
platform therefore exposes a fixed coordinate-range fraction for iteration PSF. The default
0.001 corresponds to the typical 0.1% full-range HS scale discussed by the paper; it is not
claimed to be universally optimal.

## Detailed operation

1. Initialize HMS bounded harmonies uniformly.
2. For one-based improvisation \f$t=1,\ldots,NI\f$, compute iteration-dependent HMCR.
3. Compute PAR from current HMCR and dimension.
4. Generate each coordinate by ordinary HS memory consideration or random selection.
5. Apply pitch adjustment only inside the memory branch.
6. Apply the platform's bounded-domain clamp after pitch perturbation.
7. Evaluate exactly once.
8. Strictly replace the current worst harmony only if the new candidate is better.
9. Continue until common stopping or NI.

## Parameters

- `HarmonyMemorySize`: HMS.
- `MaximumImprovisations`: NI used directly by Equation (5).
- `PitchAdjustmentBandwidthFractionOfRange`: fixed problem-scale pitch bandwidth fraction.

There are no manually set HMCR or PAR parameters and there is no Operation Type Matrix.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<
        AdvancedParameterSettingFreeHarmonySearchIterationOptimizer>(
            MetaheuristicAlgorithmIds
                .AdvancedParameterSettingFreeHarmonySearchIteration);
```

## Stable factory ID

`advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\prod_{j=1}^{D}[l_j,u_j]} f(x).
\f]

### Update equations / iterations

With one-based current improvisation \f$t\f$, maximum improvisations \f$NI\f$, and
dimension \f$D>1\f$,

\f[
HMCR(t)
=
0.5+
0.5\,\sigma\left(
10\frac{t}{NI}
-
\frac{5}{\ln D}
\right),
\f]

where

\f[
\sigma(z)=\frac{1}{1+e^{-z}}.
\f]

The PAR is

\f[
PAR(t)
=
HMCR(t)\,
\sigma\left(
\frac{4}{D}-2
\right).
\f]

For \f$D=1\f$, the published HMCR expression contains \f$\ln 1=0\f$. The platform uses the
right-hand dimensional limit \f$D\to1^+\f$, for which the sigmoid argument tends to
\f$-\infty\f$ and therefore \f$HMCR=0.5\f$. This is an explicit mathematical completion of
an undefined endpoint, not a new search rule.

Pitch adjustment is ordinary continuous HS:

\f[
x_j^{new}
=
x_j^{HM}
+
U(-1,1)\,\beta(u_j-l_j)
\f]

when both memory consideration and pitch adjustment branches fire.

### Assumptions

A positive finite coordinate span is required. The iteration scheme requires a positive NI.
The paper's object-dependent HMCR and Equation (9) bandwidth are excluded from this identity.

### Convergence conditions

The paper motivates HMCR increasing toward exploitation as the iteration budget is consumed
and decreases PAR as dimension grows. No universal finite-time global-convergence guarantee
is asserted here.

### Scientific references

Jeong, Y.-W.; Park, S.-M.; Geem, Z. W.; Sim, K.-B. (2020),
*Advanced Parameter-Setting-Free Harmony Search Algorithm*,
Applied Sciences 10(7), 2586.
DOI: `10.3390/app10072586`.
