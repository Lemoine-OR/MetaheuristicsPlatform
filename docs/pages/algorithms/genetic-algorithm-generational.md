@page genetic_algorithm_generational Generational Genetic Algorithm

# Generational Genetic Algorithm

Stable public ID: `genetic-algorithm-generational`

## General description

The Genetic Algorithm (GA) foundation maintains a fixed-size population of complete
candidate solutions. Each generation selects parents from the evaluated population,
applies representation-specific crossover and mutation, evaluates offspring, and replaces
the parental population. Optional elitism copies the best current members without
re-evaluation.

The implementation is deliberately representation-independent. It does **not** claim to
be the exact binary-coded Simple Genetic Algorithm of one historical source. Instead it
implements the generic generational GA architecture documented by Eiben and Smith and by
Whitley's tutorial, while using the tournament-selection analysis of Blickle and Thiele
for the provided canonical selector.

## Technical specifications

- **Runtime:** `GenerationalGeneticAlgorithmOptimizer<TSolution>`.
- **Solution model:** fixed-size population.
- **Family:** Evolutionary.
- **Mechanism:** evolutionary operators.
- **Stochastic:** yes.
- **Search spaces:** continuous, binary, integer, permutation, combinatorial and mixed,
  when compatible domain operators are supplied.
- **Composition:** initializer, parent-selection, crossover and mutation components.
- **Ownership:** initial members, parent snapshots, offspring and elites are cloned through
  the supplied `ISolutionCloner<TSolution>`.
- **Objective sense:** minimization and maximization use the same selection/elitism logic
  through `OptimizationSense`.
- **Evaluation accounting:** only newly evaluated initial solutions and offspring increment
  the common evaluation counter; copied elites are not re-evaluated.

## Complexity

Let \f$N\f$ be the population size. Let \f$C_S\f$, \f$C_X\f$, \f$C_M\f$ and \f$C_f\f$ denote
parent-selection, crossover, mutation and objective-evaluation costs.

Without elitism sorting, one generation is

\f[
O\!\left(N(C_S+C_X+C_M+C_f)\right).
\f]

With \f$E>0\f$ elites, the current implementation additionally ranks the population, giving

\f[
O\!\left(N\log N+N(C_S+C_X+C_M+C_f)\right).
\f]

The owned current/next populations require \f$O(N\,|\mathrm{solution}|)\f$ storage, plus
representation-specific operator workspace.

## Applicability

The foundation applies whenever the domain can provide:

1. complete-solution initialization;
2. meaningful parent selection from objective values;
3. a representation-compatible crossover;
4. a representation-compatible mutation;
5. correct solution cloning.

The optimizer itself does not impose binary strings, real vectors or permutations.
Representation-specific operator identities are supplied by the v0.42 Advanced Genetic Algorithm component catalog; the generic runtime remains representation-independent.

@subpage advanced_genetic_algorithm_operators

## Detailed operation

For population size \f$N\f$:

1. generate and evaluate exactly \f$N\f$ initial complete candidates unless a common stopping
   rule stops initialization earlier;
2. optionally copy the best `EliteCount` current members into the next population;
3. select two parents for each mating event;
4. with probability `CrossoverProbability`, invoke the crossover component; otherwise use
   independent parent clones as the two raw offspring;
5. independently for each raw offspring, invoke the mutation component with probability
   `MutationProbability`;
6. evaluate owned offspring until the next population again contains exactly \f$N\f$ members;
7. complete one common optimization iteration and evaluate the common stopping criterion;
8. repeat until a common stopping rule fires or `MaximumGenerations` is reached.

For an odd number of required offspring, the final mating produces two raw children but
only the child needed to complete the population is evaluated.

## Parameters

| Parameter | Default | Meaning |
|---|---:|---|
| `PopulationSize` | 100 | Members in each complete population; must be at least 2. |
| `MaximumGenerations` | 100 | Algorithm-level upper bound on completed generations. |
| `CrossoverProbability` | 0.9 | Probability of invoking crossover for one selected parent pair. |
| `MutationProbability` | 1.0 | Probability of invoking the configured mutation method once per offspring. |
| `EliteCount` | 0 | Best members copied without re-evaluation; must be smaller than the population. |

`MutationProbability` is an **offspring-level invocation probability**. A bit-flip or
coordinate mutation method may expose its own per-locus probability; the two concepts are
not conflated.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

