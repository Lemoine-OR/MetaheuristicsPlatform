@page reactive_grasp_prais_ribeiro_2000 Reactive GRASP - Prais-Ribeiro

# Reactive GRASP - Prais-Ribeiro

## General description

Reactive GRASP extends canonical GRASP by replacing one fixed restricted-candidate-list parameter with a discrete set of candidate values whose selection probabilities learn from previously obtained solution quality.

The implementation follows the Prais-Ribeiro rule: probabilities are initially uniform, each completed GRASP iteration is associated with the alpha value that generated it, and the probability vector is periodically recomputed from the average locally improved objective observed for each alpha.

Stable ID: `reactive-grasp-prais-ribeiro-2000`.

## Technical specifications

- **Algorithm:** Reactive GRASP.
- **Primary source:** Prais & Ribeiro (2000).
- **Base method:** canonical GRASP of Feo & Resende.
- **Family:** constructive + local search.
- **Mechanisms:** constructive, neighborhood and adaptive.
- **Alpha model:** finite discrete set \f$\Psi=\{\alpha_1,\ldots,\alpha_m\}\f$.
- **Initial probabilities:** uniform \f$p_i=1/m\f$.
- **Learning statistic:** running mean \f$A_i\f$ of locally improved objective values produced with \f$\alpha_i\f$.
- **Probability rule:** canonical Prais-Ribeiro ratio for positive minimization objectives.
- **Maximization:** sense-consistent mirrored ratio.
- **RCL construction:** reuses `CanonicalGraspConstructionProcedure`, including its allocation-free two-pass reservoir-selection implementation.
- **Local search:** reuses `ILocalSearchProcedure<TSolution>`.
- **Common lifecycle:** every completed Reactive GRASP outer iteration calls `OptimizationContext.CompleteIteration`.

## Complexity

Let \f$m=|\Psi|\f$ be the number of alpha values. Construction and local-search complexity are unchanged from canonical GRASP.

Selecting an alpha by roulette sampling costs

\f[
O(m).
\f]

A probability recomputation also costs

\f[
O(m),
\f]

and occurs only every configured update period after every alpha has at least one observation. Storage for the reactive controller is

\f[
O(m)
\f]

for alpha values, probabilities, running means and counts.

The constructive RCL itself still has O(1) additional storage because it is sampled with reservoir selection rather than materialized.

## Applicability

Reactive GRASP is appropriate when different RCL restrictions produce meaningfully different construction/local-search behavior and manual tuning of a single alpha is undesirable.

The **canonical Prais-Ribeiro ratio update requires strictly positive objective values**. This is not silently relaxed in MetaheuristicsPlatform. If a natural objective can be zero or negative, transform it at the problem boundary before using the canonical ratio controller.

## Detailed operation

For each outer iteration:

1. sample an alpha index from the current discrete probability vector;
2. execute canonical adaptive randomized greedy construction with that alpha;
3. evaluate the complete construction;
4. improve it with the configured local-search procedure;
5. update the running average associated with the selected alpha;
6. update the global best objective through `OptimizationContext`;
7. once the update period is reached and every alpha has been observed, recompute the probability vector;
8. complete one common platform iteration;
9. continue until the algorithm-specific maximum or another common stopping criterion fires.

Probability learning uses locally improved solutions, so it measures the quality of the complete GRASP iteration rather than construction quality alone.

## Parameters

### `MaximumIterations`

Maximum number of complete Reactive GRASP iterations.

Default: `200`.

### `AlphaValues`

Discrete set of alpha values.

Default platform grid:

```text
0.0, 0.1, 0.2, ..., 0.9, 1.0
```

Values must be finite, unique and in `[0,1]`.

### `ProbabilityUpdatePeriod`

Number of completed observations between probability-update attempts.

Default: `10`.

An update is postponed until every configured alpha has at least one observation, because \f$A_i\f$ is undefined for an unseen alpha.

### `MaximumConstructionSteps`

Safety bound inherited from canonical GRASP construction.

Default: `int.MaxValue`.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constructive;

var reactive =
    new ReactiveGraspOptimizer<MySolution>(
        constructionProcedure,
        localSearchProcedure);

var parameters = new ReactiveGraspParameters
{
    MaximumIterations = 500,
    AlphaValues = new[] { 0.1, 0.3, 0.5, 0.7, 0.9 },
    ProbabilityUpdatePeriod = 25
};

var result = reactive.Optimize(
    positiveObjectiveProblem,
    parameters,
    solutionCloner,
    stoppingCriterion);
```

## Stable factory ID

`reactive-grasp-prais-ribeiro-2000`

Reactive GRASP is a composed generic algorithm. Register a configured typed instance with the stable factory ID when factory construction is desired.

## Mathematical details

### Problem formulation

For positive-objective minimization,

\f[
\min_{x\in\mathcal X} f(x),
\qquad f(x)>0.
\f]

Let

\f[
\Psi=\{\alpha_1,\ldots,\alpha_m\}
\f]

be the discrete alpha set.

### Update equations / iterations

Initially,

\f[
p_i=\frac{1}{m},
\qquad i=1,\ldots,m.
\f]

Let \f$z^*\f$ be the best objective found so far and \f$A_i\f$ the mean objective of all locally improved solutions generated using \f$\alpha_i\f$.

For minimization, Prais and Ribeiro use

\f[
q_i=\frac{z^*}{A_i},
\f]

followed by

\f[
p_i=
\frac{q_i}
{\sum_{j=1}^{m}q_j}.
\f]

Because \f$z^*\le A_i\f$ for positive minimization objectives, better average performance produces a larger quality weight.

For maximization, MetaheuristicsPlatform applies the sense-consistent mirrored ratio

\f[
q_i=\frac{A_i}{z^*},
\f]

again normalized by the sum of all quality weights.

The running mean is maintained online:

\f[
A_i^{(n)}
=
A_i^{(n-1)}
+
\frac{z_n-A_i^{(n-1)}}{n}.
\f]

### Assumptions

- every configured alpha is in `[0,1]`;
- objective observations are finite;
- canonical ratio adaptation is used only with strictly positive objectives;
- probability recomputation starts only after each alpha has at least one observation;
- local search reports its resulting objective through the common procedure contract;
- the underlying GRASP construction remains valid for every configured alpha.

### Convergence conditions

Reactive GRASP remains stochastic and no unconditional finite-time global convergence result is claimed.

The adaptive probability vector changes the sampling distribution over construction policies. As long as useful alpha values retain positive selection probability and the associated construction/local-search process has positive probability of reaching an optimal basin, repeated sampling preserves the usual asymptotic opportunity to reach that basin. Finite-budget performance remains problem-dependent.

### Scientific references

1. M. Prais and C. C. Ribeiro (2000), *Reactive GRASP: An Application to a Matrix Decomposition Problem in TDMA Traffic Assignment*, INFORMS Journal on Computing 12(3), 164-176. DOI: `10.1287/ijoc.12.3.164.12639`.
2. T. A. Feo and M. G. C. Resende (1995), *Greedy Randomized Adaptive Search Procedures*, Journal of Global Optimization 6(2), 109-133. DOI: `10.1007/BF01096763`.
