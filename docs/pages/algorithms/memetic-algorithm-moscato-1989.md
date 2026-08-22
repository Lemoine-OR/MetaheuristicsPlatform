@page memetic_algorithm_moscato_1989 Memetic Algorithm - Moscato

# Memetic Algorithm - Moscato

## General description

Version 0.43.0 introduces the canonical memetic principle identified by Moscato:
population-based evolutionary search is coupled with individual local improvement.
The executable foundation deliberately reuses the exact generational GA lifecycle instead
of duplicating selection, crossover, mutation, elitism or common optimization accounting.

The v0.43 runtime is a generic **Genetic Local Search / Memetic Algorithm** composition.
Its local-improvement policy and learning policy are independent contracts, preparing later
population engines without pretending that DE or another lifecycle is already wired.

## Technical specifications

- stable algorithm ID: `memetic-algorithm-moscato-1989`;
- population model: fixed-size generational engine shared with the canonical GA;
- local improvement: any `ILocalSearchProcedure<TSolution>`;
- local-search application: every offspring, periodic, probabilistic, top-fraction or
  stagnation-adaptive;
- learning: Lamarckian or Baldwinian;
- copied elites are not locally re-optimized in v0.43, which keeps Baldwinian genotype
  fitness semantics unambiguous;
- all local-search probes share the same `OptimizationContext`, evaluation budget,
  cancellation token, callbacks and best-so-far ownership rules.

## Complexity

Let \f$N\f$ be population size, \f$C_S\f$ parent-selection cost, \f$C_X\f$ crossover cost,
\f$C_M\f$ mutation cost, \f$C_f\f$ one full objective evaluation and \f$C_{LS}(x)\f$ the
cost of the configured local search on candidate \f$x\f$.

Without elitism sorting, one generation costs

\f[
O\!\left(
N(C_S+C_X+C_M+C_f)
+
\sum_{x\in C_t} C_{LS}(x)
\right),
\f]

where \f$C_t\f$ is the policy-selected local-improvement subset. Objective-ranked
top-fraction selection adds \f$O(N\log N)\f$ ranking work. Storage remains
\f$O(N|x|)\f$ for current/next populations plus one owned local-search phenotype and
domain-specific local-search workspace.

## Applicability

The algorithm is representation independent provided the domain supplies:

1. a complete-solution population initializer;
2. parent selection, crossover and mutation compatible with the representation;
3. a reusable `ILocalSearchProcedure<TSolution>`;
4. a correct solution cloner preserving strict ownership.

The composition is appropriate when evolutionary diversification and a meaningful local
improvement mechanism are both available.

## Detailed operation

For generation \f$t\f$:

1. the shared GA engine copies configured elites without reevaluation;
2. parent selection, crossover and mutation generate the remaining offspring;
3. every offspring is evaluated through the common optimization context;
4. the memetic policy selects a subset \f$C_t\f$ of the newly generated offspring;
5. each selected genotype is cloned into an owned phenotype and locally improved;
6. the learning policy applies Lamarckian or Baldwinian inheritance semantics;
7. the resulting population replaces the previous population;
8. generation-level stagnation statistics are updated and the common outer iteration is
   completed.

A local-search stopping decision immediately propagates to the outer optimizer. There is
no hidden evaluation budget: local-search objective probes contribute to the same global
evaluation counter.

## Parameters

`MemeticAlgorithmParameters` contains the canonical `GeneticAlgorithmParameters`.
The local-search procedure, application policy and learning policy are constructor-level
scientific components.

The default composition uses:

- `EveryOffspringMemeticLocalSearchPolicy`;
- `LamarckianMemeticLearningPolicy`.

A zero-probability `ProbabilisticMemeticLocalSearchPolicy` is intentionally supported as a
control configuration and reduces the memetic layer to the shared GA execution path.

## API example

```csharp
var memetic =
    new MemeticAlgorithmOptimizer<MySolution>(
        initializer,
        parentSelection,
        crossover,
        mutation,
        localSearch,
        new StagnationAdaptiveMemeticLocalSearchPolicy(
            minimumProbability: 0.10,
            maximumProbability: 1.00,
            stagnationWindow: 10),
        new LamarckianMemeticLearningPolicy());

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.MemeticAlgorithm,
    () => memetic,
    replace: true);

var parameters =
    new MemeticAlgorithmParameters
    {
        GeneticAlgorithm =
            new GeneticAlgorithmParameters
            {
                PopulationSize = 100,
                MaximumGenerations = 250,
                CrossoverProbability = 0.9,
                MutationProbability = 0.2,
                EliteCount = 2
            }
    };
```

## Stable factory ID

`memetic-algorithm-moscato-1989`

Because the optimizer is a typed generic composition, register the configured instance
with `MetaheuristicFactory.Register` before resolving it by stable ID.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

Let \f$E\f$ denote the evolutionary generation operator, \f$O_t=E(P_t)\f$ the generated
offspring and \f$C_t\subseteq O_t\f$ the candidates chosen for individual learning:

\f[
\begin{aligned}
O_t&=E(P_t),\\
x^{LS}&=\mathcal L(x),\qquad x\in C_t,\\
P_{t+1}&=\operatorname{Learn}\!\left(O_t,\{x^{LS}:x\in C_t\}\right).
\end{aligned}
\f]

For Lamarckian learning, the improved phenotype is inherited:

\f[
g_{t+1}\leftarrow x^{LS},\qquad F=f(x^{LS}).
\f]

For Baldwinian learning, the original genotype is inherited while selection observes the
learned objective:

\f[
g_{t+1}\leftarrow x,\qquad F=f(x^{LS}).
\f]

The implemented stagnation-adaptive application probability is

\f[
p_{\mathrm{LS}}(t)
=
p_{\min}
+
\left(p_{\max}-p_{\min}\right)
\min\!\left(1,\frac{s_t}{W}\right),
\f]

where \f$s_t\f$ counts consecutive generations without a new global best.

### Assumptions

The local-search procedure is required to return a solution no worse than its starting
objective in the problem sense. The common runtime assumes cloning establishes owned
snapshots and that all objective probes are registered through the supplied
`OptimizationContext`.

### Convergence conditions

No unconditional global-optimum or finite-time convergence claim is made. The runtime
preserves the stochastic exploration properties of the configured evolutionary operators
and the convergence properties of the supplied local search; adaptive local-search pressure
changes resource allocation rather than establishing a new universal convergence theorem.

### Scientific references

- Moscato, P. (1989). *On Evolution, Search, Optimization, Genetic Algorithms and
  Martial Arts: Towards Memetic Algorithms*. Caltech Concurrent Computation Program,
  Report 826.
- Krasnogor, N.; Smith, J. (2005). *A Tutorial for Competent Memetic Algorithms:
  Model, Taxonomy, and Design Issues*. IEEE Transactions on Evolutionary Computation
  9(5), 474-488. DOI: `10.1109/TEVC.2005.850260`.

## Scientific component catalog

@subpage memetic_algorithm_components
