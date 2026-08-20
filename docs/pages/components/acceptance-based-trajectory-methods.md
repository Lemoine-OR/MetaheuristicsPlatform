@page acceptance_based_trajectory_methods Acceptance-Based Trajectory Methods

# Acceptance-Based Trajectory Methods

## Scope

The platform distinguishes acceptance-based trajectories by the reference used to judge a
candidate:

- Simulated Annealing: current objective, probabilistic Metropolis rule.
- Threshold Accepting: degradation from the current objective.
- Great Deluge: absolute moving water level.
- Record-to-Record Travel: deviation from the best accepted record.
- Late Acceptance Hill Climbing: circular history reference plus non-worsening
  hill-climbing safeguard.
- Demon-based acceptance: explicit non-negative credit/energy state with accepted-move
  energy balance; deterministic and not a threshold schedule.

## Classical Great Deluge

For minimization,

\f[
B_0=f(x_0),\qquad
x'\text{ accepted}\iff f(x')\le B_k,\qquad
B_{k+1}=B_k-\delta_B,\quad\delta_B>0.
\f]

The implementation preserves the classical Dueck rule and does not silently add the
later Extended-GDA condition.

## Classical Record-to-Record Travel

For minimization,

\f[
r_k=\min_{0\le j\le k}f(x_j),\qquad
x'\text{ accepted}\iff f(x')-r_k\le D,\quad D\ge0.
\f]

The record is the best accepted/visited solution. Rejected probes consume evaluation
budget but cannot promote the global best.

## Late Acceptance Hill Climbing

The executable LAHC method is the final Burke-Bykov formulation. With a circular history
H of length L and v=k mod L,

\f[
\begin{aligned}
x'\text{ accepted}
&\iff f(x')<H_v\ \lor\ f(x')\le f(x),\\
H_v&\leftarrow \min\{H_v,f(x_{\mathrm{current}})\}
\qquad\text{(minimization)}.
\end{aligned}
\f]

Thus the history stores objective values, not solutions, and a worse objective is never
written back into a history slot in the final formulation.

## Performance architecture

GDA, RRT, LAHC and Demon-Based Acceptance reuse `ReversibleTrajectoryStepExecutor`,
`ITrajectoryAcceptancePolicy`, optional exact move-objective deltas, common callbacks,
stopping, cancellation and deterministic seeded neighborhood randomness.

`TrajectoryStepEvaluationAccounting` centralizes probe-versus-visited accounting for
SA, TA, GDA, RRT, LAHC and Demon-Based Acceptance. LAHC adds O(L) scalar history memory;
the Demon controller adds only O(1) scalar credit state and O(1) acceptance/update work.

## Extended Great Deluge — reviewed / deferred

Burke, Bykov, Newall and Petrovic (2003) add the explicit rule "improves current OR
satisfies level" and a time/expected-quality interpretation. This changes classical Dueck
semantics.

## Adaptive Flex-Deluge — reviewed / deferred

Burke and Bykov (2016) introduce a flexible acceptance condition and run-time adaptation.
It is not merely another linear level schedule.

## Demon-based credit/energy acceptance

The executable v0.36.0 controller follows the one-point Demon energy exchange used in the
single-solution metaheuristic literature. For minimization, define
\f[
\Delta_k=f(x'_k)-f(x_k).
\f]
The candidate is accepted iff \f$\Delta_k\le D_k\f$, and an accepted move updates
\f$D_{k+1}=D_k-\Delta_k\f$. Hence an improvement (negative \f$\Delta_k\f$) replenishes
the Demon while a worsening accepted move spends credit. Rejected moves leave the credit
unchanged. In exact arithmetic, accepted minimization moves preserve \f$f(x_k)+D_k\f$.

## Demon-like last-improvement credit reset — reviewed / deferred

Some ILS/simheuristic papers use a different rule: the last improving base-solution gain is
stored as credit, and a later admissible deterioration consumes/reset that credit. This is
not the conserved-energy controller implemented here and remains separately cataloged.

## Wood-Downs optimization variants — reviewed / deferred

Wood and Downs (1998) introduced four optimization-oriented variants that modify the raw
Demon dynamics to encourage minimization: **Bounded Demon (BD)**, **Randomized Bounded
Demon (RBD)**, **Annealed Demon (AD)** and **Randomized Annealed Demon (RAD)**. Their
bound, annealing and Gaussian-control semantics are algorithmically material, so the
platform documents them separately instead of collapsing them into boolean flags on the
conserved-credit controller. They remain deferred until each generic schedule/state contract
is implemented from the primary formulation.

## Zimmermann-Salamon Demon Algorithm — reviewed / deferred separately

Zimmermann and Salamon (1992) define a generalized simulated-annealing Demon Algorithm
based on an ensemble of systems, target distributions and collective moves. This is not
the same algorithm as a later one-point Demon-like credit criterion. The platform therefore
keeps the two identities separate.

## Scientific references

- Dueck, G. (1993), *New Optimization Heuristics: The Great Deluge Algorithm and the
  Record-to-Record Travel*. DOI `10.1006/jcph.1993.1010`.
- Burke, E. K.; Bykov, Y. (2008), *A Late Acceptance Strategy in Hill-Climbing for
  Exam Timetabling Problems*, PATAT 2008.
- Burke, E. K.; Bykov, Y. (2017), *The late acceptance Hill-Climbing heuristic*.
  DOI `10.1016/j.ejor.2016.07.012`.
- Burke, E.; Bykov, Y.; Newall, J.; Petrovic, S. (2003),
  *A Time-Predefined Approach to Course Timetabling*.
  DOI `10.2298/YJOR0302139B`.
- Burke, E. K.; Bykov, Y. (2016),
  *An Adaptive Flex-Deluge Approach to University Exam Timetabling*.
  DOI `10.1287/ijoc.2015.0680`.
- Creutz, M. (1983), *Microcanonical Monte Carlo Simulation*. DOI `10.1103/PhysRevLett.50.1411`.
- Talbi, E.-G. (2009), *Single-Solution Based Metaheuristics*, Chapter 2. DOI `10.1002/9780470496916.ch2`.
- Wood, I. A.; Downs, T. (1998), *Demon algorithms and their application to optimization problems*, IEEE World Congress on Computational Intelligence / IJCNN, 1661-1666.
- Zimmermann, T.; Salamon, P. (1992), *The demon algorithm*.
  DOI `10.1080/00207169208804047`.
