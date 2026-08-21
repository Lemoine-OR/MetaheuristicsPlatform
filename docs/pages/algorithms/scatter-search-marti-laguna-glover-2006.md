@page scatter_search_marti_laguna_glover_2006 Scatter Search

# Scatter Search

## General description

Scatter Search (SS) is an evolutionary metaheuristic based on systematic diversification,
a small **reference set (RefSet)**, structured subset generation and strategic solution
combination. Unlike a classical genetic algorithm, the canonical methodology does not
require randomized selection or randomized recombination: the defining principle is to
exploit information contained in strategically chosen reference solutions.

The v0.40.0 implementation preserves the five-method template explicitly:

1. Diversification Generation Method;
2. Improvement Method (optional but first-class);
3. Reference Set Update Method;
4. Subset Generation Method;
5. Solution Combination Method.

The stable public ID is `scatter-search-marti-laguna-glover-2006`.

## Technical specifications

- **Class:** `ScatterSearchOptimizer<TSolution>`
- **Solution model:** population / small elite reference set
- **Family:** evolutionary methods
- **Mechanisms:** evolutionary combination, memory-based RefSet, constructive/domain composition
- **Search spaces:** continuous, binary, integer, permutation, combinatorial and mixed through generic domain contracts
- **Stochasticity:** not intrinsic; components may use the platform RNG, but systematic deterministic designs are scientifically valid
- **Default RefSet update:** quality tier + max-min diversity tier, followed by strict quality replacement
- **Default subset generation:** all unordered pairs containing at least one new reference solution

## Complexity

Let \f$P\f$ be the diversification population size, \f$b\f$ the RefSet size and \f$K\f$ the
number of SS rounds. Let \f$C_f\f$, \f$C_I\f$, \f$C_d\f$ and \f$C_C\f$ denote respectively the
objective, improvement, distance and combination costs.

Initial RefSet construction costs, in the generic implementation,

\f[
O\!\left(
P(C_f+C_I)
+
Pb\,C_d
+
P\log P
\right).
\f]

Pairwise subset generation creates at most

\f[
\binom{b}{2}
=
\frac{b(b-1)}{2}
\f]

subsets in one round. The total cost therefore depends primarily on the
domain-specific combination/improvement/evaluation work.

The RefSet itself is \f$O(b)\f$ solution snapshots; the initial population is \f$O(P)\f$.

## Applicability

The implementation is representation-independent provided the domain supplies:

- a diversification generator for complete solutions;
- a non-negative finite distance;
- a solution-combination method;
- optionally an improvement method.

This directly supports continuous, discrete, permutation and mixed representations
without pretending that one Euclidean combination operator is meaningful for all domains.

## Detailed operation

### 1. Diversified population

Generate a population

\f[
P_0=\{x_1,\ldots,x_P\}
\f]

using `IScatterSearchDiversificationGenerationMethod<TSolution>`. Each complete solution
may be improved and is then evaluated exactly once by the common `OptimizationContext`.

### 2. Initial reference set

Let \f$b_1\f$ be `QualityReferenceSetSize` and \f$b\f$ be `ReferenceSetSize`.

The first tier contains the \f$b_1\f$ best distinct solutions:

\f[
R^{Q}
=
\operatorname{Best}_{b_1}(P_0).
\f]

The remaining positions are filled by max-min diversity. Given the current partial
RefSet \f$R\f$, select

\f[
x^\star
\in
\operatorname*{arg\,max}_{x\in P_0\setminus R}
\;
\min_{r\in R} d(x,r).
\f]

This is repeated until \f$|R|=b\f$.

### 3. Subset generation

The built-in simple strategy generates unordered pairs

\f[
S_{ij}=\{r_i,r_j\},
\qquad
1\le i<j\le b,
\f]

but only while at least one member has entered the RefSet since the previous
subset-generation round.

### 4. Combination and improvement

For every generated subset \f$S\f$, the domain method produces one or more complete
solutions

\f[
\mathcal C(S)
=
\{y_1,\ldots,y_m\}.
\f]

Each candidate may then be improved by the optional Improvement Method:

\f[
y'_j=I(y_j).
\f]

Only complete solutions are sent to the common objective evaluator.

### 5. RefSet update

The built-in v0.39 update first rejects duplicates under the configured distance tolerance.
A distinct candidate \f$y\f$ replaces the current worst RefSet member \f$r_w\f$ only when

\f[
f(y)\prec f(r_w),
\f]

where \f$\prec\f$ is the strict ordering induced by minimization or maximization.

