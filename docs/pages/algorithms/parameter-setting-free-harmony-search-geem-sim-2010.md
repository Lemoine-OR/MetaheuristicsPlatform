@page parameter_setting_free_harmony_search_geem_sim_2010 Parameter-Setting-Free Harmony Search - Geem and Sim 2010

# Parameter-Setting-Free Harmony Search - Geem and Sim 2010

## General description

Parameter-Setting-Free Harmony Search (PSF-HS) was proposed by Geem and Sim in 2010 to
remove the manual tuning burden for the two probabilistic HS controls HMCR and PAR.

PSF-HS adds an Operation Type Matrix (OTM), with the same HMS-by-dimension shape as the
Harmony Memory variables. Each OTM cell records whether the corresponding stored variable was
created by random selection, memory consideration, or pitch adjustment. After a rehearsal stage,
each variable receives its own HMCR and PAR derived from the operation types currently surviving
in Harmony Memory.

"Parameter-setting-free" therefore has a precise scope: HMCR and PAR are learned from OTM.
HMS, the stopping budget, rehearsal duration and problem-scale pitch bandwidth still exist.

## Technical specifications

- Stable ID: `parameter-setting-free-harmony-search-geem-sim-2010`
- Class: `ParameterSettingFreeHarmonySearchOptimizer`
- Parameters: `ParameterSettingFreeHarmonySearchParameters`
- State: `ParameterSettingFreeHarmonySearchState`
- Operation type: `ParameterSettingFreeHarmonySearchOperationType`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.60.0
- Primary DOI: `10.1016/j.amc.2010.09.049`

## Complexity

Let \f$H\f$ be HMS and \f$D\f$ the dimension. HM and OTM each require \f$O(HD)\f$ memory.
An improvisation costs \f$O(D+H)\f$ plus one objective evaluation, while recomputing all
variable-specific HMCR/PAR values from OTM costs \f$O(HD)\f$. The direct v0.60 implementation
recomputes these rates before each performance improvisation for transparency and exactness.

## Applicability

PSF-HS applies naturally to bounded derivative-free problems where the ordinary HS random,
memory and pitch operations are meaningful. v0.60 exposes a continuous bounded identity.

The conventional PSF literature commonly starts rehearsal with HMCR = PAR = 0.5 and uses a
small number of HMS-sized rehearsal cycles; a three-HMS rehearsal is retained as the platform
default. The pitch bandwidth remains problem-scale information. The platform exposes it as a
fraction of each coordinate range; default 0.001 is a convenience setting, not a universal
Geem-Sim theorem.

No external numerical library is required; System.Math and standard .NET primitives suffice.

## Detailed operation

1. Random tuning: fill Harmony Memory uniformly and initialize every OTM cell as `RandomSelection`.
2. Rehearsal: use fixed HMCR = 0.5 and PAR = 0.5.
3. For every generated coordinate, record exactly one operation type:
   `RandomSelection`, `MemoryConsideration`, or `PitchAdjustment`.
4. Evaluate the candidate and, only if it strictly improves the current worst harmony, replace
   that harmony and replace the corresponding OTM row by the candidate's operation types.
5. After rehearsal, for each variable \f$i\f$, compute its HMCR and PAR from the current OTM.
6. Improvise the next harmony using those variable-specific probabilities.
7. When a successful candidate enters HM, update its OTM row at the same time.
8. Recompute the adaptive rates from the new OTM and continue.

## Parameters

- `HarmonyMemorySize`: HMS; default 30.
- `MaximumImprovisations`: total rehearsal + performance improvisations.
- `RehearsalMemoryCycles`: rehearsal duration in multiples of HMS; default 3.
- `PitchAdjustmentBandwidthFractionOfRange`: coordinate-wise pitch scale.

Scientific constants:
- rehearsal HMCR = 0.5;
- rehearsal PAR = 0.5.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ParameterSettingFreeHarmonySearchOptimizer>(
        MetaheuristicAlgorithmIds.ParameterSettingFreeHarmonySearch);

var parameters =
    new ParameterSettingFreeHarmonySearchParameters
    {
        HarmonyMemorySize = 30,
        RehearsalMemoryCycles = 3,
        PitchAdjustmentBandwidthFractionOfRange = 0.001,
        MaximumImprovisations = 3000
    };
```

## Stable factory ID

`parameter-setting-free-harmony-search-geem-sim-2010`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D}f(x),
\qquad
\mathcal X=\prod_{i=1}^{D}[l_i,u_i].
\f]

### Update equations / iterations

For variable \f$i\f$, let the OTM column contain operation labels
\f$y_i^j\in\{Random,Memory,Pitch\}\f$, \f$j=1,\ldots,H\f$. Then

\f[
HMCR_i
=
\frac{
\#\{j:y_i^j=Memory\ \text{or}\ Pitch\}
}{H},
\f]

and

\f[
PAR_i
=
\frac{
\#\{j:y_i^j=Pitch\}
}{
\#\{j:y_i^j=Memory\ \text{or}\ Pitch\}
}.
\f]

The improvisation probabilities are therefore

\f[
\begin{aligned}
P(Random_i) &= 1-HMCR_i,\\
P(Memory_i) &= HMCR_i(1-PAR_i),\\
P(Pitch_i) &= HMCR_iPAR_i.
\end{aligned}
\f]

A pitch-adjusted continuous coordinate is represented as

\f[
x_i^{new}=x_i^{HM}+U(-1,1)\,bw_i,
\qquad
bw_i=\beta(u_i-l_i).
\f]

If the denominator of \f$PAR_i\f$ is zero, the same OTM column implies \f$HMCR_i=0\f$, so
the pitch branch is unreachable. The platform sets \f$PAR_i=0\f$ in this corner case. This is
an explicit defensive completion of an otherwise undefined 0/0, not a new PSF search rule.

### Assumptions

The OTM always moves in lock-step with Harmony Memory replacement. Initial random harmonies
carry `RandomSelection` labels. Only a strictly better candidate replaces the current worst
harmony and its OTM row.

The pitch bandwidth is not learned by conventional PSF-HS and remains problem dependent.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. Conventional PSF-HS can
drive variable-specific HMCR/PAR toward 0 or 1 as the OTM composition becomes homogeneous;
this known behavior motivates later advanced PSF schemes but is preserved here rather than
silently corrected.

### Scientific references

Geem, Z. W.; Sim, K.-B. (2010),
*Parameter-setting-free harmony search algorithm*,
Applied Mathematics and Computation 217(8), 3881-3889.
DOI: `10.1016/j.amc.2010.09.049`.
