@page cross_entropy_continuous_kroese_porotsky_rubinstein_2006 Cross-Entropy Method - Continuous Optimization

# Cross-Entropy Method - Continuous Optimization

## General description

The Cross-Entropy (CE) Method is an adaptive stochastic optimization method that updates
a parameterized sampling distribution from an elite subset of the best candidates. The
continuous implementation follows the normal-updating formulation developed for
continuous multi-extremal optimization by Kroese, Porotsky and Rubinstein.

The platform uses an independent-coordinate normal model. This deliberately corresponds
to the diagonal normal CE model; it is not presented as a full-covariance CMA-ES
substitute.

## Technical specifications

- Stable ID: `cross-entropy-continuous-kroese-porotsky-rubinstein-2006`
- Class: `ContinuousCrossEntropyOptimizer`
- Parameters: `ContinuousCrossEntropyParameters`
- Family: Evolutionary methods / adaptive distribution-based population search
- Search space: bounded continuous vectors
- Public since: v0.50.0
- Primary continuous reference DOI: `10.1007/s11009-006-9753-0`

## Complexity

For sample count \f$N\f$, elite count \f$N_e\f$ and dimension \f$D\f$, one complete
iteration requires \f$N\f$ objective evaluations, \f$O(N\log N)\f$ ranking and
\f$O(ND)\f$ sampling/elite-statistics work. Storage is \f$O(ND)\f$.

## Applicability

Bounded continuous, derivative-free, multi-extremal optimization where a population of
samples can adapt a probabilistic search model. The diagonal model is especially
appropriate when a coordinate-wise normal approximation is acceptable.

## Detailed operation

At iteration \f$t\f$, the algorithm draws \f$N\f$ candidates from an independent normal
distribution with smoothed mean \f$\widehat\mu_{t-1}\f$ and coordinate standard
deviations \f$\widehat\sigma_{t-1}\f$. Samples are clamped to the configured bounded
continuous domain before evaluation.

The best \f$N_e\f$ candidates form the elite set. Their maximum-likelihood coordinate
mean and standard deviation define the raw next distribution. The mean uses fixed
smoothing. The standard deviation uses the dynamic smoothing law described by
Kroese, Porotsky and Rubinstein:

\f[
\beta_t=
\beta-\beta\left(1-\frac1t\right)^q.
\f]

As \f$t\f$ increases, \f$\beta_t\f$ decreases, deliberately slowing distribution
collapse. This is the key difference from a simple fixed-smoothing CE implementation.

Every objective evaluation uses the common `OptimizationContext`. If a global stopping
criterion fires inside an incomplete sample population, the distribution is not updated
and the incomplete iteration is not counted.

## Parameters

- `SampleCount`: \f$N\f$, number of samples per complete iteration.
- `EliteFraction`: fraction used to derive \f$N_e\f$.
- `MaximumIterations`: local complete-iteration bound.
- `MeanSmoothing`: fixed mean smoothing \f$\alpha\f$.
- `StandardDeviationSmoothingBase`: dynamic-smoothing base \f$\beta\f$.
- `DynamicSmoothingExponent`: exponent \f$q\f$.
- `InitialStandardDeviationScale`: initial sigma as a fraction of coordinate box width.
- `MinimumStandardDeviation`: numerical floor and local collapse threshold.
- `InitialMean`: optional first mean; the box center is used by default.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ContinuousCrossEntropyOptimizer>(
        MetaheuristicAlgorithmIds.ContinuousCrossEntropy);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new ContinuousCrossEntropyParameters
        {
            SampleCount = 100,
            EliteFraction = 0.10,
            MaximumIterations = 250
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`cross-entropy-continuous-kroese-porotsky-rubinstein-2006`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x).
\f]

### Update equations / iterations

Let \f$I_t\f$ denote the elite indices.

\f[
\begin{aligned}
X_i^{(t)}
&\sim
\mathcal N\!\left(
\widehat\mu_{t-1},
\operatorname{diag}(\widehat\sigma_{t-1}^{\,2})
\right),\\
\widetilde\mu_{t,j}
&=
\frac{1}{N_e}
\sum_{i\in I_t}X_{i,j}^{(t)},\\
\widetilde\sigma_{t,j}^{\,2}
&=
\frac{1}{N_e}
\sum_{i\in I_t}
\left(
X_{i,j}^{(t)}-\widetilde\mu_{t,j}
\right)^2,\\
\widehat\mu_t
&=
\alpha\widetilde\mu_t
+
(1-\alpha)\widehat\mu_{t-1},\\
\beta_t
&=
\beta-
\beta\left(1-\frac1t\right)^q,\\
\widehat\sigma_t
&=
\beta_t\widetilde\sigma_t
+
(1-\beta_t)\widehat\sigma_{t-1}.
\end{aligned}
\f]

### Assumptions

The search domain is a finite bounded box, objective values are finite, and the
coordinate-wise normal model is an explicit modeling choice. Boundary clamping changes
the unconstrained normal law near active bounds and is documented as a platform boundary
policy rather than part of the unconstrained CE theory.

### Convergence conditions

The library does not claim a universal finite-time global convergence guarantee. CE
convergence results require additional stochastic/model assumptions. Dynamic smoothing is
used to reduce premature variance collapse; the practical stopping condition is still
problem and budget dependent.

### Scientific references

Rubinstein (1999), *The Cross-Entropy Method for Combinatorial and Continuous
Optimization*, Methodology and Computing in Applied Probability 1(2), 127-190.
DOI: `10.1023/A:1010091220143`.

de Boer, Kroese, Mannor & Rubinstein (2005), *A Tutorial on the Cross-Entropy Method*,
Annals of Operations Research 134(1), 19-67.
DOI: `10.1007/s10479-005-5724-z`.

Kroese, Porotsky & Rubinstein (2006), *The Cross-Entropy Method for Continuous
Multi-Extremal Optimization*, Methodology and Computing in Applied Probability 8(3),
383-407. DOI: `10.1007/s11009-006-9753-0`.
