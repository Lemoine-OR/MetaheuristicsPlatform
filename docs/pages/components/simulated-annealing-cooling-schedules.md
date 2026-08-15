@page simulated_annealing_cooling_schedules Simulated Annealing Scientific Cooling Catalog

# Simulated Annealing Scientific Cooling Catalog

## Purpose and scientific scope

MetaheuristicsPlatform v0.20.0 separates **temperature laws** from complete annealing algorithms. A formula that appears inside Fast Simulated Annealing (FSA), Very Fast Simulated Re-Annealing / Adaptive Simulated Annealing (VFSR/ASA), or Generalized Simulated Annealing (GSA) is exposed as a cooling component only when the rest of the original algorithm is not implemented. This prevents a temperature formula from being mislabeled as the complete published method.

The executable catalog contains **10 built-in cooling laws**. Three additional influential controllers are reviewed but deliberately not exposed through the level-only cooling interface because doing so would discard essential state or per-transition feedback.

Stable IDs are part of the public scientific catalog and are intended for configuration, experiment metadata, reproducibility and documentation.

## Common notation

Let \f$T_k>0\f$ denote the temperature at level \f$k\f$, \f$T_0\f$ the configured initial temperature, and \f$k\f$ the number of completed fixed-temperature levels. For statistical schedules, \f$\sigma_k\f$ and \f$\operatorname{Var}_k\f$ are the empirical population standard deviation and variance of objective-state samples observed during the just-completed level.

The Metropolis acceptance law used by the optimizer is

\f[
P(\text{accept}\mid\Delta,T)=
\begin{cases}
1,&\Delta\le 0,\\
\exp(-\Delta/T),&\Delta>0.
\end{cases}
\f]

The cooling catalog controls only the evolution of \f$T\f$; it does not change the neighborhood, visiting distribution or acceptance law unless a future, explicitly named annealing algorithm implements those mechanisms.

## Executable catalog

### `sa.cooling.geometric` — Geometric / multiplicative cooling

**Implementation:** `GeometricCoolingSchedule`

\f[
T_{k+1}=\alpha T_k,\qquad 0<\alpha<1.
\f]

This is the practical default retained from v0.18/v0.19. It is fast, stateless and allocation-free. A fixed geometric factor is **not** presented as satisfying the logarithmic global-convergence theorem.

Reference context: Kirkpatrick, Gelatt & Vecchi (1983), *Optimization by Simulated Annealing*, Science 220(4598), 671–680, DOI `10.1126/science.220.4598.671`.

### `sa.cooling.linear` — Linear / additive cooling

**Implementation:** `LinearCoolingSchedule`

\f[
T_{k+1}=\max(0,T_k-\beta),\qquad \beta>0.
\f]

Linear cooling is a classical finite-length policy. It is useful when a finite temperature horizon is desired, but the library does not attach the asymptotic logarithmic global-convergence theorem to it. Strenski & Kirkpatrick's finite-length analysis is important context because their experiments show that optimal finite schedules need not even be monotone and compare practical schedules under explicit finite budgets.

Reference context: Strenski & Kirkpatrick (1991), *Analysis of finite length annealing schedules*, Algorithmica 6(3), 346–366.

### `sa.cooling.lundy-mees-1986` — Lundy–Mees

**Implementation:** `LundyMeesCoolingSchedule`

\f[
T_{k+1}=\frac{T_k}{1+\beta T_k},\qquad \beta>0.
\f]

For constant \f$\beta\f$,

\f[
T_k=\frac{T_0}{1+k\beta T_0}.
\f]

Reference: Lundy & Mees (1986), *Convergence of an annealing algorithm*, Mathematical Programming 34(1), 111–124, DOI `10.1007/BF01582166`.

### `sa.cooling.hajek-1988` — Normalized logarithmic cooling

**Implementation:** `HajekLogarithmicCoolingSchedule`

The library uses the shifted normalization

\f[
T_k=T_0\frac{\ln 2}{\ln(k+2)}.
\f]

