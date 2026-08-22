@page cma_es_hansen_ostermeier_2001 Covariance Matrix Adaptation Evolution Strategy

# Covariance Matrix Adaptation Evolution Strategy

## General description

CMA-ES adapts a multivariate Gaussian search distribution for non-linear,
non-convex continuous optimization. Version 0.46.0 implements positive logarithmic
recombination, evolution-path cumulation, cumulative step-size adaptation, and
full rank-one plus rank-mu covariance adaptation.

## Technical specifications

- Stable ID: `cma-es-hansen-ostermeier-2001`
- Class: `CmaEsOptimizer`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Public since: v0.46.0
- Scientific components: @subpage cma_es_components

## Complexity

With dimension \f$n\f$ and offspring population \f$\lambda\f$, offspring generation
and covariance transforms cost \f$O(\lambda n^2)\f$ and the dependency-free symmetric
Jacobi eigendecomposition costs \f$O(n^3)\f$ per complete generation. Storage is
\f$O(\lambda n+n^2)\f$.

## Applicability

CMA-ES is intended for real-valued, derivative-free optimization, especially when
the objective is non-separable or ill-conditioned. This implementation uses the
platform's `ISpanContinuousOptimizationProblem` bounded-box contract.

## Detailed operation

1. Resolve \f$\lambda\f$, \f$\mu\f$, positive logarithmic weights, and
   \f$\mu_{\mathrm{eff}}\f$.
2. Initialize mean \f$m\f$, global step size \f$\sigma\f$, covariance
   \f$C=I\f$, and evolution paths.
3. Sample offspring from \f$m+\sigma\mathcal N(0,C)\f$.
4. Clamp offspring to the bounded search space and evaluate each candidate through
   the common `OptimizationContext`.
5. Stop immediately without a partial-generation distribution update if a common
   stopping criterion becomes true inside the generation.
6. Recombine the best \f$\mu\f$ offspring.
7. Update the step-size path, covariance path, covariance matrix, and global step size.
8. Recompute a stable symmetric eigendecomposition for the next generation.

## Parameters

- `PopulationSize`: zero uses \f$4+\lfloor3\ln n\rfloor\f$.
- `ParentCount`: zero uses \f$\lfloor\lambda/2\rfloor\f$.
- `MaximumGenerations`: hard algorithm safety limit.
- `InitialMean`: optional; otherwise the box center.
- `InitialStepSize`: optional; otherwise 0.3 times the RMS box width.
- `MinimumCovarianceEigenvalue`: positive numerical floor for covariance decomposition.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<CmaEsOptimizer>(
        MetaheuristicAlgorithmIds.CmaEs);

var parameters =
    new CmaEsParameters
    {
        MaximumGenerations = 500,
        InitialStepSize = 0.5
    };

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        parameters,
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`cma-es-hansen-ostermeier-2001`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subset\mathbb R^n} f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X\subset\mathbb R^n} f(x).
\f]

### Update equations / iterations

\f[
\begin{aligned}
x_k^{(g+1)}
&=m^{(g)}+\sigma^{(g)}B^{(g)}D^{(g)}z_k,
\qquad z_k\sim\mathcal N(0,I),\\
m^{(g+1)}
&=\sum_{i=1}^{\mu}w_i x_{i:\lambda}^{(g+1)},\\
p_{\sigma}^{(g+1)}
&=(1-c_{\sigma})p_{\sigma}^{(g)}
+\sqrt{c_{\sigma}(2-c_{\sigma})\mu_{\mathrm{eff}}}
(C^{(g)})^{-1/2}
\frac{m^{(g+1)}-m^{(g)}}{\sigma^{(g)}},\\
\sigma^{(g+1)}
&=\sigma^{(g)}
\exp\!\left[
\frac{c_{\sigma}}{d_{\sigma}}
\left(
\frac{\lVert p_{\sigma}^{(g+1)}\rVert}{\chi_n}-1
\right)
\right],\\
C^{(g+1)}
&=(1-c_1-c_{\mu})C^{(g)}
+c_1p_c^{(g+1)}(p_c^{(g+1)})^{\mathsf T}
+c_{\mu}\sum_{i=1}^{\mu}
w_i y_{i:\lambda}y_{i:\lambda}^{\mathsf T}.
\end{aligned}
\f]

The implementation also applies the canonical \f$h_{\sigma}\f$ correction to the
covariance retention term when the covariance evolution path is suppressed.

### Assumptions

The objective is derivative-free and finite on the represented bounded domain.
The full covariance matrix is symmetric positive definite up to the documented
numerical eigenvalue floor. Near active box bounds, clamping modifies the ideal
unconstrained Gaussian sampling invariance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. CMA-ES is a
stochastic adaptive search method; theoretical behavior depends on objective class,
sampling support, adaptation parameters, and boundary treatment.

### Scientific references

Hansen & Ostermeier (2001), *Completely Derandomized Self-Adaptation in Evolution
Strategies*, Evolutionary Computation 9(2), 159-195.
DOI: `10.1162/106365601750190398`.

Hansen, Muller & Koumoutsakos (2003), *Reducing the Time Complexity of the
Derandomized Evolution Strategy with Covariance Matrix Adaptation (CMA-ES)*,
Evolutionary Computation 11(1), 1-18.
DOI: `10.1162/106365603321828970`.
