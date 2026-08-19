@page threshold_accepting_dueck_scheuer_1990 Threshold Accepting - Dueck-Scheuer

# Threshold Accepting - Dueck-Scheuer

## General description

Threshold Accepting (TA) is the deterministic threshold-based trajectory metaheuristic
introduced by Dueck and Scheuer (1990). Rather than accepting worsening moves with a
temperature-dependent probability, TA accepts a worsening candidate whenever its
sense-aware objective degradation does not exceed the current threshold.

@subpage threshold_accepting_schedules

## Technical specifications

- Stable ID: `threshold-accepting-dueck-scheuer-1990`.
- Public optimizer:
  `ThresholdAcceptingOptimizer<TSolution,TMove,TUndo>`.
- Acceptance policy: `ThresholdAcceptancePolicy`.
- Neighborhood: generic `IStochasticNeighborhood<TSolution,TMove>`.
- Move mutation: generic reversible
  `IReversibleMoveOperator<TSolution,TMove,TUndo>`.
- Exact fast path: optional
  `IMoveObjectiveDeltaEvaluator<TSolution,TMove>`.
- Threshold schedules: linear, geometric, explicit non-increasing sequence, or custom.
- Common runtime: `OptimizationContext<TSolution>`.
- Acceptance is deterministic; stochasticity can still enter through neighborhood sampling.

## Complexity

For one attempted transition let \f$C_m\f$ be move application cost, \f$C_u\f$ undo
cost, \f$C_f\f$ full objective cost and \f$C_\Delta\f$ exact candidate-objective cost.

With exact move deltas,

\f[
O(C_\Delta+C_m)
\f]

is required for an accepted transition and \f$O(C_\Delta)\f$ for a rejected transition.

Without exact deltas the reversible fallback costs

\f[
O(C_m+C_f+C_u)
\f]

for a rejected transition and \f$O(C_m+C_f)\f$ for an accepted one.

Threshold acceptance itself is \f$O(1)\f$ time and \f$O(1)\f$ additional memory.

## Applicability

TA applies to finite or discretized search spaces, and more generally to any representation
that provides a stochastic move sampler and reversible move operator. It is particularly
attractive when objective evaluations or neighborhood operations dominate runtime and a
simple deterministic non-monotone local trajectory is desired.

## Detailed operation

The optimizer creates an initial solution and evaluates it through the common
`OptimizationContext`. Each iteration samples one move, evaluates the candidate through
the exact-delta fast path when available, and computes its sense-aware degradation.

Improving and equal moves are accepted. A worsening move is accepted only when its
degradation is at most the current threshold. After
`TransitionsPerThresholdLevel` attempted transitions, the configured monotone schedule
produces the next threshold. Generic stopping criteria, cancellation and callbacks remain
active throughout the run.

Every candidate objective consumes the common evaluation budget. A newly discovered
global best is registered with an owned solution snapshot exactly once.

## Parameters

`ThresholdAcceptingParameters` exposes:

- `InitialThreshold` — default `1.0`;
- `MinimumThreshold` — default `0.0`;
- `TransitionsPerThresholdLevel` — default `100`;
- `ThresholdSchedule` — default `Linear`;
- `LinearDecrement` — default `0.01`;
- `GeometricAlpha` — default `0.95`;
- `ExplicitThresholds` — required for `Explicit`;
- `CustomThresholdSchedule` — overrides built-in schedule selection;
- `StopAtMinimumThreshold` — default `true`;
- `MaximumConsecutiveSamplingFailures` — default `64`.

The platform's generic evaluation, iteration, time, target-fitness and no-improvement
stopping criteria remain available independently of the threshold controls.

## API example

```csharp
var algorithm =
    new ThresholdAcceptingOptimizer<
        MySolution,
        MyMove,
        MyUndo>(
            initialSolutionGenerator,
            stochasticNeighborhood,
            reversibleMoveOperator,
            exactDeltaEvaluator);

OptimizationResult<MySolution> result =
    algorithm.Optimize(
        problem,
        new ThresholdAcceptingParameters
        {
            InitialThreshold = 10.0,
            MinimumThreshold = 0.0,
            TransitionsPerThresholdLevel = 100,
            ThresholdSchedule =
                ThresholdAcceptingScheduleKind.Linear,
            LinearDecrement = 0.1
        },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`threshold-accepting-dueck-scheuer-1990`

The method requires domain composition, so users register a typed factory when they want
factory-based creation.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X}f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X}f(x).
\f]

### Update equations / iterations

Let \f$d_k\f$ denote sense-aware non-negative degradation. Then

\f[
\begin{aligned}
d_k
&=
\begin{cases}
\max\{0,f(x')-f(x_k)\}, & \text{minimization},\\
\max\{0,f(x_k)-f(x')\}, & \text{maximization},
\end{cases}\\
x_{k+1}
&=
\begin{cases}
x', & d_k\le\tau_\ell,\\
x_k, & d_k>\tau_\ell,
\end{cases}\\
\tau_{\ell+1}
&\le
\tau_\ell.
\end{aligned}
\f]

### Assumptions

- Candidate objective values are finite.
- The move operator and undo token are exact on the reversible fallback path.
- An optional delta evaluator returns the exact candidate objective.
- Built-in v0.33 threshold schedules are finite, non-negative and non-increasing.
- The stochastic neighborhood may fail to sample a move; repeated failure is bounded by
  `MaximumConsecutiveSamplingFailures`.

### Convergence conditions

The v0.33 implementation guarantees finite termination when a finite generic stopping
criterion fires, when the linear or explicit schedule reaches the configured minimum with
`StopAtMinimumThreshold=true`, or when the neighborhood is exhausted. The library does
not claim unconditional finite-time convergence to a global optimum. At zero threshold,
the acceptance rule becomes non-worsening hill climbing.

### Scientific references

- Dueck, G.; Scheuer, T. (1990).
  *Threshold accepting: A general purpose optimization algorithm appearing superior to simulated annealing*.
  Journal of Computational Physics 90(1), 161-175.
  DOI: `10.1016/0021-9991(90)90201-B`.
- Winker, P.; Fang, K.-T. (1997).
  *Application of Threshold-Accepting to the Evaluation of the Discrepancy of a Set of Points*.
  SIAM Journal on Numerical Analysis 34(5), 2028-2042.
  DOI: `10.1137/S0036142995286076`.
- Hu, T. C.; Kahng, A. B.; Tsao, C.-W. A. (1995).
  *Old Bachelor Acceptance: A New Class of Non-Monotone Threshold Accepting Methods*.
  ORSA Journal on Computing 7(4), 417-425.
  DOI: `10.1287/ijoc.7.4.417`.