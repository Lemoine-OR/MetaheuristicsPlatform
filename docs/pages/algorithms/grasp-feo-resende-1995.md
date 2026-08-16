@page grasp_feo_resende_1995 GRASP - Feo-Resende

# GRASP - Feo-Resende

## General description

Greedy Randomized Adaptive Search Procedures (GRASP) repeatedly executes two phases:

1. construct a complete solution with an adaptive randomized greedy rule;
2. improve that solution with local search.

The best solution found over all GRASP iterations is retained. MetaheuristicsPlatform implements the canonical Feo-Resende structure while keeping the problem-specific greedy score, construction components and local neighborhood outside the generic optimizer.

Stable ID: `grasp-feo-resende-1995`.

## Technical specifications

- **Algorithm:** Greedy Randomized Adaptive Search Procedure.
- **Canonical source:** Feo & Resende (1995).
- **Solution model:** single constructed solution per outer iteration.
- **Family:** constructive + local search.
- **Randomization:** uniform selection from a threshold Restricted Candidate List (RCL).
- **Adaptation:** greedy scores are recomputed after every selected construction component.
- **Local improvement:** any `ILocalSearchProcedure<TSolution>`.
- **RCL implementation:** allocation-free two-pass cursor scan plus reservoir sampling.
- **Objective accounting:** completed constructed solutions and all local-search probes use the common `OptimizationContext`.
- **Reproducibility:** random choices use the deterministic run-owned `IRandomSource`.

## Complexity

Let \f$C_t\f$ be the candidate set at construction step \f$t\f$, \f$C_g\f$ the cost of one greedy-score evaluation, and \f$C_{LS}\f$ the cost of the composed local search.

The canonical allocation-free construction scans the current candidate set twice:

\f[
T_{\mathrm{construct}}
=
O\!\left(
2\sum_t |C_t|C_g
+
\sum_t C_{\mathrm{apply},t}
\right).
\f]

One GRASP outer iteration therefore costs

\f[
T_{\mathrm{GRASP}}
=
T_{\mathrm{construct}}
+
C_{\mathrm{eval}}
+
C_{LS}.
\f]

The RCL itself requires **O(1) additional memory**: no candidate list is materialized. The second pass uses uniform reservoir sampling over qualifying candidates. Total memory is therefore the solution representation plus the domain construction cursor and local-search workspace.

## Applicability

GRASP is especially natural for combinatorial problems in which a feasible or repairable solution can be assembled incrementally from components and where a meaningful local search is available.

The generic contracts also permit integer, mixed, permutation, binary and continuous representations when a domain supplies a valid construction model. The canonical literature identity, however, is fundamentally constructive and is most common in combinatorial optimization.

## Detailed operation

For each outer iteration:

1. create an initial partial solution;
2. enumerate all currently admissible construction candidates;
3. evaluate the domain-owned greedy score of every candidate;
4. obtain the best and worst greedy scores;
5. derive the threshold RCL from `Alpha`;
6. enumerate the candidates again;
7. select uniformly among RCL members using reservoir sampling;
8. apply the selected component to the partial solution;
9. repeat from step 2 until the construction model reports completeness;
10. evaluate the completed solution through `OptimizationContext`;
11. invoke the configured `ILocalSearchProcedure<TSolution>`;
12. retain the best-so-far solution through the common platform lifecycle;
13. start a new independent construction until the configured GRASP outer-iteration limit or a common stopping criterion is reached.

The construction phase is adaptive because candidate scores are recalculated after every component insertion against the new partial solution.

## Parameters

### `MaximumIterations`

Maximum number of complete construction + local-search GRASP outer iterations.

Default: `100`.

### `Alpha`

Threshold-RCL parameter in `[0,1]`.

Default: `0.2`.

`Alpha=0` keeps only candidates tied for the current greedy best. `Alpha=1` admits the complete candidate list. Intermediate values interpolate between greedy selection and full randomization.

### `MaximumConstructionSteps`

Safety bound for one construction phase. A domain model that fails to reach a complete solution before this limit is treated as invalid.

Default: `int.MaxValue`.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constructive;
using MetaheuristicsPlatform.Algorithms.Neighborhood;

var construction =
    new CanonicalGraspConstructionProcedure<
        MySolution,
        MyComponent,
        MyCandidateEnumerator>(
        myConstructionModel);

var grasp =
    new GraspOptimizer<MySolution>(
        construction,
        myLocalSearchProcedure);

var parameters = new GraspParameters
{
    MaximumIterations = 200,
    Alpha = 0.2
};

var result = grasp.Optimize(
    problem,
    parameters,
    solutionCloner,
    stoppingCriterion);
```

## Stable factory ID

`grasp-feo-resende-1995`

GRASP is a composed generic algorithm. Register the configured typed instance with the stable factory ID when factory construction is desired.

## Mathematical details

### Problem formulation

For a minimization problem,

\f[
\min_{x\in\mathcal X} f(x),
\f]

GRASP produces a sequence of locally improved randomized greedy constructions \f$x^{(1)},x^{(2)},\ldots\f$ and returns the best objective observed.

### Update equations / iterations

For a minimization-oriented greedy score \f$c(e)\f$, let

\f[
c_{\min}=\min_{e\in C}c(e),
\qquad
c_{\max}=\max_{e\in C}c(e).
\f]

The threshold is

\f[
\tau
=
c_{\min}
+
\alpha(c_{\max}-c_{\min}),
\qquad
0\le\alpha\le1,
\f]

and the restricted candidate list is

\f[
RCL
=
\{e\in C:\;c(e)\le\tau\}.
\f]

For a maximization-oriented greedy score,

\f[
\tau
=
c_{\max}
-
\alpha(c_{\max}-c_{\min}),
\f]

with

\f[
RCL
=
\{e\in C:\;c(e)\ge\tau\}.
\f]

One candidate is selected uniformly from the RCL. MetaheuristicsPlatform implements that uniform selection by reservoir sampling, so the probability of selecting any RCL member is

\f[
P(e\mid e\in RCL)=\frac{1}{|RCL|},
\f]

without allocating the RCL.

After construction, local search maps the constructed solution \f$x\f$ to a locally improved solution

\f[
x' = LS(x).
\f]

The incumbent best is updated using the original objective \f$f\f$, independently of the greedy construction score.

### Assumptions

The construction model must:

- eventually produce a complete solution;
- expose at least one candidate while incomplete;
- return finite, side-effect-free greedy scores;
- provide a fresh restartable cursor for each scan;
- apply selected components consistently;
- use a greedy score whose direction (`Minimize` or `Maximize`) is declared correctly.

The local-search procedure must satisfy the common platform contract and preserve objective accounting through `OptimizationContext`.

### Convergence conditions

No unconditional finite-time global convergence claim is made. GRASP is stochastic.

If a globally optimal basin has strictly positive probability of being generated and reached by the construction + local-search process, and independent GRASP iterations continue indefinitely, then the probability of never visiting that basin tends to zero. Practical finite-budget behavior remains problem- and construction-dependent.

### Scientific references

1. T. A. Feo and M. G. C. Resende (1989), *A probabilistic heuristic for a computationally difficult set covering problem*, Operations Research Letters 8(2), 67-71. DOI: `10.1016/0167-6377(89)90002-3`.
2. T. A. Feo and M. G. C. Resende (1995), *Greedy Randomized Adaptive Search Procedures*, Journal of Global Optimization 6(2), 109-133. DOI: `10.1007/BF01096763`.
