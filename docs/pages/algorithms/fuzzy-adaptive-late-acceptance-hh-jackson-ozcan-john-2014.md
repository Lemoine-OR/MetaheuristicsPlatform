@page fuzzy_adaptive_late_acceptance_hh_jackson_ozcan_john_2014 Fuzzy Adaptive Late-Acceptance Hyper-Heuristic

# Fuzzy Adaptive Late-Acceptance Hyper-Heuristic

## General description

Fuzzy Adaptive Late-Acceptance Hyper-Heuristic (`FuzzyAdaptiveLateAcceptanceHyperHeuristic`) is the public scientific identity associated with
Jackson, Ozcan & John (2014), *Fuzzy adaptive parameter control of a late acceptance hyper-heuristic*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `fuzzy-adaptive-late-acceptance-hh-jackson-ozcan-john-2014`
- Class: `FuzzyAdaptiveLateAcceptanceHyperHeuristicOptimizer`
- Parameters: `FuzzyAdaptiveLateAcceptanceHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.153.0
- Primary DOI/permanent identifier: `10.1109/UKCI.2014.6930167`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

The late-acceptance history length adapts online from improvement and stagnation signals through a portable rule-based fuzzy-control adaptation.

## Parameters

`FuzzyAdaptiveLateAcceptanceHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new FuzzyAdaptiveLateAcceptanceHyperHeuristicOptimizer().Optimize(
        domain,
        new FuzzyAdaptiveLateAcceptanceHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`fuzzy-adaptive-late-acceptance-hh-jackson-ozcan-john-2014`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}L_{t+1}&=\operatorname{Adapt}(L_t,\operatorname{stagnation}_t,r_t),\\x_{t+1}&=\operatorname{LA}(x_t,h_t(x_t),H^{(L_t)}).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Jackson, Ozcan & John (2014), *Fuzzy adaptive parameter control of a late acceptance hyper-heuristic*, 14th UK Workshop on Computational Intelligence.
DOI/permanent identifier: `10.1109/UKCI.2014.6930167`.
