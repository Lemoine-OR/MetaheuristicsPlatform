@page cma_es_components CMA-ES Components

# CMA-ES Components

Version 0.46.0 introduces the canonical full-covariance CMA-ES foundation.

## Implemented components

- `cma.sampling.multivariate-normal`
- `cma.recombination.logarithmic-positive`
- `cma.path.cumulation`
- `cma.step-size.csa`
- `cma.covariance.rank-one`
- `cma.covariance.rank-mu`

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

## Reviewed / deferred

- `cma.covariance.active`
- `cma.variant.separable`
- `cma.restart.ipop`
- `cma.restart.bipop`

These variants are reserved for the advanced CMA-ES package rather than approximated.

## Scientific references

- Hansen & Ostermeier (2001), *Completely Derandomized Self-Adaptation in Evolution Strategies*, Evolutionary Computation 9(2), 159-195. DOI: `10.1162/106365601750190398`.
- Hansen, Muller & Koumoutsakos (2003), *Reducing the Time Complexity of the Derandomized Evolution Strategy with Covariance Matrix Adaptation (CMA-ES)*, Evolutionary Computation 11(1), 1-18. DOI: `10.1162/106365603321828970`.
- Auger & Hansen (2012), *Tutorial CMA-ES: Evolution Strategies and Covariance Matrix Adaptation*, GECCO Companion. DOI: `10.1145/2330784.2330919`.
