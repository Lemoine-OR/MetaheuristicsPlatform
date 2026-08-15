@page tabu_search_glover Tabu Search

# Tabu Search (Glover short-term-memory foundation)

Stable ID: `tabu-search-glover`

## General description

Tabu Search (TS) is a memory-based trajectory metaheuristic introduced by Fred Glover.
Instead of accepting only improving moves, TS selects a best admissible neighbor and uses
explicit memory to prevent strategically undesirable reversals or repetitions. The v0.21
implementation provides the generic short-term-memory core: attribute-based tabu status,
aspiration, best-admissible selection, configurable tenure, and efficient candidate evaluation.

The scope follows the distinction made in the foundational literature: short-term memory is
the core mechanism, while intermediate/long-term memory provides intensification and
diversification. Reactive Tabu Search additionally learns tenure from detected cycles. Those
advanced controllers are scientifically important, but v0.21 does not pretend to implement
them through a fixed-tenure approximation.

## Technical specifications

- Class: `TabuSearchOptimizer<TSolution,TMove,TUndo,TAttribute,TEnumerator>`
- Stable ID: `tabu-search-glover`
- Family: trajectory-based / local search
- Mechanisms: neighborhood, trajectory, memory-based guidance
- Neighborhood: allocation-free `IEnumeratedNeighborhood` cursor
- Move execution: reversible `IReversibleMoveOperator`
- Optional exact candidate objective: `IMoveObjectiveDeltaEvaluator`
- Tabu representation: domain-defined attributes through `ITabuAttributeProvider`
- Default tenure: fixed short-term tenure of 7 iterations
- Default aspiration: release a tabu candidate if it strictly improves the global best

## Complexity

Let \f$N(x)\f$ be the enumerated neighborhood of the current solution and let
\f$C_\Delta\f$ denote an exact delta-evaluation cost. With the delta fast path, one TS iteration
costs

\f[
O\!\left(|N(x)|\,C_\Delta + C_{\mathrm{apply}} + \log M\right).
\f]

Without exact delta evaluation, each candidate is temporarily applied, fully evaluated and
undone, giving

\f[
O\!\left(|N(x)|(C_{\mathrm{apply}}+C_f+C_{\mathrm{undo}})+\log M\right).
\f]

Expected tabu lookup is \f$O(1)\f$ with the default hash memory. Registration is \f$O(\log M)\f$
because expiration records are inserted into a min-priority queue. Expired records are removed
in expiration order without scanning the full dictionary.

Space is \f$O(|x|+M)\f$, where \f$M\f$ is the number of retained short-term expiration
records; stale re-registration records are removed lazily when their expiration is reached.

## Applicability

The implementation is generic over binary, integer, permutation, combinatorial, mixed, or
continuous representations when the user can provide:

1. an initial solution generator;
2. an allocation-free enumerated neighborhood;
3. a reversible move operator;
4. a meaningful tabu-attribute mapping.

Exact objective deltas are optional but strongly recommended for large neighborhoods.

## Detailed operation

At iteration \f$k\f$, the engine advances short-term memory and scans the current neighborhood.
For each applicable move it obtains a candidate tabu attribute. Non-tabu candidates are
eligible. A tabu candidate can become eligible when the configured aspiration criterion is
satisfied. The default aspiration rule admits a candidate that strictly improves the best
objective found so far.

Among eligible candidates, the algorithm retains the best objective value. The selected move
is then applied exactly once. A domain-defined attribute is registered as tabu for the tenure
returned by the tenure policy. This separation between the candidate attribute and the
registered attribute supports classical reversal prevention without forcing a particular move
representation.

When aspiration is disabled, a tabu candidate is rejected before objective evaluation. When an
exact delta evaluator is available, candidates are not temporarily applied during the scan.
Otherwise, reversible apply/evaluate/undo preserves the current solution without full cloning.

## Parameters

`TabuSearchParameters` exposes:

