@page active_cma_es_hansen_ros_2010 Active CMA-ES

# Active CMA-ES

## General description

Active CMA-ES augments covariance adaptation with weighted negative information from
unsuccessful offspring. The implementation follows the weighted active CMA lineage:
successful directions increase covariance while poor ranked directions actively decrease
variance, with negative-step normalization and positive-definite reconstruction.

## Technical specifications

- Stable ID: `active-cma-es-hansen-ros-2010`
- Class: `ActiveCmaEsOptimizer`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Public since: v0.47.0
- Scientific components: @subpage cma_es_components

## Complexity

For dimension \f$n\f$ and population size \f$\lambda\f$, sampling and weighted covariance
updates cost \f$O(\lambda n^2)\f$ and the symmetric eigendecomposition costs \f$O(n^3)\f$
per complete generation. Storage is \f$O(\lambda n+n^2)\f$.

## Applicability

Derivative-free continuous optimization where variable dependencies matter and faster
covariance contraction along unsuccessful directions is useful.

## Detailed operation

The mean and evolution paths use the successful parent set. The covariance update also
uses negatively weighted unsuccessful offspring. Negative steps are normalized by their
Mahalanobis length before covariance subtraction. A symmetric eigendecomposition with the
platform eigenvalue floor reconstructs a positive-definite covariance for the next generation.

## Parameters

Active CMA-ES uses `CmaEsParameters`. `PopulationSize`, `ParentCount`,
`MaximumGenerations`, `InitialMean`, `InitialStepSize`, and
`MinimumCovarianceEigenvalue` retain their v0.46 meanings.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ActiveCmaEsOptimizer>(
        MetaheuristicAlgorithmIds.ActiveCmaEs);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new CmaEsParameters
        {
            MaximumGenerations = 500,
            InitialStepSize = 0.5
        },
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`active-cma-es-hansen-ros-2010`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subset\mathbb R^n}f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X\subset\mathbb R^n}f(x).
\f]

### Update equations / iterations

\f[
\begin{aligned}
C^{+}
&=
\left(1-c_1-c_{\mu}\sum_{i=1}^{\lambda}w_i\right)C
+c_1p_cp_c^{\mathsf T}
+c_{\mu}\sum_{i=1}^{\lambda}
w_i^{\circ}y_{i:\lambda}y_{i:\lambda}^{\mathsf T},\\
w_i^{\circ}
&=
\begin{cases}
w_i,&w_i\ge0,\\
w_i\,n/\lVert C^{-1/2}y_{i:\lambda}\rVert^2,&w_i<0.
\end{cases}
\end{aligned}
\f]

### Assumptions

The objective is finite on the bounded domain. The active negative mass is limited using
the standard positive-definiteness controls and every decomposed eigenvalue is bounded by
`MinimumCovarianceEigenvalue`.

### Convergence conditions

No universal finite-time global convergence guarantee is claimed. Active covariance
adaptation accelerates learning of unfavorable directions but remains a stochastic
distribution-adaptation method.

### Scientific references

Hansen & Ros (2010), *Benchmarking a Weighted Negative Covariance Matrix Update on the
BBOB-2010 Noiseless Testbed*, GECCO Companion, 1673-1680.
DOI: `10.1145/1830761.1830788`.

Jastrebski & Arnold (2006), *Improving Evolution Strategies through Active Covariance
Matrix Adaptation*, IEEE CEC.
DOI: `10.1109/CEC.2006.1688662`.
