@page cma_es_components CMA-ES Components

# CMA-ES Components

Version 0.47.0 extends the canonical full-covariance foundation with executable Active CMA-ES and sep-CMA-ES.

## Implemented components

- `cma.sampling.multivariate-normal`
- `cma.recombination.logarithmic-positive`
- `cma.path.cumulation`
- `cma.step-size.csa`
- `cma.covariance.rank-one`
- `cma.covariance.rank-mu`
- `cma.covariance.active`
- `cma.variant.separable`

The sampling distribution is

\f[
x_k=m+\sigma B D z_k,
\qquad
z_k\sim\mathcal N(0,I),
\qquad
C=B D^2 B^{\mathsf T}.
\f]

The cumulative step-size path is

\f[
p_{\sigma}\leftarrow
(1-c_{\sigma})p_{\sigma}
+
\sqrt{c_{\sigma}(2-c_{\sigma})\mu_{\mathrm{eff}}}
C^{-1/2}
\frac{m^{+}-m}{\sigma}.
\f]

The covariance matrix receives rank-one and rank-\f$\mu\f$ information.

## Bounded-domain policy

The mathematical CMA-ES distribution is defined on \f$\mathbb R^n\f$. The platform
implementation targets the existing bounded continuous search-space contract and clamps
sampled points to the configured box. Adaptation uses the repaired displacement
\f$(x-m)/\sigma\f$. This boundary policy is explicit because it changes invariance close
to active bounds; it is not presented as part of the unconstrained CMA-ES theory.

## Active covariance adaptation

\f[
C^+=
\left(1-c_1-c_{\mu}\sum_iw_i\right)C
+c_1p_cp_c^{\mathsf T}
+c_{\mu}\sum_iw_i^{\circ}y_i y_i^{\mathsf T}.
\f]

Negative unsuccessful directions are normalized by their Mahalanobis length before
subtraction and the covariance eigenspectrum is floored before the next generation.

## Separable covariance adaptation

\f[
c_{jj}^{+}
=
\left[
1-c_{\mathrm{cov}}
+
(1-h_{\sigma})
\frac{c_{\mathrm{cov}}}{\mu_{\mathrm{cov}}}
c_c(2-c_c)
\right]c_{jj}
+
\frac{c_{\mathrm{cov}}}{\mu_{\mathrm{cov}}}(p_c)_j^2
+
c_{\mathrm{cov}}
\left(1-\frac{1}{\mu_{\mathrm{cov}}}\right)
\sum_iw_i y_{i,j}^2.
\f]

sep-CMA-ES stores only coordinate-wise variances and increases the covariance learning rate
by the Ros-Hansen factor \f$(n+2)/3\f$.

## Reviewed / deferred

- `cma.restart.ipop` — Auger & Hansen (2005), DOI `10.1109/CEC.2005.1554902`
- `cma.restart.bipop` — Hansen (2009), DOI `10.1145/1570256.1570333`

## Scientific references

- Hansen & Ostermeier (2001), *Completely Derandomized Self-Adaptation in Evolution Strategies*, Evolutionary Computation 9(2), 159-195. DOI: `10.1162/106365601750190398`.
- Hansen, Muller & Koumoutsakos (2003), *Reducing the Time Complexity of the Derandomized Evolution Strategy with Covariance Matrix Adaptation (CMA-ES)*, Evolutionary Computation 11(1), 1-18. DOI: `10.1162/106365603321828970`.
- Auger & Hansen (2012), *Tutorial CMA-ES: Evolution Strategies and Covariance Matrix Adaptation*, GECCO Companion. DOI: `10.1145/2330784.2330919`.

- Ros & Hansen (2008), *A Simple Modification in CMA-ES Achieving Linear Time and Space Complexity*. DOI: `10.1007/978-3-540-87700-4_30`.
- Jastrebski & Arnold (2006), *Improving Evolution Strategies through Active Covariance Matrix Adaptation*. DOI: `10.1109/CEC.2006.1688662`.
- Hansen & Ros (2010), *Benchmarking a Weighted Negative Covariance Matrix Update on the BBOB-2010 Noiseless Testbed*. DOI: `10.1145/1830761.1830788`.
