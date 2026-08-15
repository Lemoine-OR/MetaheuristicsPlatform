@page tabu_search_glover Tabu Search

# Tabu Search (Glover foundation)

Stable ID: `tabu-search-glover`

## General description

Tabu Search (TS) is a memory-based trajectory metaheuristic introduced by Fred Glover.
Instead of accepting only improving moves, TS selects a best admissible neighbor and uses
explicit memory to prevent strategically undesirable reversals or repetitions.

The stable `tabu-search-glover` algorithm remains the generic v0.21 short-term-memory
foundation: attribute-based tabu status, aspiration, best-admissible selection, configurable
tenure and efficient candidate evaluation. v0.22 does **not** silently change this identity.
Longer-term memory components are cataloged separately, and Battiti-Tecchiolli Reactive Tabu
Search is exposed under the distinct stable ID
`reactive-tabu-search-battiti-tecchiolli-1994`.

## Technical specifications

- **Stable factory ID:** `tabu-search-glover`
- **Implementation class:** `TabuSearchOptimizer<TSolution,TMove,TUndo,TAttribute,TEnumerator>`
- **Family:** trajectory-based / local search
- **Mechanisms:** neighborhood, trajectory, memory-based guidance
- **Neighborhood:** allocation-free `IEnumeratedNeighborhood` cursor
- **Move execution:** reversible `IReversibleMoveOperator`
- **Optional exact candidate objective:** `IMoveObjectiveDeltaEvaluator`
- **Tabu representation:** domain-defined attributes through `ITabuAttributeProvider`
- **Default tenure:** fixed short-term tenure of 7 iterations
- **Default aspiration:** release a tabu candidate if it strictly improves the global best

## Complexity

Let \f$N(x)\f$ be the enumerated neighborhood and \f$C_\Delta\f$ an exact delta-evaluation cost.
With the delta fast path,

\f[
O\!\left(|N(x)|\,C_\Delta+C_{\mathrm{apply}}+\log M\right)
\f]

per iteration. Without exact deltas,

\f[
O\!\left(
|N(x)|(C_{\mathrm{apply}}+C_f+C_{\mathrm{undo}})
+\log M
\right).
\f]

Expected tabu lookup is \f$O(1)\f$ with the default hash memory. Registration is
\f$O(\log M)\f$ because expiration records use a min-priority queue.

Space is \f$O(|x|+M)\f$ for the current solution and retained short-term tabu records.

## Applicability

The implementation is generic over binary, integer, permutation, combinatorial, mixed or
continuous representations when the user supplies:

1. an initial solution generator;
2. an allocation-free enumerated neighborhood;
3. a reversible move operator;
4. meaningful tabu attributes.

Exact objective deltas are optional but strongly recommended for large neighborhoods.

## Detailed operation

At iteration \f$k\f$, the engine advances short-term memory and scans the current neighborhood.
For each applicable move it obtains a candidate tabu attribute. Non-tabu candidates are
eligible. A tabu candidate becomes eligible when the configured aspiration criterion is
satisfied. The default aspiration rule admits a candidate that strictly improves the global
best.

Among eligible candidates, the algorithm retains the best objective value and applies the
selected move exactly once. A domain-defined attribute is registered as tabu for the tenure
returned by the configured policy.

When aspiration is disabled, a tabu candidate is rejected before objective evaluation. With an
exact delta evaluator, candidates are not temporarily applied during the scan. Otherwise,
reversible apply/evaluate/undo preserves the current solution without full cloning.

## Parameters

`TabuSearchParameters` exposes:

- `TenurePolicyKind`: fixed or uniformly varying tenure;
- `FixedTabuTenure`: fixed tenure, default 7;
- `RandomTenureMinimum` / `RandomTenureMaximum`;
- `CustomTenurePolicy`;
- `AspirationCriterionKind`: best-so-far or none;
- `CustomAspirationCriterion`;
- `MemoryInitialCapacity`.

Generic stopping criteria, deterministic random ownership, callbacks, cancellation,
best-so-far tracking and evaluation accounting remain owned by the platform lifecycle.

## API example

```csharp
var tabu = new TabuSearchOptimizer<
    MySolution,
    MyMove,
    MyUndo,
    MyTabuAttribute,
    MyMoveEnumerator>(
        initialSolutionGenerator,
        neighborhood,
        reversibleMoveOperator,
        tabuAttributeProvider,
        exactDeltaEvaluator);

var parameters = new TabuSearchParameters
{
    FixedTabuTenure = 9,
    AspirationCriterionKind = TabuAspirationCriterionKind.BestSoFar
};

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.TabuSearch,
    () => tabu,
    replace: true);
```

## Stable factory ID

The canonical stable ID is `tabu-search-glover`. Domain components are composed first and the
typed instance is registered through `MetaheuristicFactory`.

## Advanced memory and reactive control

v0.22 adds a dedicated scientific component catalog:

`ts.*`

See @ref tabu_search_memory_control_strategies "Tabu Search Memory and Reactive Control Catalog".

Reactive Tabu Search is a distinct public algorithm:

`reactive-tabu-search-battiti-tecchiolli-1994`

This preserves the meaning and reproducibility of the original stable Glover TS ID.

## Mathematical details

### Problem formulation

For minimization,

\f[
\min_{x\in\mathcal X}f(x).
\f]

### Update equations / iterations

Let \f$a(x,m)\f$ be the candidate attribute and \f$A_k(x,m)\f$ the aspiration predicate. The
admissible set is

\f[
\mathcal M_k(x)=
\{m\in N(x):a(x,m)\notin T_k\ \lor\ A_k(x,m)\}.
\f]

The canonical step is

\f[
m_k^*\in
\arg\min_{m\in\mathcal M_k(x_k)}
f(m(x_k)),
\qquad
x_{k+1}=m_k^*(x_k).
\f]

The default best-so-far aspiration criterion is

\f[
A_k(x,m)\iff
f(m(x))<f_{\mathrm{best},k}.
\f]

### Assumptions

Neighborhood enumeration must be finite. The attribute provider must encode the intended
domain semantics. Exact-delta evaluators must return exactly the objective produced by full
evaluation after applying the move. The default hash memory assumes stable equality and hash
semantics for `TAttribute`.

### Convergence conditions

Tabu Search is a metaheuristic framework rather than a single globally convergent Markov
process with one universal parameter theorem. Search behavior depends on neighborhood,
memory attributes, tenure, aspiration and longer-term strategies. No universal finite-time
global-optimum guarantee is claimed.

Reactive Tabu Search is now implemented separately in v0.22 with explicit repetition memory,
reactive tenure and escape control rather than being mislabeled as part of this stable
short-term foundation.

### Scientific references

- Glover, F. (1986), *Future paths for integer programming and links to artificial intelligence*,
  Computers & Operations Research 13(5), 533-549. DOI
  `10.1016/0305-0548(86)90048-1`.
- Glover, F. (1989), *Tabu Search-Part I*, ORSA Journal on Computing 1(3),
  190-206. DOI `10.1287/ijoc.1.3.190`.
- Glover, F. (1990), *Tabu Search-Part II*, ORSA Journal on Computing 2(1),
  4-32. DOI `10.1287/ijoc.2.1.4`.
- Glover, F. & Laguna, M. (1997), *Tabu Search*, Kluwer/Springer.
  DOI `10.1007/978-1-4615-6089-0`.
- Battiti, R. & Tecchiolli, G. (1994), *The Reactive Tabu Search*, ORSA Journal
  on Computing 6(2), 126-140. DOI `10.1287/ijoc.6.2.126`.