- `TenurePolicyKind`: fixed or uniformly varying tenure;
- `FixedTabuTenure`: fixed tenure, default 7;
- `RandomTenureMinimum` / `RandomTenureMaximum`: bounds for the varying policy;
- `CustomTenurePolicy`: user-defined tenure controller;
- `AspirationCriterionKind`: best-so-far or none;
- `CustomAspirationCriterion`: user-defined aspiration logic;
- `MemoryInitialCapacity`: initial capacity of the default expiration memory.

The generic platform still owns stopping criteria, deterministic random source, callbacks,
cancellation, best-so-far tracking, and evaluation accounting.

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

The canonical stable ID is `tabu-search-glover`. As with generic Simulated Annealing, domain
components must be composed first and the typed instance is then registered through
`MetaheuristicFactory`.

## Mathematical details

### Problem formulation

For minimization (maximization is handled symmetrically), let \f$N(x)\f$ be a neighborhood and
\f$T_k\f$ the set of attributes currently declared tabu:

\f[
\min_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

Let \f$a(x,m)\f$ denote the candidate attribute associated with move \f$m\f$, and let
\f$A_k(x,m)\f$ denote the aspiration predicate. The admissible move set is

\f[
\mathcal M_k(x)=\{m\in N(x): a(x,m)\notin T_k\ \lor\ A_k(x,m)\}.
\f]

The canonical best-admissible step is

\f[
m_k^*\in\arg\min_{m\in\mathcal M_k(x_k)} f(m(x_k)),
\qquad x_{k+1}=m_k^*(x_k).
\f]

For the default best-so-far aspiration criterion,

\f[
A_k(x,m) \iff f(m(x)) < f_{\mathrm{best},k}.
\f]

If the selected move registers attribute \f$r_k\f$ with tenure \f$\tau_k\f$, this implementation
marks it tabu for the next \f$\tau_k\f$ candidate-selection iterations, through iteration
\f$k+\tau_k\f$ under the library's one-based scan index.

### Assumptions

The neighborhood enumeration must be finite for each current solution. The attribute provider
must encode the domain semantics intended by the user: a tabu attribute is not necessarily the
move object itself. Exact-delta evaluators must return the exact objective that a full evaluation
would produce after applying the move. The default hash memory assumes stable equality and hash
semantics for `TAttribute`.

### Convergence conditions

Classical Tabu Search is a metaheuristic framework rather than a single globally convergent
Markov process with one universal parameter theorem. Search behavior depends on neighborhood,
memory attributes, tenure, aspiration, and longer-term strategies. Glover's foundational work
uses strategic memory to avoid local entrapment and to guide exploration; no universal
finite-time global-optimum guarantee is claimed by this implementation.

Reactive Tabu Search (Battiti & Tecchiolli, 1994) learns tabu-list size from detected cycles and
adds diversification when repetitions persist. That method requires configuration-repetition
memory and adaptive control not present in the v0.21 short-term foundation, and is therefore
reviewed for a later dedicated implementation rather than mislabeled as already supported.

### Scientific references

- Glover, F. (1986), *Future paths for integer programming and links to artificial intelligence*, Computers & Operations Research 13(5), 533-549. DOI `10.1016/0305-0548(86)90048-1`.
- Glover, F. (1989), *Tabu Search-Part I*, ORSA Journal on Computing 1(3), 190-206. DOI `10.1287/ijoc.1.3.190`.
- Glover, F. (1990), *Tabu Search-Part II*, ORSA Journal on Computing 2(1), 4-32. DOI `10.1287/ijoc.2.1.4`.
- Glover, F. & Laguna, M. (1997), *Tabu Search*, Kluwer/Springer. DOI `10.1007/978-1-4615-6089-0`.
- Battiti, R. & Tecchiolli, G. (1994), *The Reactive Tabu Search*, ORSA Journal on Computing 6(2), 126-140. DOI `10.1287/ijoc.6.2.126` (reviewed future extension; not claimed as implemented in v0.21).