The run stops early when a complete combination round produces no RefSet update.

## Parameters

- `DiversificationPopulationSize = 100`
- `ReferenceSetSize = 10`
- `QualityReferenceSetSize = 5`
- `MaximumIterations = 100`

The defaults are library defaults, not universal prescriptions. The classical literature
often uses a RefSet that is deliberately small relative to the diversified population.

## API example

```csharp
var diversification =
    new DelegateScatterSearchDiversificationGenerationMethod<MySolution>(
        (problem, random) => GenerateDiverseSolution(random));

var distance =
    new DelegateScatterSearchDistance<MySolution>(
        (in MySolution left, in MySolution right) =>
            MyDistance(left, right));

var combination =
    new DelegateScatterSearchSolutionCombinationMethod<MySolution>(
        (subset, problem, random) =>
            CombineReferenceSolutions(subset));

var scatterSearch =
    new ScatterSearchOptimizer<MySolution>(
        diversification,
        combination,
        distance);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.ScatterSearch,
    () => scatterSearch,
    replace: true);
```

## Stable factory ID

`scatter-search-marti-laguna-glover-2006`

Scatter Search requires domain composition, so the stable ID is catalogued immediately
but a typed runtime factory must be registered after the required methods are supplied.

## Mathematical details

### Problem formulation

For a minimization problem:

\f[
\min_{x\in\mathcal X} f(x).
\f]

The same runtime is objective-sense symmetric and supports maximization.

### Update equations / iterations

A compact description of the v0.39 foundation is

\f[
\begin{aligned}
P_0
&\leftarrow
\{I(D_1),\ldots,I(D_P)\},\\
R_0
&\leftarrow
\operatorname{RefSet}_{b_1,b}(P_0;f,d),\\
\mathcal S_k
&\leftarrow
\operatorname{Subsets}(R_k),\\
Y_k
&\leftarrow
\bigcup_{S\in\mathcal S_k} I(\mathcal C(S)),\\
R_{k+1}
&\leftarrow
\operatorname{Update}(R_k,Y_k;f,d).
\end{aligned}
\f]

### Assumptions

The common evaluator receives complete feasible candidates. Distance semantics and
combination semantics are representation-specific. The default RefSet updater assumes
that distance zero (or the configured tolerance) represents duplication for diversity
control.

### Convergence conditions

Scatter Search does not have a universal finite-time global-optimum guarantee under
arbitrary diversification, combination and improvement methods.

On a finite search space, the v0.39 strict-improvement replacement rule prevents an
infinite sequence of objective-worsening RefSet replacements; the implemented algorithm
also has an explicit iteration/stopping bound. These properties imply termination, not
global optimality. Global convergence requires additional problem- and component-specific
exploration assumptions that the generic library does not claim.

### Scientific references

- Martí, R.; Laguna, M.; Glover, F. (2006), *Principles of scatter search*,
  European Journal of Operational Research 169(2), 359-372.
  DOI `10.1016/j.ejor.2004.08.004`.
- Laguna, M.; Martí, R. (2003), *Scatter Search: Methodology and Implementations in C*.
  DOI `10.1007/978-1-4615-0337-8`.
- Glover, F.; Laguna, M.; Martí, R. (2004),
  *Scatter Search and Path Relinking: Foundations and Advanced Designs*.
  DOI `10.1007/978-3-540-39930-8_4`.
- Glover (1977) introduced the Scatter Search idea for integer programming; the v0.39
  stable ID is anchored to the later explicit principles/template reference above.

## Advanced Scatter Search in v0.40.0

v0.40.0 keeps the same canonical public algorithm ID and layers scientifically explicit
components over the five-method foundation:

- dynamic RefSet refresh after an accepted admission;
- a two-tier quality/diversity RefSet updater;
- optional partial max-min RefSet rebuilding after stable rounds;
- an explicit minimum-diversity threshold for quality-tier construction;
- representative Glover/Martí/Laguna Subset Types 1–4.

See @subpage advanced_scatter_search_strategies "Advanced Scatter Search strategies"
for the executable `ss.*` component catalog and the designs that remain
reviewed/deferred.

Compatibility defaults preserve v0.39.0 behavior:
`ReferenceSetRefreshMode = RoundSnapshot` and
`MaximumReferenceSetRebuilds = 0`.

The three-tier good-generator design is not approximated: it requires historical
generator performance \f$g(x)\f$. Hash-assisted duplicate control is likewise kept
reviewed/deferred until a representation-specific stable identity/hash contract can be
provided without treating collisions as semantic equality.
