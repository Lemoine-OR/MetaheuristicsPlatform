@page threshold_accepting_schedules Threshold Accepting Schedules and Acceptance Controls

# Threshold Accepting Schedules and Acceptance Controls

## Scope

Version 0.33.0 introduces the canonical Dueck-Scheuer **Threshold Accepting (TA)**
trajectory and a small, explicit catalog of monotone threshold schedules. The
algorithm deliberately separates:

1. the deterministic acceptance rule;
2. the threshold-level schedule;
3. the stochastic neighborhood sampler.

This distinction is important because TA's acceptance decision itself does not require
a random draw.

## Deterministic acceptance rule

For minimization define the non-negative degradation

\f[
d(x,x')=\max\{0,f(x')-f(x)\}.
\f]

For maximization the sense-aware mirror is

\f[
d(x,x')=\max\{0,f(x)-f(x')\}.
\f]

The Dueck-Scheuer decision at threshold level \f$k\f$ is

\f[
x'\text{ is accepted}
\iff
d(x,x')\le\tau_k.
\f]

Hence \f$\tau_k=0\f$ accepts only improving or equal transitions.

## Implemented monotone threshold schedules

### Linear reduction

\f[
\tau_{k+1}
=
\max\{\tau_{\min},\tau_k-\delta\},
\qquad
\delta>0.
\f]

This is the v0.33 default because it can reach zero exactly after finitely many
threshold-level updates.

### Geometric reduction

\f[
\tau_{k+1}
=
\max\{\tau_{\min},\alpha\tau_k\},
\qquad
0<\alpha<1.
\f]

With \f$\tau_{\min}=0\f$, geometric decay approaches zero asymptotically, so a generic
stopping criterion should still bound the run.

### Explicit threshold sequence

\f[
\tau_0\ge\tau_1\ge\cdots\ge\tau_K\ge0.
\f]

`ExplicitThresholdSchedule` represents the classical generic threshold-list form
directly. The implementation validates finiteness, non-negativity and monotonicity.

## Performance architecture

TA uses the same `ReversibleTrajectoryStepExecutor` as Simulated Annealing.

With an exact `IMoveObjectiveDeltaEvaluator<TSolution,TMove>`:

- rejected moves are never applied;
- accepted moves are applied once;
- acceptance needs one comparison;
- no solution clone is required per transition.

Without a delta evaluator, the reversible fallback applies the move, evaluates it and
undoes rejected moves.

Compared with Metropolis acceptance, the threshold decision avoids both an exponential
function evaluation and the acceptance random draw. The neighborhood may of course
remain stochastic.

## Reviewed but intentionally deferred: Old Bachelor Acceptance

Hu, Kahng and Tsao (1995) introduced **Old Bachelor Acceptance (OBA)** as a
self-tuning, non-monotone threshold method designed for time-bounded optimization.
Its threshold can even become negative. OBA therefore does not belong inside a
monotone `IThresholdAcceptingSchedule`: it changes the acceptance-controller semantics
and deserves its own implementation rather than a misleading schedule flag.

## Scientific references

- Dueck, G.; Scheuer, T. (1990).
  *Threshold accepting: A general purpose optimization algorithm appearing superior to simulated annealing*,
  Journal of Computational Physics 90(1), 161-175.
  DOI: `10.1016/0021-9991(90)90201-B`.
- Winker, P.; Fang, K.-T. (1997).
  *Application of Threshold-Accepting to the Evaluation of the Discrepancy of a Set of Points*,
  SIAM Journal on Numerical Analysis 34(5), 2028-2042.
  DOI: `10.1137/S0036142995286076`.
- Hu, T. C.; Kahng, A. B.; Tsao, C.-W. A. (1995).
  *Old Bachelor Acceptance: A New Class of Non-Monotone Threshold Accepting Methods*,
  ORSA Journal on Computing 7(4), 417-425.
  DOI: `10.1287/ijoc.7.4.417`.