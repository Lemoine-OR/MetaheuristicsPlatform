@page path_relinking_strategies Advanced Path Relinking Strategies

# Advanced Path Relinking Strategies

## Scope

Version 0.31.0 implements the advanced pairwise path-generation strategies reviewed by
Ribeiro and Resende (2012) while preserving the v0.30.x greedy-forward public contract.
The strategy model is deliberately factored into three orthogonal decisions:

1. **direction**: forward, backward, back-and-forward, or mixed;
2. **move selection**: greedy or greedy-randomized adaptive;
3. **path fraction**: full path or truncated fraction.

This factorization is more general than a flat strategy enum because truncated path relinking
can be applied to several direction policies.

## Direction policies

### Forward

The newly generated local optimum is the initiating endpoint and an elite solution is the guide:

\f[
x^I \longrightarrow x^G.
\f]

### Backward

The elite solution becomes the initiating endpoint and the newly generated local optimum is the guide:

\f[
x^G \longrightarrow x^I.
\f]

Ribeiro and Resende report that backward path relinking often outperformed forward relinking
in the cited computational studies because the restricted neighborhood is explored more heavily
near the initiating endpoint.

### Back-and-forward

Two paths are traversed, backward first and then forward:

\f[
P_{BF}=P(x^G,x^I)\cup P(x^I,x^G).
\f]

This spends approximately the work of two directional traversals when neither is interrupted.

### Mixed

Mixed path relinking starts simultaneously from both endpoints and alternates the active side.
Each accepted move must strictly reduce the current distance between the two active endpoints:

\f[
\rho(x^{L}_{k+1},x^{R}_{k})<\rho(x^{L}_{k},x^{R}_{k}),
\qquad
\rho(x^{L}_{k+1},x^{R}_{k+1})<\rho(x^{L}_{k+1},x^{R}_{k}).
\f]

The two partial paths terminate when their endpoint attribute configurations meet.

## Greedy-randomized adaptive move selection

Let \f$f_m\f$ be the objective value obtained by applying target-directed move \f$m\f$.
For minimization, define

\f[
\tau_\alpha=f_{best}+\alpha(f_{worst}-f_{best}),
\qquad
RCL_\alpha=\{m:f_m\le\tau_\alpha\},
\qquad 0\le\alpha\le1.
\f]

One move is sampled uniformly from this RCL. For maximization the inequality and threshold
orientation are reversed. The implementation probes candidates once, stores the compact probe
triples in pooled arrays, and therefore avoids a second objective-evaluation pass.

## Truncated path relinking

Let \f$\rho_0\f$ be the initial endpoint distance and \f$0<\theta\le1\f$ the configured
`PathFraction`. The traversal stops once it has eliminated at least

\f[
\left\lceil\theta\rho_0\right\rceil
\f]

units of path distance, unless the guide/meeting configuration or another stopping criterion is
reached first. Setting \f$\theta=1\f$ recovers the full path.

## Runtime and allocation policy

Greedy selection keeps the v0.30.x allocation-free candidate scan. Greedy-randomized selection
must retain the already-probed move values until the RCL threshold is known; v0.31.0 uses
`ArrayPool<T>` rather than allocating a fresh candidate list at every path position.

Backward and forward have the same asymptotic cost. Back-and-forward can require roughly twice
the path work. Mixed advances one endpoint per path step and evaluates only that endpoint's
current target-directed restricted neighborhood.

## Evolutionary path relinking

Evolutionary path relinking is scientifically reviewed but intentionally **not** represented as
one `IPathRelinkingProcedure` policy in v0.31.0. The published scheme evolves an elite population
over generations by relinking pairs and forming a renewed elite set. That requires a distinct
population-level intensification contract and is therefore deferred rather than falsely reduced
to a pairwise direction flag.

## Scientific references

- Resende, M. G. C.; Ribeiro, C. C. (2005). *GRASP with path-relinking: Recent advances and applications*.
  DOI: `10.1007/0-387-25383-1_2`.
- Aiex, R. M.; Resende, M. G. C.; Pardalos, P. M.; Toraldo, G. (2005).
  *GRASP with Path Relinking for Three-Index Assignment*. DOI: `10.1287/ijoc.1030.0059`.
- Ribeiro, C. C.; Resende, M. G. C. (2012).
  *Path-relinking intensification methods for stochastic local search algorithms*,
  Journal of Heuristics 18(2), 193-214. DOI: `10.1007/s10732-011-9167-1`.