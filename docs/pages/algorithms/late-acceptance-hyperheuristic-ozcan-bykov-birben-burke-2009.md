@page late_acceptance_hyperheuristic_ozcan_bykov_birben_burke_2009 Late Acceptance Hyper-Heuristic

# Late Acceptance Hyper-Heuristic

## General description

Late Acceptance Hyper-Heuristic (`LateAcceptanceHyperHeuristic`) is the public scientific identity associated with
Ozcan, Bykov, Birben & Burke (2009), *Examination Timetabling Using Late Acceptance Hyper-heuristics*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009`
- Class: `LateAcceptanceHyperHeuristicOptimizer`
- Parameters: `LateAcceptanceHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.145.0
- Primary DOI/permanent identifier: `10.1109/CEC.2009.4983054`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

A low-level heuristic generates a candidate and late acceptance compares it with both the current objective and a historical objective.

## Parameters

`LateAcceptanceHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new LateAcceptanceHyperHeuristicOptimizer().Optimize(
        domain,
        new LateAcceptanceHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}y_t&=h_t(x_t),\\x_{t+1}&=\begin{cases}y_t,&\widetilde f(y_t)\le\widetilde f(x_t)\text{ or }\widetilde f(y_t)\le H_{t\bmod L},\\x_t,&\text{otherwise.}\end{cases}\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Ozcan, Bykov, Birben & Burke (2009), *Examination Timetabling Using Late Acceptance Hyper-heuristics*, IEEE Congress on Evolutionary Computation 2009, 997-1004.
DOI/permanent identifier: `10.1109/CEC.2009.4983054`.
