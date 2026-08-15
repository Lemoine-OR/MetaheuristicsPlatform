@page tabu_search_memory_control_strategies Tabu Search Memory and Reactive Control Catalog

# Tabu Search Memory and Reactive Control Catalog

Introduced in **v0.22.0**, this catalog separates the classical short-term Tabu Search
foundation from longer-term memory, intensification/diversification mechanisms and the
Reactive Tabu Search controller.

The stable public algorithm identities remain distinct:

- `tabu-search-glover` — Glover-style generic Tabu Search foundation;
- `reactive-tabu-search-battiti-tecchiolli-1994` — Reactive Tabu Search with explicit
  configuration repetition memory and adaptive prohibition period.

This distinction avoids silently changing the scientific identity of the v0.21 algorithm.

## Scientific basis

Glover's Part I explicitly separates the short-term memory process from intermediate and
long-term memory used to intensify and diversify search. Part II adds dynamic tabu-list
management. Battiti and Tecchiolli then introduce explicit detection of repeated
configurations, automatic adaptation of the prohibition period in reaction to cycles, and an
escape mechanism using random moves whose count is proportional to a moving average of cycle
length.

The implementation follows those mechanisms while keeping domain-dependent semantics explicit:

- tabu attributes remain supplied by `ITabuAttributeProvider`;
- configuration identity is supplied by `ITabuSearchSolutionSignatureProvider`;
- a 64-bit signature is a contract of the provider, not a claim of collision-free hashing by
  the library;
- frequency penalties use a user-supplied objective-scale weight rather than pretending there
  is a universal coefficient;
- elite restart is an optional Glover-style intensification component and is disabled by
  default.

## Executable components

### `ts.memory.short-term.expiration`

**Expiration-based short-term tabu memory** — `ExpirationTabuMemory<TAttribute>`.

\f[
a\in T_k \iff e(a)\ge k.
\f]

Expected hash lookup is \f$O(1)\f$, registration is \f$O(\log M)\f$ because expiration
records are maintained in a min-priority queue, and expired entries are removed in expiration
order.

Reference: Glover (1989, 1990), DOI `10.1287/ijoc.1.3.190`.

### `ts.memory.frequency.attribute`

**Attribute frequency memory** — `AttributeFrequencyMemory<TAttribute>`.

\f[
F_k(a)=\sum_{j=1}^{k}\mathbf 1\{a_j=a\}.
\f]

Expected lookup/update is \f$O(1)\f$. The memory is used only when a frequency-guided
selection policy is enabled.

References: Glover (1989); Glover & Laguna (1997), DOI
`10.1007/978-1-4615-6089-0`.

### `ts.memory.repetition.hash`

**Configuration repetition hash memory** — `ConfigurationRepetitionMemory`.

For a configuration signature \f$h(x_k)\f$ last seen at iteration \f$j\f$,

\f[
L_k=k-j
\f]

is the detected cycle length. Hash-table lookup is expected \f$O(1)\f$, matching the
implementation direction described by Battiti and Tecchiolli.

Reference: Battiti & Tecchiolli (1994), DOI `10.1287/ijoc.6.2.126`.

### `ts.tenure.fixed`

**Fixed tenure** — `FixedTabuTenurePolicy`.

\f[
\tau_k=\tau.
\f]

Reference: Glover (1989), DOI `10.1287/ijoc.1.3.190`.

### `ts.tenure.uniform-random`

**Uniformly varying tenure** — `UniformRandomTabuTenurePolicy`.

\f[
\tau_k\sim\mathcal U_{\mathbb Z}[\tau_{\min},\tau_{\max}].
\f]

Reference: Glover (1990), DOI `10.1287/ijoc.2.1.4`.

### `ts.tenure.reactive-battiti-tecchiolli-1994`

**Reactive tenure** — `ReactiveTabuTenurePolicy`.

The prohibition period starts from a configured value (default 1), increases when repetition
is detected, and decreases after a configurable interval without repetition evidence:

\f[
\tau_{k+1}=
\begin{cases}
\operatorname{clip}(\lceil\rho_+\tau_k\rceil), & \text{repetition},\\
\operatorname{clip}(\lfloor\rho_-\tau_k\rfloor), & \text{sustained non-repetition},\\
\tau_k, & \text{otherwise}.
\end{cases}
\f]

The numerical factors are intentionally exposed because the original paper defines a reactive
mechanism rather than one universal parameter set.

Reference: Battiti & Tecchiolli (1994), DOI `10.1287/ijoc.6.2.126`.

### `ts.aspiration.best-so-far`

**Best-so-far aspiration** — `BestSoFarAspirationCriterion`.

For minimization,

