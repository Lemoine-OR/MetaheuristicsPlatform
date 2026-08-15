@page simulated_annealing_metropolis Simulated Annealing

# Simulated Annealing

## General description

Generic reversible trajectory engine, Metropolis acceptance, pluggable cooling schedules, exact-delta fast path and common OptimizationContext lifecycle.

## Technical specifications

- **Stable factory ID:** `simulated-annealing-metropolis`
- **Implementation class:** `SimulatedAnnealingOptimizer<TSolution,TMove,TUndo>`
- **Family:** Trajectory-based methods
- **Source:** `src/MetaheuristicsPlatform/Algorithms/SA/SimulatedAnnealingOptimizer.cs`
- **Runtime creation:** explicit typed composition registration

## Complexity

- **Time:** O(C_move + C_eval) per attempted transition; O(C_delta) when an exact differential evaluator is available
- **Space:** O(|solution| + |move| + |undo|); no mandatory per-transition solution clone on the reversible path

## Applicability

Any solution representation admitting a stochastic neighborhood and reversible move operator; exact delta evaluation is optional

## Detailed operation

The implementation follows the cited scientific method while preserving the platform invariants: deterministic random streams where applicable, explicit ownership of mutable state, common stopping/callback lifecycle, and no avoidable hot-loop allocation.

## Parameters


Generic: seed, stopping criteria, callbacks, cancellation, solution cloner.

Specific: initial/minimum temperature, transitions per temperature level, cooling schedule, cooling parameters, neighborhood sampling-failure limit. The neighborhood, move operator and optional delta evaluator are supplied through composition.


## API example


```csharp
var optimizer =
    new SimulatedAnnealingOptimizer<MySolution, MyMove, MyUndo>(
        initialSolutionGenerator,
        neighborhood,
        reversibleMoveOperator,
        exactDeltaEvaluator);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.SimulatedAnnealing,
    () => optimizer,
    replace: true);
```


## Stable factory ID

```text
simulated-annealing-metropolis
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)\quad\text{or}\quad\max_{x\in\mathcal X} f(x)
\f]

### Update equations / iterations

\f[
P(x\rightarrow x')=\begin{cases}1,&\Delta\le 0\\\exp(-\Delta/T_k),&\Delta>0\end{cases},\qquad \Delta=\begin{cases}f(x')-f(x),&\min\\f(x)-f(x'),&\max\end{cases}
\f]

### Assumptions

The neighborhood must be sampleable and moves must be reversible for the zero-clone fast path. Cooling schedule assumptions depend on the chosen schedule.

### Convergence conditions

Classical asymptotic global-convergence theorems require sufficiently slow logarithmic cooling under irreducibility/communication assumptions. The practical geometric default does not claim that theorem.

### Scientific references

Metropolis et al. (1953), Journal of Chemical Physics 21(6), 1087–1092; Kirkpatrick, Gelatt & Vecchi (1983), Science 220(4598), 671–680

DOI: `10.1126/science.220.4598.671`

## Scientific cooling catalog

Version 0.20.0 provides **10 built-in executable cooling laws** with stable `sa.cooling.*` IDs, plus explicit literature review of broader controllers that cannot be faithfully represented by a scalar level-only temperature rule.

See [Simulated Annealing Scientific Cooling Catalog](../components/simulated-annealing-cooling-schedules.md) for formulas, assumptions, asymptotic behavior, implementation scope and primary references.

The catalog distinguishes a **temperature component** from a complete published annealing algorithm: Szu-Hartley FSA also specifies a Cauchy visiting distribution; Ingber VFSR/ASA includes re-annealing and parameter adaptation; Tsallis-Stariolo GSA changes visiting and acceptance distributions; and Huang et al. additionally control chain length and freezing.

## Scientific references

- Metropolis et al. (1953), Journal of Chemical Physics 21(6), 1087–1092; Kirkpatrick, Gelatt & Vecchi (1983), Science 220(4598), 671–680
- DOI: `10.1126/science.220.4598.671`
