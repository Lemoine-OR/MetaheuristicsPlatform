@page advanced_iterated_greedy_strategies Advanced Iterated Greedy Strategies

# Advanced Iterated Greedy Strategies

v0.38.0 separates **generic executable controls** from complete published IG variants
whose scientific meaning depends on a specific scheduling representation.

The canonical public algorithm remains
`iterated-greedy-ruiz-stutzle-2007`. Advanced controls use component IDs under `ig.*`;
they do not inflate the public-algorithm count.

## Executable generic components

### `ig.destruction.fixed`

Canonical fixed destruction size:

\f[
d_k=d_0.
\f]

### `ig.destruction.stagnation-escalating`

Generic platform controller:

\f[
d_k=
\min\!\left\{
d_{\max},
d_{\min}
+
\left\lfloor
\frac{s_k}{q}
\right\rfloor
\delta_d
\right\},
\f]

where \f$s_k\f$ counts consecutive completed cycles without a new run-wide best.
This is deliberately documented as a **generic platform policy inspired by adaptive IG
research**, not as an exact reproduction of the problem-specific controller of Li et al.
(2021).

### `ig.partial-improvement.hook`

`IIteratedGreedyPartialSolutionImprovement<TSolution,TRemoved>` is executed strictly
between destruction and reconstruction. The common complete-solution evaluator is not
exposed to this hook. That design is intentional: Dubois-Lacoste, Pagnozzi and Stützle
(2017) can optimize PFSP partial solutions because those partial solutions have
problem-defined objective semantics, but a generic library cannot assume the same for
arbitrary representations.

### `ig.acceptance.improving-only`

\f[
A_k=\mathbf 1[\Delta_k<0].
\f]

### `ig.acceptance.constant-temperature`

\f[
P(A_k=1)=
\begin{cases}
1,&\Delta_k\le 0,\\
\exp(-\Delta_k/\tau),&\Delta_k>0.
\end{cases}
\f]

The generic core receives an absolute objective-scale temperature. The PFSP-specific
normalization from the canonical paper remains a responsibility of the problem adapter.

## Reviewed complete variants

The following published methods are intentionally **not** represented as Boolean flags:

- `ig.bounded-search.fernandez-viagas-framinan-2015` —
  bounded-search distributed PFSP IG,
  DOI `10.1080/00207543.2014.948578`;
- `ig.tabu-reconstruction.ding-et-al-2015` —
  Tabu-based reconstruction for no-wait flowshop,
  DOI `10.1016/j.asoc.2015.02.006`;
- `ig.partial-optimization.dubois-lacoste-pagnozzi-stutzle-2017` —
  partial-solution optimization,
  DOI `10.1016/j.cor.2016.12.021`;
- `ig.reference-greedy.ying-lin-cheng-he-2017` —
  Iterated Reference Greedy,
  DOI `10.1016/j.cie.2017.06.025`;
- `ig.distributed.ruiz-pan-naderi-2019` —
  improved distributed PFSP IG,
  DOI `10.1016/j.omega.2018.03.004`;
- `ig.best-of-breed.fernandez-viagas-framinan-2019` —
  best-of-breed PFSP IG,
  DOI `10.1016/j.cor.2019.104767`;
- `ig.due-windows.jing-pan-gao-wang-2020` —
  distributed due-window IG,
  DOI `10.1016/j.asoc.2020.106629`;
- `ig.adaptive.li-pan-li-gao-tasgetiren-2021` —
  adaptive destruction + restart + problem-specific DMNIPFSP mechanisms,
  DOI `10.1016/j.swevo.2021.100874`;
- `ig.two-stage.zhang-qian-hu-li-yang-2026` —
  recent two-stage distributed blocking-flowshop IG,
  DOI `10.1016/j.eswa.2025.130422`.

## Why these methods remain separate

A complete scientific variant can modify several interacting mechanisms simultaneously:
representation, destruction bias, reconstruction memory, restart, reference guidance,
partial objective semantics, local-search neighborhoods and acceleration formulas.
Collapsing such methods into a collection of unrelated flags would create combinations
that no publication defines.

The v0.38.0 rule is therefore:

1. expose representation-independent mechanisms as reusable `ig.*` components;
2. document complete problem-specific variants with exact provenance;
3. implement a complete variant only when the platform owns all abstractions required to
   preserve its published semantics.

## API composition sketch

```csharp
var destructionSize =
    new StagnationEscalatingIteratedGreedyDestructionSizePolicy(
        minimumDestructionSize: 2,
        maximumDestructionSize: 8,
        stagnationWindow: 50);

var algorithm =
    new IteratedGreedyOptimizer<MySolution, RemovedComponents>(
        initialSolutionGenerator,
        destructionOperator,
        reconstructionOperator,
        acceptancePolicy,
        destructionSize,
        partialSolutionImprovement,
        localSearch);
```

## Scientific references

- Ruiz, R.; Stützle, T. (2007), DOI `10.1016/j.ejor.2005.12.009`.
- Fernandez-Viagas, V.; Framinan, J. M. (2015),
  DOI `10.1080/00207543.2014.948578`.
- Ding, J.-Y.; Song, S.; Gupta, J. N. D.; Zhang, R.; Chiong, R.; Wu, C. (2015),
  DOI `10.1016/j.asoc.2015.02.006`.
- Dubois-Lacoste, J.; Pagnozzi, F.; Stützle, T. (2017),
  DOI `10.1016/j.cor.2016.12.021`.
- Ying, K.-C.; Lin, S.-W.; Cheng, C.-Y.; He, C.-D. (2017),
  DOI `10.1016/j.cie.2017.06.025`.
- Ruiz, R.; Pan, Q.-K.; Naderi, B. (2019),
  DOI `10.1016/j.omega.2018.03.004`.
- Fernandez-Viagas, V.; Framinan, J. M. (2019),
  DOI `10.1016/j.cor.2019.104767`.
- Jing, X.-L.; Pan, Q.-K.; Gao, L.; Wang, Y.-L. (2020),
  DOI `10.1016/j.asoc.2020.106629`.
- Li, Y.-Z.; Pan, Q.-K.; Li, J.-Q.; Gao, L.; Tasgetiren, M. F. (2021),
  DOI `10.1016/j.swevo.2021.100874`.
- Zhang, S.; Qian, B.; Hu, R.; Li, K.; Yang, J.-B. (2026),
  DOI `10.1016/j.eswa.2025.130422`.
