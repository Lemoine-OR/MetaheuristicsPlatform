@page advanced_scatter_search_strategies Advanced Scatter Search strategies

# Advanced Scatter Search strategies

v0.40.0 extends the canonical five-method Scatter Search implementation without creating
a second public algorithm identity. The public algorithm remains
`scatter-search-marti-laguna-glover-2006`; the mechanisms below are scientific components.

## Implemented generic components

### Dynamic RefSet refresh — `ss.refset.update.dynamic-refresh`

The advanced refresh mode stops the current stale subset schedule immediately after an
accepted RefSet admission. The next combination round is therefore generated from the
updated RefSet:

\f[
y\in\operatorname{Admit}(R_k)
\Longrightarrow
R_{k+1}\leftarrow\operatorname{Update}(R_k,y),
\qquad
\mathcal S_{k+1}\leftarrow\operatorname{Subsets}(R_{k+1}).
\f]

`RoundSnapshot` remains the compatibility default.

### Two-tier RefSet — `ss.refset.update.two-tier`

Let

\f[
R=R^Q\cup R^D,
\qquad
|R^Q|=b_1,
\qquad
|R^D|=b_2.
\f]

`TwoTierScatterSearchReferenceSetUpdateMethod<TSolution>` improves the first tier by
objective value and the second by max-min diversity, preserving the advanced
quality/diversity split described by Martí, Laguna and Glover.

### Partial max-min rebuilding — `ss.refset.rebuild.max-min`

When a complete combination round becomes stable, an optional rebuilding method retains
the quality tier and can refill the diversity tier from a fresh diversified population:

\f[
x^\star
\in
\operatorname*{arg\,max}_{x\in P_{\mathrm{new}}\setminus R}
\min_{r\in R} d(x,r).
\f]

Rebuilding is disabled by default through `MaximumReferenceSetRebuilds = 0`.

### Minimum diversity — `ss.diversity.minimum-distance`

The two-tier updater can impose

\f[
d_{\min}(x)
=
\min_{r\in R^Q}d(x,r)
\ge
\operatorname{th}_{\mathrm{dist}}
\f]

while constructing and refreshing the quality tier.

### Representative Subset Types 1–4 — `ss.subsets.glover-types-1-4`

`GloverScatterSearchSubsetGenerationMethod<TSolution>` implements the representative
subset families:

1. all pairs;
2. triples obtained by augmenting each pair with the best solution outside it;
3. quadruples obtained by augmenting Type-2 triples;
4. the nested sets containing the best \f$i\f$ reference solutions for \f$5\le i\le b\f$.

Repeated subsets are suppressed and only subsets containing a new reference member are
returned in the current round.

## Reviewed / deferred advanced designs

The following designs are deliberately not reduced to generic flags:

- `ss.refset.update.three-tier-good-generators`: requires historical generator quality
  \f$g(x)\f$ for each reference solution;
- `ss.diversity.hashing`: the literature's hashing rules are representation-specific and
  collisions cannot replace semantic distance/equality generically;
- `ss.combination.variable-cardinality` and `ss.combination.binary`: specialized
  combination semantics require representation contracts;
- `ss.memory.explicit-evaluated-solutions`: requires stable solution identity and a
  bounded memory policy;
- `ss.path-relinking.deep-integration`: requires a typed bridge to the existing Path
  Relinking trajectory contracts.

## Scientific references

- Martí, R.; Laguna, M.; Glover, F. (2006), *Principles of scatter search*,
  DOI `10.1016/j.ejor.2004.08.004`.
- Laguna, M.; Martí, R. (2003), *Scatter Search: Methodology and Implementations in C*,
  DOI `10.1007/978-1-4615-0337-8`.
- Glover, F.; Laguna, M.; Martí, R. (2004),
  *Scatter Search and Path Relinking: Foundations and Advanced Designs*,
  DOI `10.1007/978-3-540-39930-8_4`.

Return to @ref scatter_search_marti_laguna_glover_2006 "Scatter Search".