It is a member of the classical \f$c/\ln(1+t)\f$ family and preserves the configured initial temperature at artificial time one. Hajek's theorem is more specific: under the theorem's communication assumptions, the scale constant must be large enough relative to the critical depth of the deepest non-global local minimum. Consequently, **choosing this enum value does not by itself assert that the problem-specific theorem hypotheses are satisfied**.

Reference: Hajek (1988), *Cooling Schedules for Optimal Annealing*, Mathematics of Operations Research 13(2), 311–329, DOI `10.1287/moor.13.2.311`. Related logarithmic convergence context: Geman & Geman (1984), DOI `10.1109/TPAMI.1984.4767596`.

### `sa.cooling.szu-hartley-1987` — Fast / Cauchy temperature law

**Implementation:** `SzuHartleyFastCauchyCoolingSchedule`

\f[
T_k=\frac{T_0}{k+1}.
\f]

Szu & Hartley's FSA combines inverse-linear cooling with a heavy-tailed Cauchy visiting distribution. MetaheuristicsPlatform v0.20 implements **only the temperature law**; the ordinary generic SA neighborhood is not silently relabeled as the Cauchy visiting process.

Reference: Szu & Hartley (1987), *Fast simulated annealing*, Physics Letters A 122(3–4), 157–162, DOI `10.1016/0375-9601(87)90796-1`.

### `sa.cooling.ingber-vfsr-1989` — Ingber very-fast temperature law

**Implementation:** `IngberVeryFastCoolingSchedule`

\f[
T_k=T_0\exp\!\left(-c k^{1/D}\right),
\qquad c>0,\ D\ge 1.
\f]

The dimension \f$D\f$ is explicit. The original VFSR/ASA framework also contains parameter-specific generating temperatures, sensitivity adaptation and re-annealing. Those mechanisms are **not** implied by selecting this cooling component.

Reference: Ingber (1989), *Very fast simulated re-annealing*, Mathematical and Computer Modelling 12(8), 967–973, DOI `10.1016/0895-7177(89)90202-1`.

### `sa.cooling.tsallis-stariolo-1996` — Generalized visiting-temperature law

**Implementation:** `TsallisStarioloGeneralizedCoolingSchedule`

For artificial time \f$t\ge1\f$,

\f[
T_q(t)=T_q(1)
\frac{2^{q-1}-1}{(1+t)^{q-1}-1}.
\f]

The implementation accepts \f$1\le q<3\f$, handles the \f$q\to1\f$ logarithmic limit explicitly, and recovers inverse-linear Fast SA cooling at \f$q=2\f$. Full GSA additionally changes the visiting and acceptance distributions; v0.20 does not conflate those mechanisms with the temperature law.

Reference: Tsallis & Stariolo (1996), *Generalized simulated annealing*, Physica A 233(1–2), 395–406, DOI `10.1016/S0378-4371(96)00271-3`.

### `sa.cooling.aarts-van-laarhoven-1985` — Statistical cooling

**Implementation:** `AartsVanLaarhovenStatisticalCoolingSchedule`

\f[
T_{k+1}=
\frac{T_k}
{1+T_k\ln(1+\delta)/(3\sigma_k)},
\qquad \delta>0.
\f]

This schedule adapts the decrement to observed objective fluctuations. The optimizer therefore enables an allocation-free Welford accumulator only for schedules implementing `ISimulatedAnnealingStatisticalCoolingSchedule`. With zero empirical variance, the implementation reports a frozen next temperature (`0`), which is then subjected to the optimizer's configured minimum-temperature floor.

Reference: Aarts & van Laarhoven (1985), *Statistical cooling: a general approach to combinatorial optimization problems*, Philips Journal of Research 40(4), 193–226.

### `sa.cooling.huang-romeo-sangiovanni-1986` — Statistical decrement

**Implementation:** `HuangStatisticalCoolingSchedule`

\f[
T_{k+1}=T_k
\exp\!\left(-\lambda\frac{T_k}{\sigma_k}\right),
\qquad 0<\lambda\le1.
\f]

The 1986 Huang–Romeo–Sangiovanni-Vincentelli schedule is broader than this expression: it also adapts Markov-chain length and detects the frozen condition. v0.20 exposes only the statistical temperature-decrement component and marks it as such in metadata.

