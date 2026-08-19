@page path_relinking_strategies Advanced Path Relinking Strategies

# Advanced Path Relinking Strategies

## Scope

Version 0.32.0 retains all six v0.31.0 pairwise/path-generation strategies and adds
**generational Evolutionary Path Relinking (EvPR)** as a genuine population-level
intensification component.

Pairwise relinking remains factored into three orthogonal decisions:

1. **direction**: forward, backward, back-and-forward, or mixed;
2. **move selection**: greedy or greedy-randomized adaptive;
3. **path fraction**: full path or truncated fraction.

EvPR is an outer population decision: it evolves an elite population by repeatedly
applying the configured pairwise engine to elite-solution pairs.

## Direction policies

### Forward

\f[
x^I \longrightarrow x^G.
\f]

### Backward

\f[
x^G \longrightarrow x^I.
\f]

### Back-and-forward

\f[
P_{BF}=P(x^G,x^I)\cup P(x^I,x^G).
\f]

### Mixed

Mixed path relinking starts from both endpoints and alternates the active side:

\f[
\rho(x^{L}_{k+1},x^{R}_{k})<\rho(x^{L}_{k},x^{R}_{k}),
\qquad
\rho(x^{L}_{k+1},x^{R}_{k+1})<\rho(x^{L}_{k+1},x^{R}_{k}).
\f]

Mixed PR explores both endpoint neighborhoods with about one directional traversal,
whereas back-and-forward may require two complete traversals.

## Greedy-randomized adaptive move selection

For minimization,

\f[
\begin{aligned}
\tau_\alpha
  &=f_{best}+\alpha(f_{worst}-f_{best}),\\
RCL_\alpha
  &=\{m:f_m\le\tau_\alpha\},
\qquad 0\le\alpha\le1.
\end{aligned}
\f]

One target-directed move is sampled uniformly from the RCL. The implementation probes
each candidate once and retains compact probe records in pooled storage.

## Truncated path relinking

With initial path distance \f$\rho_0\f$ and configured fraction
\f$0<\theta\le1\f$, traversal stops once

\f[
\rho_0-\rho_k\ge\left\lceil\theta\rho_0\right\rceil.
\f]

## Evolutionary path relinking

The Resende-Werneck scheme takes an elite population \f$P^k\f$ and forms a fresh
generation from path-relinking offspring produced by **all unordered pairs**:

\f[
P^{k+1}
=
\operatorname{Elite}
\left(
\left\{
PR(x_i^k,x_j^k)
:
1\le i<j\le |P^k|
\right\}
\right).
\f]

`EvolutionaryPathRelinkingProcedure<TSolution>` implements this generation-level contract.
`EliteSolutionPool<TSolution>.TryAddEvolutionary` implements the population admission rule.

For a candidate \f$y\f$ and a full new population, the implemented admission rule is:

- accept on quality grounds when \f$y\f$ improves the current best;
- otherwise require that \f$y\f$ improves the current worst **and** satisfies the
  configured elite-distance threshold;
- replace the **most similar** elite whose objective is not better than \f$y\f$.

If the new generation best does not strictly improve the preceding generation best,
the evolutionary phase converges:

\f[
f^*_{k+1}\not\prec f^*_k
\quad\Longrightarrow\quad
\text{stop EvPR}.
\f]

The implementation optionally applies the composed local-search procedure to every
pairwise offspring before elite admission.

## Efficient default used by GRASP-PR

EvPR is opt-in. When enabled, its pairwise engine defaults to:

- `Mixed` direction;
- `GreedyRandomizedAdaptive` move selection;
- full path;
- `EvolutionaryPathRelinkingAlpha = 0.2`.

This avoids the double traversal of back-and-forward and reduces deterministic path replay
when an elite pair is encountered repeatedly.

## Runtime and memory

Let \f$b_k=|P^k|\f$. Generation \f$k\f$ performs

\f[
\binom{b_k}{2}
\f]

pairwise relinkings. If one pairwise call costs \f$C_{PR}\f$, one generation costs
\f$O(b_k^2 C_{PR})\f$ plus elite admission and optional local improvement.

Population memory remains bounded by the configured elite capacity. The pairwise greedy
fast path stays allocation-free; randomized path candidate buffers continue to use
`ArrayPool<T>`.

## Compatibility and stopping

`EvolutionaryPathRelinkingEnabled` defaults to `false`, so v0.31.0 behavior is unchanged
unless users explicitly enable EvPR.

All path objective probes, local-search evaluations, cancellation, deterministic random
streams and generic stopping criteria continue to share the active `OptimizationContext`.
If a generic stopping criterion fires, EvPR stops immediately.

## Scientific references

- Resende, M. G. C.; Werneck, R. F. (2004).
  *A Hybrid Heuristic for the p-Median Problem*, Journal of Heuristics 10(1), 59-88.
  DOI: `10.1023/B:HEUR.0000019986.96257.50`.
- Resende, M. G. C.; Ribeiro, C. C. (2005).
  *GRASP with path-relinking: Recent advances and applications*.
  DOI: `10.1007/0-387-25383-1_2`.
- Aiex, R. M.; Resende, M. G. C.; Pardalos, P. M.; Toraldo, G. (2005).
  *GRASP with Path Relinking for Three-Index Assignment*.
  DOI: `10.1287/ijoc.1030.0059`.
- Resende, M. G. C.; Marti, R.; Gallego, M.; Duarte, A. (2010).
  *GRASP and path relinking for the max-min diversity problem*,
  Computers & Operations Research 37(3), 498-508.
  DOI: `10.1016/j.cor.2008.05.011`.
- Ribeiro, C. C.; Resende, M. G. C. (2012).
  *Path-relinking intensification methods for stochastic local search algorithms*,
  Journal of Heuristics 18(2), 193-214.
  DOI: `10.1007/s10732-011-9167-1`.