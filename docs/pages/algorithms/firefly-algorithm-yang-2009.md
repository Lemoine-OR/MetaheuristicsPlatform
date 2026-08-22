@page firefly_algorithm_yang_2009 Firefly Algorithm - Yang 2009

# Firefly Algorithm - Yang 2009

## General description

The Firefly Algorithm (FA) is a stochastic swarm-intelligence method introduced by
Xin-She Yang for multimodal continuous optimization. Candidate solutions are fireflies;
objective quality determines relative brightness, and a less bright firefly is attracted
toward a brighter one. Attraction decreases exponentially with squared Euclidean distance,
while an additive random term maintains exploration.

The platform implements the canonical continuous movement model rather than later adaptive,
chaotic, Lévy-flight or hybrid variants.

## Technical specifications

- Stable ID: `firefly-algorithm-yang-2009`
- Class: `FireflyOptimizer`
- Parameters: `FireflyParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.51.0
- Primary DOI: `10.1007/978-3-642-04944-6_14`
- Supporting DOI: `10.1504/IJBIC.2010.032124`

## Complexity

For population size \f$N\f$ and dimension \f$D\f$, a complete pairwise sweep performs
\f$O(N^2D)\f$ distance/movement work. In the sequential canonical implementation, up to
\f$N(N-1)\f$ attraction moves may be evaluated in the worst case, so objective-evaluation
cost is additive. Population storage is \f$O(ND)\f$.

## Applicability

FA is intended for bounded, derivative-free continuous optimization, particularly when
multimodal exploration is useful. Its distance-dependent attraction is scale-sensitive:
users should normalize variables or tune \f$\gamma\f$ when coordinate units differ strongly.

## Detailed operation

The population is initialized uniformly in the bounded continuous search space and every
firefly is evaluated through the common `OptimizationContext`.

During each complete iteration, ordered pairs are examined. If firefly \f$j\f$ is brighter
than firefly \f$i\f$ according to the configured optimization sense, \f$i\f$ moves toward
\f$j\f$. The moved point is clamped to the bounded domain and immediately evaluated. The
updated brightness participates in subsequent comparisons of the same sequential sweep.

This sequential semantics follows the original pairwise-improvisation structure and is made
explicit for reproducibility. A stopping condition that fires during an incomplete pairwise
sweep does not increment the completed-iteration count.

## Parameters

- `PopulationSize`: number of fireflies \f$N\f$.
- `MaximumIterations`: maximum number of complete pairwise sweeps.
- `BaseAttractiveness`: \f$\beta_0\f$, attractiveness at zero distance.
- `LightAbsorptionCoefficient`: \f$\gamma\f$, exponential distance attenuation.
- `RandomizationAmplitude`: \f$\alpha\f$ in the additive randomization term.

The random term is implemented literally as
\f$\alpha(U(0,1)-1/2)\f$ per coordinate; it is not silently rescaled by box width.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<FireflyOptimizer>(
        MetaheuristicAlgorithmIds.Firefly);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new FireflyParameters
        {
            PopulationSize = 20,
            BaseAttractiveness = 1.0,
            LightAbsorptionCoefficient = 1.0,
            RandomizationAmplitude = 0.2,
            MaximumIterations = 250
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`firefly-algorithm-yang-2009`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x).
\f]

The implementation is objective-sense symmetric, so maximization uses the same movement
logic with the platform's `OptimizationSense` comparison.

### Update equations / iterations

For a less bright firefly \f$i\f$ and brighter firefly \f$j\f$:

\f[
\begin{aligned}
r_{ij}^2
&=\sum_{d=1}^{D}(x_{i,d}-x_{j,d})^2,\\
\beta(r_{ij})
&=\beta_0\exp(-\gamma r_{ij}^2),\\
x_{i,d}^{+}
&=x_{i,d}
+\beta(r_{ij})(x_{j,d}-x_{i,d})
+\alpha\left(U_{i,d}-\frac12\right),
\qquad U_{i,d}\sim\mathcal U(0,1).
\end{aligned}
\f]

The bounded platform then applies component-wise clamping.

### Assumptions

The represented domain is a finite bounded continuous box, objective values are finite, and
pairwise squared Euclidean distances remain finite. Brightness ordering is identified with
objective ordering under the configured minimization or maximization sense.

### Convergence conditions

The library does not claim a universal finite-time global convergence guarantee. Practical
behavior depends on \f$\beta_0\f$, \f$\gamma\f$, \f$\alpha\f$, population size, scaling of the
search coordinates and evaluation budget. Later adaptive/randomization variants are kept
scientifically distinct from this canonical identity.

### Scientific references

Yang (2009), *Firefly Algorithms for Multimodal Optimization*, Stochastic Algorithms:
Foundations and Applications, LNCS 5792, 169-178.
DOI: `10.1007/978-3-642-04944-6_14`.

Yang (2010), *Firefly Algorithm, Stochastic Test Functions and Design Optimisation*,
International Journal of Bio-Inspired Computation 2(2), 78-84.
DOI: `10.1504/IJBIC.2010.032124`.
