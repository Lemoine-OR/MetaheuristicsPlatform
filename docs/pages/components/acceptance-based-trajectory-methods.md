@page acceptance_based_trajectory_methods Acceptance-Based Trajectory Methods

# Acceptance-Based Trajectory Methods

## Scope

The platform distinguishes acceptance-based trajectories by the reference against which a
candidate is judged:

- Simulated Annealing: current objective, probabilistic Metropolis rule.
- Threshold Accepting: degradation from current objective.
- Great Deluge: absolute moving water level.
- Record-to-Record Travel: deviation from the best accepted record.

## Classical Great Deluge

For minimization,

\f[
B_0=f(x_0),\qquad
x'\text{ accepted}\iff f(x')\le B_k,\qquad
B_{k+1}=B_k-\delta_B,\quad\delta_B>0.
\f]

The implementation preserves this classical Dueck rule. It does not silently add the
later Extended-GDA rule that also accepts every move improving the current solution.

## Classical Record-to-Record Travel

For minimization,

\f[
r_k=\min_{0\le j\le k}f(x_j),\qquad
x'\text{ accepted}\iff f(x')-r_k\le D,\quad D\ge0.
\f]

The record is the best accepted/visited solution. Rejected objective probes consume the
evaluation budget but cannot become a best solution without an accepted state snapshot.

## Performance architecture

Both methods reuse `ReversibleTrajectoryStepExecutor`,
`ITrajectoryAcceptancePolicy`, optional exact move-objective deltas, common callbacks,
stopping, cancellation and deterministic seeded neighborhood randomness.

`TrajectoryStepEvaluationAccounting` now centralizes probe-versus-visited accounting
for SA, TA, GDA and RRT.

## Extended Great Deluge — reviewed / deferred

Burke, Bykov, Newall and Petrovic (2003) add the explicit rule "improves current OR
satisfies level" and a time/expected-quality interpretation. This changes the classical
Dueck acceptance semantics and is therefore not hidden behind a GDA flag.

## Adaptive Flex-Deluge — reviewed / deferred

Burke and Bykov (2016) introduce a flexible acceptance condition and run-time adaptation.
It is not merely another linear level schedule and remains a distinct reviewed extension.

## Scientific references

- Dueck, G. (1993), *New Optimization Heuristics: The Great Deluge Algorithm and the
  Record-to-Record Travel*, Journal of Computational Physics 104(1), 86-92.
  DOI: `10.1006/jcph.1993.1010`.
- Burke, E.; Bykov, Y.; Newall, J.; Petrovic, S. (2003),
  *A Time-Predefined Approach to Course Timetabling*.
  DOI: `10.2298/YJOR0302139B`.
- Burke, E. K.; Bykov, Y. (2016),
  *An Adaptive Flex-Deluge Approach to University Exam Timetabling*.
  DOI: `10.1287/ijoc.2015.0680`.