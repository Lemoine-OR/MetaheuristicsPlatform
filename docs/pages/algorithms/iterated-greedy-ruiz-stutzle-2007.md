@page iterated_greedy_ruiz_stutzle_2007 Iterated Greedy - Ruiz-Stützle 2007

# Iterated Greedy - Ruiz-Stützle 2007

## General description

Iterated Greedy (IG) repeatedly applies a constructive heuristic through two defining
phases: partial destruction of the incumbent complete solution and greedy reconstruction
of a new complete candidate. The platform implements this scientific pattern directly,
with optional reusable local improvement and a pluggable acceptance criterion.

The primary provenance is Ruiz and Stützle (2007), while the 2025 *Handbook of Heuristics*
chapter is used as the modern generic reference for the broader Iterated Greedy framework.

The implementation deliberately does **not** hard-code the permutation-flowshop NEH
reconstruction, processing-time temperature normalization, or job representation. Those
belong to the problem domain, not to a reusable generic metaheuristic core.

## Technical specifications

- Stable ID: `iterated-greedy-ruiz-stutzle-2007`.
- Optimizer: `IteratedGreedyOptimizer<TSolution,TRemoved>`.
- Destruction contract: `IIteratedGreedyDestruction<TSolution,TRemoved>`.
- Reconstruction contract: `IIteratedGreedyConstruction<TSolution,TRemoved>`.
- Optional local improvement: existing `ILocalSearchProcedure<TSolution>`.
- Built-in strict-improvement acceptance:
  `ImprovingOnlyIteratedGreedyAcceptancePolicy`.
- Built-in constant-temperature Metropolis acceptance:
  `ConstantTemperatureIteratedGreedyAcceptancePolicy`.
- Common `OptimizationContext<TSolution>` lifecycle, callbacks, stopping, best-so-far
  ownership and deterministic random-source ownership.
- The partially destroyed solution is never submitted to the objective evaluator.

## Complexity

Let \f$C_D(d)\f$ denote destruction cost for destruction size \f$d\f$, \f$C_C(d)\f$ the
reconstruction cost, \f$C_{\mathrm{eval}}\f$ one complete objective evaluation and
\f$C_{LS}\f$ an optional local-search invocation. One IG cycle costs

\f[
O\!\left(C_D(d)+C_C(d)+C_{\mathrm{eval}}+C_{LS}\right).
\f]

The generic core itself adds O(1) scalar state. Memory beyond the owned incumbent,
candidate, domain-specific removed-component state and optional local-search workspace is
O(1).

## Applicability

IG is most natural for combinatorial or component-decomposable solutions for which a
complete solution can be partially destroyed and then reconstructed by a greedy or
semi-greedy procedure. Permutations are the classical example, but the contracts do not
assume a job-sequencing representation.

The destruction/reconstruction pair must agree on the semantics of `TRemoved`, and
reconstruction must restore a valid complete solution before the platform evaluates it.

## Detailed operation

Starting from a complete incumbent \f$x_0\f$, an optional local search may first improve it.
Each iteration then:

1. clones the incumbent into an owned candidate;
2. destroys `DestructionSize` components and obtains domain-owned removed-component state;
3. reconstructs a complete candidate from the partial solution and removed state;
4. evaluates the complete reconstructed candidate;
5. optionally applies the reusable local-search procedure;
6. applies the configured acceptance policy against the current incumbent;
7. keeps best-so-far ownership independently of whether the candidate becomes the next
   incumbent.

This is intentionally different from Iterated Local Search: ILS perturbs a complete
solution and then improves it, whereas IG explicitly passes through a partial-solution
destruction and reconstruction cycle.

## Parameters

- `DestructionSize` — positive requested number of destroyed components; default `4`.
  The default reflects a common classical flowshop setting and is a library convenience,
  not a universal value prescribed for every problem.
- `MaximumIterations` — positive maximum number of destroy-reconstruct-accept cycles;
  default `1000`.
- Constant-temperature acceptance receives an **absolute** objective-scale temperature.
  For the classical PFSP implementation, the paper derives a problem-specific scaled
  temperature from processing times; the generic library does not pretend that formula is
  valid for arbitrary objective functions.
- Generic stopping, callbacks, deterministic seed, cancellation and local-search controls
  remain independent.

## API example

```csharp
var acceptance =
    new ConstantTemperatureIteratedGreedyAcceptancePolicy(
        temperature);

var algorithm =
    new IteratedGreedyOptimizer<MySolution, RemovedComponents>(
        initialSolutionGenerator,
        destructionOperator,
        greedyReconstructionOperator,
        acceptance,
        localSearch);

var result =
    algorithm.Optimize(
        problem,
        new IteratedGreedyParameters
        {
            DestructionSize = 4,
            MaximumIterations = 5000
        },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`iterated-greedy-ruiz-stutzle-2007`

Because the optimizer requires domain-defined destruction and reconstruction, the stable
ID identifies the scientific method in the catalog/factory but typed composition must be
registered by the application.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

With destruction \f$D_d\f$, reconstruction \f$C\f$, optional improvement \f$L\f$, and
acceptance \f$A\f$,

\f[
\begin{aligned}
p_k&=D_d(x_k),\\
y_k&=C(p_k),\\
z_k&=L(y_k),\\
x_{k+1}&=
\begin{cases}
z_k,&A(x_k,z_k)=1,\\
x_k,&\text{otherwise}.
\end{cases}
\end{aligned}
\f]

When no local-search procedure is composed, \f$L\f$ is the identity. For constant
temperature \f$\tau>0\f$, a worsening minimization candidate with degradation
\f$\Delta=f(z_k)-f(x_k)>0\f$ is accepted with probability

\f[
\exp\!\left(-\frac{\Delta}{\tau}\right).
\f]

Maximization uses the mirrored non-negative degradation.

### Assumptions

Destruction operates on an owned clone; reconstruction returns a valid complete solution;
objective values used by acceptance are finite; the domain-specific destruction size is
valid for the represented solution; composed local search obeys the common evaluation and
stopping lifecycle.

### Convergence conditions

The library makes no universal finite-time global-convergence claim for IG. With strict
improvement acceptance and deterministic operators the incumbent sequence is monotone in
objective quality, but the method can become trapped. Stochastic destruction and
Metropolis-style acceptance increase exploration, yet global convergence requires
additional problem- and transition-specific irreducibility/recurrence conditions that are
not implied by the generic IG pattern alone.

### Scientific references

- Ruiz, R.; Stützle, T. (2007), *A simple and effective iterated greedy algorithm for the
  permutation flowshop scheduling problem*, European Journal of Operational Research
  177(3), 2033-2049. DOI `10.1016/j.ejor.2005.12.009`.
- Stützle, T.; Ruiz, R. (2025), *Iterated Greedy*, in *Handbook of Heuristics*,
  pp. 745-777. DOI `10.1007/978-3-032-00385-0_10`.

### Reviewed advanced lineages reserved for v0.38.0

These methods are scientifically distinct extensions and are not reduced to Boolean
options in v0.37.0:

- improved / two-stage Iterated Greedy for distributed PFSP:
  DOI `10.1016/j.omega.2018.03.004`;
- Iterated Reference Greedy:
  DOI `10.1016/j.cie.2017.06.025`;
- adaptive / dynamic destruction-size Iterated Greedy:
  DOI `10.1016/j.asoc.2020.106629`.

v0.38.0 is reserved for a reviewed advanced catalog and only those generic strategies
that can be represented faithfully without smuggling flowshop-specific assumptions into
the common runtime.
