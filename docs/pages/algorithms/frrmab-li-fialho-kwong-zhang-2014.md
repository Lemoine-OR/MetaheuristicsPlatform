@page frrmab_li_fialho_kwong_zhang_2014 Fitness-Rate-Rank Multi-Armed Bandit

# Fitness-Rate-Rank Multi-Armed Bandit

## General description

Fitness-Rate-Rank Multi-Armed Bandit (`FrrmabHyperHeuristic`) is the public scientific identity associated with
Li, Fialho, Kwong & Zhang (2014), *Adaptive Operator Selection With Bandits for a Multiobjective Evolutionary Algorithm Based on Decomposition*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `frrmab-li-fialho-kwong-zhang-2014`
- Class: `FrrmabHyperHeuristicOptimizer`
- Parameters: `FrrmabHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.149.0
- Primary DOI/permanent identifier: `10.1109/TEVC.2013.2239648`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

A sliding window accumulates fitness-improvement rewards, ranks operators by recent fitness-rate credit and combines rank credit with bandit exploration.

## Parameters

`FrrmabHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new FrrmabHyperHeuristicOptimizer().Optimize(
        domain,
        new FrrmabHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`frrmab-li-fialho-kwong-zhang-2014`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}q_i&=\sum_{s\in W_t:h_s=i}r_s,\qquad c_i=q_i\delta^{\operatorname{rank}(i)},\\h_t&=\arg\max_i\left(c_i+c\sqrt{\frac{2\log t}{n_i}}\right).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Li, Fialho, Kwong & Zhang (2014), *Adaptive Operator Selection With Bandits for a Multiobjective Evolutionary Algorithm Based on Decomposition*, IEEE Transactions on Evolutionary Computation 18(1), 114-130.
DOI/permanent identifier: `10.1109/TEVC.2013.2239648`.
