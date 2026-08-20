@page demon_based_acceptance_talbi_2009 Demon-Based Acceptance - Creutz/Talbi credit-energy controller

# Demon-Based Acceptance - Creutz/Talbi credit-energy controller

## General description

Demon-Based Acceptance is a deterministic one-point trajectory method with an explicit
non-negative scalar credit (or Demon energy) D. Its mechanism originates in Creutz's
microcanonical energy-exchange algorithm and is presented as a single-solution
metaheuristic acceptance rule by Talbi. The platform implements this conserved-credit
controller and does not conflate it with the ensemble Demon Algorithm of Zimmermann and
Salamon or with later ILS credit-reset rules.

@subpage acceptance_based_trajectory_methods

## Technical specifications

- Stable ID: `demon-based-acceptance-talbi-2009`.
- Optimizer: `DemonBasedAcceptanceOptimizer<TSolution,TMove,TUndo>`.
- Policy: `DemonAcceptancePolicy`.
- Main control parameter: non-negative `InitialCredit` (Demon energy D0).
- Deterministic acceptance; stochasticity comes from the sampled neighborhood.
- Generic reversible moves and optional exact candidate-objective deltas.
- O(1) Demon state, common `OptimizationContext<TSolution>` lifecycle and visited-state accounting.

## Complexity

Acceptance and Demon update are O(1): one oriented objective difference, one comparison
and, for an accepted move, one subtraction. With an exact move-objective evaluator,
rejection costs O(C_delta) and does not apply the move. Full reversible evaluation follows
the common trajectory executor. Demon-specific memory is O(1).

## Applicability

Any solution representation admitting a stochastic neighborhood and reversible moves.
Because `InitialCredit` is an absolute objective-scale quantity, it must be interpreted on
the scale of the problem objective. Unlike Simulated Annealing, the acceptance decision
requires neither an exponential function nor an acceptance random draw.

## Detailed operation

For minimization, let delta be the candidate objective increase relative to the current
solution. The candidate is accepted exactly when delta does not exceed the current Demon
credit D. On acceptance the same signed delta is subtracted from D. Therefore an
improvement, whose delta is negative, increases the available credit; an accepted
worsening move spends it; an equality leaves it unchanged. Rejected candidates do not
change D.

For maximization, the platform changes only the objective orientation: delta is current
minus candidate, so positive delta still means degradation. This preserves identical
credit semantics in both optimization senses.

## Parameters

- `InitialCredit` — finite non-negative initial Demon credit; library default `1.0`.
- `MaximumConsecutiveSamplingFailures` — default `64`.
- `InitialCredit = 0` is allowed: the search initially behaves greedily, but subsequent
  improvements can accumulate credit that later permits deterioration.
- The default credit is a library convenience, not a scale-independent recommendation.
- Generic stopping, callback, seed and cancellation controls remain independent.

## API example

```csharp
var algorithm =
    new DemonBasedAcceptanceOptimizer<MySolution, MyMove, MyUndo>(
        initialSolutionGenerator,
        stochasticNeighborhood,
        reversibleMoveOperator,
        exactDeltaEvaluator);

var result =
    algorithm.Optimize(
        problem,
        new DemonAcceptanceParameters { InitialCredit = 25.0 },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`demon-based-acceptance-talbi-2009`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

For minimization,

\f[
\begin{aligned}
\Delta_k&=f(x'_k)-f(x_k),\\
x_{k+1}&=
\begin{cases}
x'_k,&\Delta_k\le D_k,\\
x_k,&\Delta_k>D_k,
\end{cases}\\
D_{k+1}&=
\begin{cases}
D_k-\Delta_k,&\Delta_k\le D_k,\\
D_k,&\Delta_k>D_k,
\end{cases}
\qquad D_0\ge0.
\end{aligned}
\f]

For maximization use \f$\Delta_k=f(x_k)-f(x'_k)\f$. Positive delta therefore always
means objective degradation, independently of the optimization sense.

### Assumptions

Candidate and current objective values are finite; `InitialCredit` is finite and
non-negative; optional exact deltas agree with full evaluation; the neighborhood and
reversible move operator define valid transitions. No maximum-credit cap is imposed in
the canonical conserved-credit controller.

### Convergence conditions

The implementation makes no universal finite-time global-convergence claim. For every
accepted minimization transition, exact arithmetic gives the invariant
\f$f(x_{k+1})+D_{k+1}=f(x_k)+D_k\f$; for maximization the corresponding energy is
\f$-f(x)+D\f$. The credit can therefore move between objective energy and Demon energy,
while the best-so-far solution remains tracked independently by the common optimization
context.

### Scientific references

- Creutz, M. (1983), *Microcanonical Monte Carlo Simulation*, Physical Review Letters
  50(19), 1411-1414. DOI `10.1103/PhysRevLett.50.1411`.
- Talbi, E.-G. (2009), *Single-Solution Based Metaheuristics*, in *Metaheuristics: From
  Design to Implementation*, Chapter 2. DOI `10.1002/9780470496916.ch2`.
- Wood, I. A.; Downs, T. (1998), *Demon algorithms and their application to optimization
  problems*, IEEE World Congress on Computational Intelligence / IJCNN, 1661-1666.
- Zimmermann, T.; Salamon, P. (1992), *The demon algorithm*, International Journal of
  Computer Mathematics 42(1-2), 21-31. DOI `10.1080/00207169208804047` — documented
  here only to make the scientific distinction explicit; this is not the implemented
  one-point controller.