var initializer =
    new DelegateGeneticPopulationInitializer<double[]>(
        (problem, random) =>
        [
            20.0 * random.NextDouble() - 10.0,
            20.0 * random.NextDouble() - 10.0
        ]);

var crossover =
    new DelegateGeneticCrossoverMethod<double[]>(
        (first, second, problem, random) =>
        {
            double alpha = random.NextDouble();

            return new GeneticOffspringPair<double[]>(
                [
                    alpha * first[0] + (1.0 - alpha) * second[0],
                    alpha * first[1] + (1.0 - alpha) * second[1]
                ],
                [
                    alpha * second[0] + (1.0 - alpha) * first[0],
                    alpha * second[1] + (1.0 - alpha) * first[1]
                ]);
        });

var mutation =
    new DelegateGeneticMutationMethod<double[]>(
        (solution, problem, random) =>
        {
            int coordinate = random.NextInt32(solution.Length);
            solution[coordinate] += 0.1 * (2.0 * random.NextDouble() - 1.0);
            return solution;
        });

var ga =
    new GenerationalGeneticAlgorithmOptimizer<double[]>(
        initializer,
        new TournamentGeneticParentSelectionMethod<double[]>(3),
        crossover,
        mutation);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.GeneticAlgorithm,
    () => ga,
    replace: true);

OptimizationResult<double[]> result =
    ga.Optimize(
        problem,
        new GeneticAlgorithmParameters
        {
            PopulationSize = 100,
            MaximumGenerations = 200,
            CrossoverProbability = 0.9,
            MutationProbability = 1.0,
            EliteCount = 1
        },
        new ArraySolutionCloner<double>(),
        new MaxIterationsStoppingCriterion(200));
```

## Stable factory ID

The stable public ID is:

```text
genetic-algorithm-generational
```

Because the algorithm requires representation-specific composition, register the typed
configured optimizer with `MetaheuristicFactory.Register(...)` before resolving it with
`MetaheuristicFactory.Create<TAlgorithm>(...)`.

## Mathematical details

### Problem formulation

The generic foundation supports either optimization sense:

\f[
\min_{x\in\mathcal X} f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

Let \f$P_t=\{x_1^t,\ldots,x_N^t\}\f$. One generic generation is represented by

\f[
\begin{aligned}
(i,j)&\sim S(P_t;f),\\
(y_1,y_2)&\leftarrow X(x_i^t,x_j^t),\\
y_k&\leftarrow M(y_k),\qquad k\in\{1,2\},\\
P_{t+1}&\leftarrow E_t\cup O_t,\qquad |P_{t+1}|=N,
\end{aligned}
\f]

where \f$S\f$ is parent selection, \f$X\f$ crossover, \f$M\f$ mutation, \f$E_t\f$ the optional
elite subset and \f$O_t\f$ the evaluated offspring needed to restore size \f$N\f$.

Tournament selection of size \f$q\f$ samples \f$q\f$ indices with replacement and returns

\f[
i^\star\in
\operatorname*{arg\,best}_{i\in T_q} f(x_i),
\f]

with `arg best` interpreted according to the optimization sense.

### Assumptions

- every candidate supplied to the common evaluator is a complete solution;
- crossover and mutation semantics are representation-specific;
- the cloner returns independent owned snapshots for mutable representations;
- tournament selection uses objective values only and samples with replacement;
- no feasibility repair is silently added by the generic engine.

### Convergence conditions

No universal finite-time global convergence claim is made. A particular GA may admit
asymptotic convergence results under additional assumptions on mutation reachability,
selection, replacement and elitism. Those conditions depend on the configured
representation and operators and are therefore not claimed by this generic runtime.

### Scientific references

- Eiben, A. E.; Smith, J. E. (2003), *Genetic Algorithms*, in
  *Introduction to Evolutionary Computing*, pp. 37-69.
  DOI `10.1007/978-3-662-05094-1_3`.
- Whitley, D. (1994), *A genetic algorithm tutorial*, *Statistics and Computing* 4(2),
  65-85. DOI `10.1007/BF00175354`.
- Blickle, T.; Thiele, L. (1996), *A Comparison of Selection Schemes used in
  Evolutionary Algorithms*, *Evolutionary Computation* 4(4), 361-394.
  DOI `10.1162/EVCO.1996.4.4.361`.
