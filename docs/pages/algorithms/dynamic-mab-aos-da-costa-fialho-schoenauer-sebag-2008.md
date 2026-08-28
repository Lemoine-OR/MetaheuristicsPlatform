@page dynamic_mab_aos_da_costa_fialho_schoenauer_sebag_2008 Dynamic Multi-Armed Bandit Adaptive Operator Selection

# Dynamic Multi-Armed Bandit Adaptive Operator Selection

## General description

Dynamic Multi-Armed Bandit Adaptive Operator Selection (`DynamicMabHyperHeuristic`) is the public scientific identity associated with
Da Costa, Fialho, Schoenauer & Sebag (2008), *Adaptive Operator Selection with Dynamic Multi-Armed Bandits*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008`
- Class: `DynamicMabHyperHeuristicOptimizer`
- Parameters: `DynamicMabHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.146.0
- Primary DOI/permanent identifier: `10.1145/1389095.1389272`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

Low-level heuristics are arms of a dynamic multi-armed bandit; UCB selection is coupled to a change statistic that can reset stale credit.

## Parameters

`DynamicMabHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new DynamicMabHyperHeuristicOptimizer().Optimize(
        domain,
        new DynamicMabHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}h_t&=\arg\max_i\left(\bar r_i+c\sqrt{\frac{2\log t}{n_i}}\right),\qquad D_t>\tau\Longrightarrow(n_i,\bar r_i)\leftarrow(0,0).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Da Costa, Fialho, Schoenauer & Sebag (2008), *Adaptive Operator Selection with Dynamic Multi-Armed Bandits*, GECCO 2008.
DOI/permanent identifier: `10.1145/1389095.1389272`.