Reference: Huang, Romeo & Sangiovanni-Vincentelli (1986), *An Efficient General Cooling Schedule for Simulated Annealing*, IEEE ICCAD, 381–384.

### `sa.cooling.triki-collette-siarry-2005` — Variance-driven adaptive cooling

**Implementation:** `TrikiAdaptiveCoolingSchedule`

\f[
T_{k+1}=T_k
\left(1-\frac{T_k\Delta}{\operatorname{Var}_k}\right),
\qquad \Delta>0.
\f]

The update uses the empirical objective variance. If finite-sample statistics would make the next physical temperature non-positive, the implementation saturates the raw update at zero and lets the optimizer apply `MinimumTemperature`.

Reference: Triki, Collette & Siarry (2005), *A theoretical study on the behavior of simulated annealing leading to a new cooling schedule*, European Journal of Operational Research 166(1), 77–92, DOI `10.1016/j.ejor.2004.03.035`.

## Reviewed controllers intentionally not reduced to a scalar level law

### `sa.cooling.otten-van-ginneken` — Otten–van Ginneken adaptive control

Otten & van Ginneken couple the temperature decrement to objective fluctuations, a cost-scale estimate and a dynamically determined Markov-chain length. Because the chain length is part of the controller, reproducing only its scalar decrement while leaving `TransitionsPerTemperatureLevel` fixed would change the method. It is therefore catalogued as **reviewed-composite**.

Reference context: Otten & van Ginneken, *The Annealing Algorithm* (1989), DOI `10.1007/978-1-4613-1627-5`, together with their earlier floorplanning annealing-control work.

### `sa.cooling.lam-delosme-1988` — Lam–Delosme

Lam & Delosme's schedule uses adaptive feedback at move granularity, with acceptance/statistical control that is richer than the current fixed-level `ISimulatedAnnealingCoolingSchedule` contract. Implementing a look-alike `T_{k+1}` formula would lose essential parts of the published method. It is therefore catalogued as **reviewed-composite**, not implemented.

Reference: Lam & Delosme (1988), *Performance of a New Annealing Schedule*, 25th ACM/IEEE Design Automation Conference, 306–311, DOI `10.1109/DAC.1988.14775`.

### `sa.cooling.salamon-constant-thermodynamic-speed-1988` — Constant thermodynamic speed

Salamon et al. formulate annealing as thermodynamic-speed control using quantities such as relaxation time and energy-fluctuation/heat-capacity information. These quantities are not represented by the current cooling context. The method is therefore **reviewed-composite** until a dedicated thermodynamic controller contract can represent it without approximation.

Reference: Salamon et al. (1988), *Simulated Annealing with Constant Thermodynamic Speed*, Computer Physics Communications 49(3), 423–428, DOI `10.1016/0010-4655(88)90003-3`.

## Why some famous methods are components rather than new SA algorithms

The following implications are deliberately rejected:

- selecting `sa.cooling.szu-hartley-1987` does **not** install the original Cauchy visiting distribution;
- selecting `sa.cooling.ingber-vfsr-1989` does **not** activate VFSR/ASA re-annealing or parameter sensitivity adaptation;
- selecting `sa.cooling.tsallis-stariolo-1996` does **not** replace the generic Metropolis acceptance law with generalized Tsallis acceptance;
- selecting `sa.cooling.huang-romeo-sangiovanni-1986` does **not** reproduce the full dynamic chain-length and freezing controller.

This distinction is part of the API/documentation contract and is machine-readable through `SimulatedAnnealingCoolingScheduleDescriptor.IsComponentOfBroaderAnnealingAlgorithm`.

## Runtime API

Built-in schedules are selected through `SimulatedAnnealingCoolingScheduleKind`:

```csharp
var parameters = new SimulatedAnnealingParameters
{
    InitialTemperature = 100.0,
    MinimumTemperature = 1e-9,
    TransitionsPerTemperatureLevel = 250,
    CoolingSchedule =
        SimulatedAnnealingCoolingScheduleKind.HajekLogarithmic
};
```

