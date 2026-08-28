@page extreme_value_dmab_fialho_da_costa_schoenauer_sebag_2009 Extreme-Value Dynamic Multi-Armed Bandit AOS

# Extreme-Value Dynamic Multi-Armed Bandit AOS

## General description

Extreme-Value Dynamic Multi-Armed Bandit AOS (`ExtremeValueMabHyperHeuristic`) is the public scientific identity associated with
Fialho, Da Costa, Schoenauer & Sebag (2009), *Dynamic Multi-Armed Bandits and Extreme Value-Based Rewards for Adaptive Operator Selection in Evolutionary Algorithms*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009`
- Class: `ExtremeValueMabHyperHeuristicOptimizer`
- Parameters: `ExtremeValueMabHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.147.0
- Primary DOI/permanent identifier: `10.1007/978-3-642-11169-3_13`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

Operator credit is the extreme recent improvement in a bounded reward window and is combined with upper-confidence exploration.

## Parameters

`ExtremeValueMabHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new ExtremeValueMabHyperHeuristicOptimizer().Optimize(
        domain,
        new ExtremeValueMabHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}R_i(t)&=\max\{r_i(s):s\in W_t\},\\h_t&=\arg\max_i\left(R_i(t)+c\sqrt{\frac{2\log t}{n_i}}\right).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Fialho, Da Costa, Schoenauer & Sebag (2009), *Dynamic Multi-Armed Bandits and Extreme Value-Based Rewards for Adaptive Operator Selection in Evolutionary Algorithms*, Learning and Intelligent Optimization.
DOI/permanent identifier: `10.1007/978-3-642-11169-3_13`.
