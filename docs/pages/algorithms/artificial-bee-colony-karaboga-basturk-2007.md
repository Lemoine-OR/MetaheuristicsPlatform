@page artificial_bee_colony_karaboga_basturk_2007 Artificial Bee Colony

# Artificial Bee Colony

## General description

Artificial Bee Colony (ABC) is the population-based swarm method introduced by
Karaboga and developed with Basturk for numerical optimization. A candidate solution is
a food source. Employed bees explore around their own sources, onlookers select sources
with probability proportional to source fitness, and a scout replaces one source whose
trial counter reaches the abandonment limit.

## Technical specifications

- Stable ID: `artificial-bee-colony-karaboga-basturk-2007`
- Class: `ArtificialBeeColonyOptimizer`
- Parameters: `ArtificialBeeColonyParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.49.0
- Canonical reference DOI: `10.1007/s10898-007-9149-x`

## Complexity

For \f$S\f$ food sources and dimension \f$D\f$, initialization costs
\f$O(SD)\f$. One complete cycle evaluates \f$S\f$ employed candidates and
\f$S\f$ onlooker candidates, with one-coordinate perturbations but owned vector copies
and optional one-source scout reinitialization. Internal work is therefore
\f$O(SD)\f$ per cycle plus objective-evaluation cost, with \f$O(SD+S)\f$ storage.

## Applicability

Derivative-free bounded continuous optimization where local source exploitation should
coexist with probabilistic allocation of search effort and explicit abandonment/restart.

## Detailed operation

The implementation keeps \f$S\f$ owned food-source vectors. For source \f$i\f$, another
source \f$k\ne i\f$ and one coordinate \f$j\f$ are sampled. A random
\f$\phi\sim\mathcal U[-1,1]\f$ forms the canonical neighboring food source. Greedy
selection keeps strict improvements and otherwise increments the source trial counter.

After the employed-bee phase, onlooker probabilities are proportional to the canonical
ABC fitness transform. The platform scales all fitness values by their common maximum
before roulette selection; this does not change the probabilities and avoids overflow
in the probability sum.

At the scout phase, the source with the largest trial counter is reinitialized when that
counter reaches the abandonment limit. At most one source is scouted per cycle, matching
the canonical ABC structure.

Every candidate evaluation goes through the common `OptimizationContext`; a global
stopping criterion can therefore stop during initialization, employed, onlooker or scout
phases without evaluation-budget overshoot. An incomplete cycle never increments the
platform iteration count.

## Parameters

- `FoodSourceCount`: number of food sources; employed and onlooker counts both equal it.
- `MaximumCycles`: local complete-cycle limit.
- `AbandonmentLimit`: unsuccessful-trial limit; zero selects
  `FoodSourceCount * dimension`.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ArtificialBeeColonyOptimizer>(
        MetaheuristicAlgorithmIds.ArtificialBeeColony);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new ArtificialBeeColonyParameters
        {
            FoodSourceCount = 20,
            MaximumCycles = 500
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`artificial-bee-colony-karaboga-basturk-2007`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x).
\f]

### Update equations / iterations

For source \f$i\f$, partner \f$k\ne i\f$, coordinate \f$j\f$ and
\f$\phi_{i,j}\sim\mathcal U[-1,1]\f$,

\f[
\begin{aligned}
v_{i,j}
&=
x_{i,j}
+
\phi_{i,j}
\left(
x_{i,j}-x_{k,j}
\right),\\
p_i
&=
\frac{\mathrm{fit}(x_i)}
{\sum_{s=1}^{S}\mathrm{fit}(x_s)},\\
x_i^{+}
&=
\begin{cases}
v_i,& f(v_i)<f(x_i),\\
x_i,& \text{otherwise},
\end{cases}\\
\mathrm{trial}_i^{+}
&=
\begin{cases}
0,& f(v_i)<f(x_i),\\
\mathrm{trial}_i+1,& \text{otherwise}.
\end{cases}
\end{aligned}
\f]

The source is abandoned when its trial counter reaches the configured limit.

### Assumptions

The implementation assumes a finite bounded continuous search box and finite objective
values. One coordinate is perturbed in each employed/onlooker proposal. Boundary repair
uses the platform component-wise clamp policy.

### Convergence conditions

The platform does not claim a universal finite-time global convergence guarantee. ABC is
a stochastic population heuristic whose empirical behavior depends on source count,
abandonment limit, objective geometry and the available evaluation budget.

### Scientific references

Karaboga & Basturk (2007), *A Powerful and Efficient Algorithm for Numerical Function
Optimization: Artificial Bee Colony (ABC) Algorithm*, Journal of Global Optimization
39(3), 459-471. DOI: `10.1007/s10898-007-9149-x`.

Karaboga & Basturk (2008), *On the Performance of Artificial Bee Colony (ABC) Algorithm*,
Applied Soft Computing 8(1), 687-697. DOI: `10.1016/j.asoc.2007.05.007`.
