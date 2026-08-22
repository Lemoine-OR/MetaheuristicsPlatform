@page separable_cma_es_ros_hansen_2008 Separable CMA-ES

# Separable CMA-ES

## General description

Separable CMA-ES (sep-CMA-ES) constrains covariance adaptation to diagonal variances.
It therefore samples coordinates independently, reducing internal time and storage
complexity while increasing the covariance learning rate as proposed by Ros and Hansen.

## Technical specifications

- Stable ID: `separable-cma-es-ros-hansen-2008`
- Class: `SeparableCmaEsOptimizer`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Public since: v0.47.0
- Scientific components: @subpage cma_es_components

## Complexity

With dimension \f$n\f$ and population size \f$\lambda\f$, all distribution operations
are \f$O(\lambda n)\f$ per complete generation and storage is
\f$O(\lambda n+n)\f$.

## Applicability

High-dimensional derivative-free optimization when the coordinate system is meaningful
and separability or weak variable interaction makes full rotational covariance learning
unnecessary.

## Detailed operation

The mean, cumulative step-size path, and covariance evolution path follow the CMA
lifecycle. The covariance state is represented by \f$n\f$ coordinate variances only.
The diagonal covariance learning rate is the CMA rate multiplied by
\f$(n+2)/3\f$, capped at one, matching the sep-CMA design.

## Parameters

sep-CMA-ES uses `CmaEsParameters` unchanged.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<SeparableCmaEsOptimizer>(
        MetaheuristicAlgorithmIds.SeparableCmaEs);

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

`separable-cma-es-ros-hansen-2008`

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
c_{jj}^{+}
&=
\left[
1-c_{\mathrm{cov}}
+
(1-h_{\sigma})
\frac{c_{\mathrm{cov}}}{\mu_{\mathrm{cov}}}
c_c(2-c_c)
\right]c_{jj}
+\frac{c_{\mathrm{cov}}}{\mu_{\mathrm{cov}}}(p_c)_j^2
+c_{\mathrm{cov}}
\left(1-\frac{1}{\mu_{\mathrm{cov}}}\right)
\sum_{i=1}^{\mu}w_i y_{i:\lambda,j}^2,\\
c_{\mathrm{cov}}^{\mathrm{sep}}
&=
\min\left\{
1,\frac{n+2}{3}c_{\mathrm{cov}}^{\mathrm{CMA}}
\right\}.
\end{aligned}
\f]

### Assumptions

The bounded continuous domain uses a meaningful coordinate system. Unlike full CMA-ES,
sep-CMA-ES is not rotationally invariant because cross-coordinate covariance is not learned.

### Convergence conditions

No universal finite-time global convergence guarantee is claimed. The reduced covariance
model trades dependency learning for linear internal complexity.

### Scientific references

Ros & Hansen (2008), *A Simple Modification in CMA-ES Achieving Linear Time and Space
Complexity*, PPSN X, 296-305.
DOI: `10.1007/978-3-540-87700-4_30`.