Adaptive statistical schedule:

```csharp
var parameters = new SimulatedAnnealingParameters
{
    InitialTemperature = 100.0,
    MinimumTemperature = 1e-9,
    TransitionsPerTemperatureLevel = 500,
    CoolingSchedule =
        SimulatedAnnealingCoolingScheduleKind.AartsVanLaarhovenStatistical,
    AartsVanLaarhovenDelta = 0.1
};
```

External research schedules remain possible without changing the enum:

```csharp
var parameters = new SimulatedAnnealingParameters
{
    InitialTemperature = 100.0,
    MinimumTemperature = 1e-9,
    CustomCoolingSchedule = myCoolingSchedule
};
```

`CustomCoolingSchedule`, when non-null, has precedence over the built-in enum. A custom schedule may implement `ISimulatedAnnealingStatisticalCoolingSchedule` to request per-level objective statistics.

## Complexity and allocation policy

For deterministic closed-form schedules, computing the next temperature is \f$O(1)\f$ time and \f$O(1)\f$ space. For statistical schedules, each attempted transition adds one Welford update in \f$O(1)\f$ time and \f$O(1)\f$ state. No collection of all objective samples is allocated.

Therefore the existing non-statistical SA hot path incurs **no objective-statistics accumulation** merely because statistical schedules exist in the library.

## Validation policy

The repository validation checks that:

- exactly ten built-in executable schedules are registered;
- stable schedule IDs are unique;
- every implemented catalog entry maps to an enum member, runtime descriptor and source file;
- every reviewed-composite method remains explicitly non-implemented;
- the scientific page contains every stable ID and all available DOI metadata;
- `version.json` is `0.20.0` for this pack.

## Literature reviewed

1. Metropolis et al. (1953), *Equation of State Calculations by Fast Computing Machines*, DOI `10.1063/1.1699114`.
2. Kirkpatrick, Gelatt & Vecchi (1983), *Optimization by Simulated Annealing*, DOI `10.1126/science.220.4598.671`.
3. Geman & Geman (1984), *Stochastic Relaxation, Gibbs Distributions, and the Bayesian Restoration of Images*, DOI `10.1109/TPAMI.1984.4767596`.
4. Aarts & van Laarhoven (1985), *Statistical cooling: a general approach to combinatorial optimization problems*.
5. Lundy & Mees (1986), *Convergence of an annealing algorithm*, DOI `10.1007/BF01582166`.
6. Huang, Romeo & Sangiovanni-Vincentelli (1986), *An Efficient General Cooling Schedule for Simulated Annealing*.
7. Szu & Hartley (1987), *Fast simulated annealing*, DOI `10.1016/0375-9601(87)90796-1`.
8. Hajek (1988), *Cooling Schedules for Optimal Annealing*, DOI `10.1287/moor.13.2.311`.
9. Lam & Delosme (1988), *Performance of a New Annealing Schedule*, DOI `10.1109/DAC.1988.14775`.
10. Salamon et al. (1988), *Simulated Annealing with Constant Thermodynamic Speed*, DOI `10.1016/0010-4655(88)90003-3`.
11. Otten & van Ginneken (1989), *The Annealing Algorithm*, DOI `10.1007/978-1-4613-1627-5`.
12. Ingber (1989), *Very fast simulated re-annealing*, DOI `10.1016/0895-7177(89)90202-1`.
13. Strenski & Kirkpatrick (1991), *Analysis of finite length annealing schedules*, Algorithmica 6(3), 346–366.
14. Tsallis & Stariolo (1996), *Generalized simulated annealing*, DOI `10.1016/S0378-4371(96)00271-3`.
15. Cohn & Fielding (1999), *Simulated Annealing: Searching for an Optimal Temperature Schedule*, DOI `10.1137/S1052623497329683` — important analysis of why convergent schedules can be impractically slow and why alternative temperature regimes may be justified.
16. Triki, Collette & Siarry (2005), *A theoretical study on the behavior of simulated annealing leading to a new cooling schedule*, DOI `10.1016/j.ejor.2004.03.035`.
