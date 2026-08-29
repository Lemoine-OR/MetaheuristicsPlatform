@page late_acceptance_selection_hh_jackson_ozcan_drake_2013 Late-Acceptance Cross-Domain Selection Hyper-Heuristic

# Late-Acceptance Cross-Domain Selection Hyper-Heuristic

## General description

Late-Acceptance Cross-Domain Selection Hyper-Heuristic (`LateAcceptanceCrossDomainHyperHeuristic`) is the public scientific identity associated with
Jackson, Ozcan & Drake (2013), *Late acceptance-based selection hyper-heuristics for cross-domain heuristic search*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `late-acceptance-selection-hh-jackson-ozcan-drake-2013`
- Class: `LateAcceptanceCrossDomainHyperHeuristicOptimizer`
- Parameters: `LateAcceptanceCrossDomainHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.152.0
- Primary DOI/permanent identifier: `10.1109/UKCI.2013.6651310`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

A choice-function score balances learned performance and recency, while late acceptance provides move acceptance.

## Parameters

`LateAcceptanceCrossDomainHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new LateAcceptanceCrossDomainHyperHeuristicOptimizer().Optimize(
        domain,
        new LateAcceptanceCrossDomainHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`late-acceptance-selection-hh-jackson-ozcan-drake-2013`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}C_i(t)&=S_i(t)+\lambda(t-\ell_i(t)),\\h_t&=\arg\max_iC_i(t).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Jackson, Ozcan & Drake (2013), *Late acceptance-based selection hyper-heuristics for cross-domain heuristic search*, 13th UK Workshop on Computational Intelligence.
DOI/permanent identifier: `10.1109/UKCI.2013.6651310`.