\f[
A_k(m)\iff f(m(x_k))<f_{\mathrm{best},k}.
\f]

Reference: Glover (1989), DOI `10.1287/ijoc.1.3.190`.

### `ts.control.intensification.elite-restart`

**Elite restart intensification** — implemented by the RTS engine when
`IntensificationAfterIterationsWithoutImprovement > 0`.

\f[
x_k\leftarrow x_{\mathrm{best}}
\quad\text{when}\quad
s_k\ge s_{\mathrm{int}}.
\f]

The short-term tabu memory is reset, while repetition/frequency knowledge is retained. This is
a generic Glover-style intensification component and not claimed as a unique canonical formula.

### `ts.control.diversification.frequency-penalty`

**Frequency-guided diversification** — enabled when `FrequencyPenaltyWeight > 0`.

For minimization, admissible candidates are ranked by

\f[
\widetilde f_k(m)=f(m(x_k))+\lambda F_k(a_m).
\f]

For maximization the sign of the penalty is reversed. The default \f$\lambda=0\f$ preserves
pure objective ranking.

This is a transparent library realization of Glover-style frequency memory; the coefficient is
problem-scale dependent and is therefore never auto-invented.

### `ts.control.diversification.reactive-random-walk`

**Reactive random-walk escape** — activated when repeated configurations persist.

The controller maintains an exponentially smoothed cycle length \f$\overline L\f$ and requests

\f[
n_{\mathrm{esc}}=
\left\lceil\gamma\,\overline L\right\rceil
\f]

applicable random moves (bounded by a configured maximum). Each escape move is selected by
allocation-free reservoir sampling from the current finite neighborhood, so only the selected
move needs objective evaluation.

Reference: Battiti & Tecchiolli (1994), DOI `10.1287/ijoc.6.2.126`.

## Reactive Tabu Search execution contract

`ReactiveTabuSearchOptimizer<TSolution,TMove,TUndo,TAttribute,TEnumerator>` uses:

1. the same allocation-free enumerated-neighborhood contract as v0.21;
2. the same reversible move and exact-delta fast paths;
3. an explicit solution-signature provider for repetition memory;
4. a stateful reactive tenure controller;
5. best-so-far aspiration;
6. long-term attribute frequency memory;
7. optional elite intensification;
8. reactive escape moves selected uniformly by reservoir sampling.

The main best-admissible scan remains allocation-free. With exact deltas its dominant cost is

\f[
O(|N(x)|\,C_\Delta)
\f]

per normal iteration, plus expected constant-time memory lookups. An escape step scans
\f$N(x)\f$ only to reservoir-sample one applicable move and evaluates only that selected move.

## Reviewed advanced strategies not falsely reduced

### `ts.control.strategic-oscillation-glover`

Strategic oscillation moves around deliberately selected feasibility or structural boundaries.
Those boundaries are domain-specific, so v0.22 reviews the strategy but does not reduce it to
a generic numeric toggle.

### `ts.memory.influence-based`

Influence-based memory requires domain semantics that measure the consequences of search
choices beyond recency/frequency counts. It is reviewed but not represented by a fake generic
scalar.

### `ts.control.path-relinking`

Path relinking generates trajectories between elite/reference solutions. It is a broader
advanced/hybrid strategy with its own reference-set and path-generation semantics, so v0.22
does not hide it inside the RTS kernel.

## Parameters

`ReactiveTabuSearchParameters` exposes:

- initial/minimum/maximum reactive tabu tenure;
- tenure increase/decrease factors;
- decrease interval without repetition;
- cycle-length moving-average coefficient;
- repetition threshold that triggers escape;
- escape-length multiplier and maximum;
- `FrequencyPenaltyWeight` (0 disables);
- `IntensificationAfterIterationsWithoutImprovement` (0 disables);
- aspiration policy/custom aspiration;
- custom reactive tenure controller;
- initial memory capacity.

Generic stopping, callbacks, cancellation, deterministic RNG ownership and evaluation
accounting remain managed by `OptimizationContext`.

## Scientific references

1. Glover, F. (1989), *Tabu Search-Part I*, ORSA Journal on Computing 1(3),
   190-206. DOI `10.1287/ijoc.1.3.190`.
2. Glover, F. (1990), *Tabu Search-Part II*, ORSA Journal on Computing 2(1),
   4-32. DOI `10.1287/ijoc.2.1.4`.
3. Battiti, R. & Tecchiolli, G. (1994), *The Reactive Tabu Search*, ORSA Journal
   on Computing 6(2), 126-140. DOI `10.1287/ijoc.6.2.126`.
4. Glover, F. & Laguna, M. (1997), *Tabu Search*, Kluwer/Springer.
   DOI `10.1007/978-1-4615-6089-0`.
