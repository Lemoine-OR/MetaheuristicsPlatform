@page reactive_tabu_search_battiti_tecchiolli_1994 Reactive Tabu Search

# Reactive Tabu Search

Stable ID: `reactive-tabu-search-battiti-tecchiolli-1994`

## General description

Reactive Tabu Search (RTS), proposed by Roberto Battiti and Giampietro Tecchiolli, augments
Tabu Search with explicit detection of repeated configurations and a feedback mechanism that
learns an appropriate prohibition period while the search runs. When repetitions persist, an
escape phase performs random moves whose length is tied to a moving average of detected cycle
lengths.

The v0.22 implementation exposes RTS as a separate public algorithm instead of silently
changing `tabu-search-glover`. It reuses the same allocation-free neighborhood, reversible move,
exact-delta, aspiration, callback and stopping infrastructure.

## Technical specifications

- **Stable factory ID:** `reactive-tabu-search-battiti-tecchiolli-1994`
- **Implementation class:** `ReactiveTabuSearchOptimizer<TSolution,TMove,TUndo,TAttribute,TEnumerator>`
- **Family:** trajectory-based / local search
- **Mechanisms:** neighborhood, trajectory, memory-based, adaptive
- **Configuration memory:** expected-O(1) `ConfigurationRepetitionMemory`
- **Reactive control:** `ReactiveTabuTenurePolicy`
- **Escape:** allocation-free reservoir sampling of applicable moves
- **Optional longer-term memory:** `AttributeFrequencyMemory<TAttribute>`
- **Optional intensification:** elite restart
- **Source:** `src/MetaheuristicsPlatform/Algorithms/TS/ReactiveTabuSearchOptimizer.cs`

## Complexity

For a normal best-admissible iteration with exact deltas,

\f[
O(|N(x)|\,C_\Delta+\log M_s)
\f]

where \f$M_s\f$ is the active short-term tabu memory. Hash lookups for repetition and frequency
memory are expected \f$O(1)\f$.

Without exact deltas,

\f[
O(|N(x)|(C_{\mathrm{apply}}+C_f+C_{\mathrm{undo}})+\log M_s).
\f]

A reactive escape move performs one reservoir scan \f$O(|N(x)|)\f$ and evaluates only the
uniformly selected applicable move. Memory is

\f[
O(M_s+M_f+M_r+|x|),
\f]

with short-term tabu records \f$M_s\f$, distinct frequency attributes \f$M_f\f$, repetition
signatures \f$M_r\f$, and owned elite/current solutions.

## Applicability

RTS requires:

1. a finite enumerated neighborhood;
2. a reversible move operator;
3. domain-defined tabu attributes;
4. a stable configuration signature provider;
5. an objective evaluator, optionally with exact move deltas.

The signature provider owns the semantic guarantee that equal configurations receive equal
signatures. The library does not pretend that an arbitrary 64-bit hash is collision-free.

## Detailed operation

After each selected move, the resulting configuration signature is observed in hash memory. A
repetition yields a cycle length. The reactive tenure controller increases the prohibition
period on repetition and can decrease it after a sustained interval without repetition.

A moving average of cycle length is maintained. After a configured number of repetition
events, the controller requests an escape phase. Escape moves are sampled uniformly from the
applicable neighborhood by reservoir sampling, bypassing best-admissible ranking while still
recording short-term tabu attributes and visited configurations.

Optional frequency diversification adds a linear penalty to frequently selected candidate
attributes. Optional intensification restarts the current trajectory from the owned global-best
solution after configured stagnation while retaining longer-term frequency/repetition
knowledge.

## Parameters

`ReactiveTabuSearchParameters` exposes:

- `InitialTabuTenure` (default 1);
- `MinimumTabuTenure` / `MaximumTabuTenure`;
- `TenureIncreaseFactor` / `TenureDecreaseFactor`;
- `TenureDecreaseAfterIterationsWithoutRepetition`;
- `CycleLengthMovingAverageAlpha`;
- `DiversificationRepetitionThreshold`;
- `DiversificationCycleMultiplier`;
- `MaximumDiversificationMoves`;
- `FrequencyPenaltyWeight` (0 disables frequency bias);
- `IntensificationAfterIterationsWithoutImprovement` (0 disables elite restart);
- aspiration and custom reactive-controller extension points;
- initial memory capacity.

## API example

```csharp
var rts = new ReactiveTabuSearchOptimizer<
    MySolution,
    MyMove,
    MyUndo,
    MyTabuAttribute,
    MyMoveEnumerator>(
        initialSolutionGenerator,
        neighborhood,
        reversibleMoveOperator,
        tabuAttributeProvider,
        solutionSignatureProvider,
        exactDeltaEvaluator);

var parameters = new ReactiveTabuSearchParameters
{
    InitialTabuTenure = 1,
    MaximumTabuTenure = 64,
    DiversificationRepetitionThreshold = 3,
    FrequencyPenaltyWeight = 0.0
};

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.ReactiveTabuSearch,
    () => rts,
    replace: true);
```

## Stable factory ID

The canonical stable ID is:

`reactive-tabu-search-battiti-tecchiolli-1994`

Because RTS requires domain components, runtime creation uses explicit typed composition
registration in the same way as generic Simulated Annealing and the Glover TS foundation.

## Mathematical details

### Problem formulation

For minimization,

\f[
\min_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

Normal search uses the best admissible move

\f[
m_k^*\in\arg\min_{m\in\mathcal M_k(x_k)}
\widetilde f_k(m),
\qquad
x_{k+1}=m_k^*(x_k),
\f]

where \f$\widetilde f_k=f\f$ when frequency bias is disabled.

If signature \f$h(x_k)\f$ was last observed at \f$j<k\f$, the cycle length is

\f[
L_k=k-j.
\f]

The reactive prohibition period grows on detected repetition and decreases after sustained
absence of repetition evidence. Persistent repetition requests an escape length based on the
moving average \f$\overline L_k\f$:

\f[
n_{\mathrm{esc},k}
=
\left\lceil
\gamma\,\overline L_k
\right\rceil .
\f]

### Assumptions

Neighborhood enumeration must be finite. Tabu attributes and solution signatures must have
stable domain semantics. Any exact delta evaluator must match full objective evaluation.
Reactive-feedback constants and frequency-penalty weights are problem/configuration choices,
not universal constants from the literature.

### Convergence conditions

RTS is an adaptive memory-based metaheuristic. The implementation does not claim a universal
finite-time global-optimum theorem. Its purpose is to detect cycling, adapt short-term memory
online, and trigger diversification when the trajectory becomes excessively repetitive.

### Scientific references

- Battiti, R. & Tecchiolli, G. (1994), *The Reactive Tabu Search*, ORSA Journal
  on Computing 6(2), 126-140. DOI `10.1287/ijoc.6.2.126`.
- Glover, F. (1989), *Tabu Search-Part I*, ORSA Journal on Computing 1(3),
  190-206. DOI `10.1287/ijoc.1.3.190`.
- Glover, F. (1990), *Tabu Search-Part II*, ORSA Journal on Computing 2(1),
  4-32. DOI `10.1287/ijoc.2.1.4`.
- Glover, F. & Laguna, M. (1997), *Tabu Search*. DOI
  `10.1007/978-1-4615-6089-0`.
