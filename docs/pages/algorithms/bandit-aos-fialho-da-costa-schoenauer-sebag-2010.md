@page bandit_aos_fialho_da_costa_schoenauer_sebag_2010 Bandit-Based Adaptive Operator Selection

# Bandit-Based Adaptive Operator Selection

## General description

Bandit-Based Adaptive Operator Selection (`BanditAosHyperHeuristic`) is the public scientific identity associated with
Fialho, Da Costa, Schoenauer & Sebag (2010), *Analyzing bandit-based adaptive operator selection mechanisms*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `bandit-aos-fialho-da-costa-schoenauer-sebag-2010`
- Class: `BanditAosHyperHeuristicOptimizer`
- Parameters: `BanditAosHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.148.0
- Primary DOI/permanent identifier: `10.1007/s10472-010-9213-y`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

Each low-level heuristic is a bandit arm whose empirical mean reward is balanced against an upper-confidence exploration bonus.

## Parameters

`BanditAosHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new BanditAosHyperHeuristicOptimizer().Optimize(
        domain,
        new BanditAosHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`bandit-aos-fialho-da-costa-schoenauer-sebag-2010`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}\bar r_i(t+1)&=\bar r_i(t)+\frac{r_t-\bar r_i(t)}{n_i(t+1)},\\h_t&=\arg\max_i\left(\bar r_i(t)+c\sqrt{\frac{\log t}{n_i(t)}}\right).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Fialho, Da Costa, Schoenauer & Sebag (2010), *Analyzing bandit-based adaptive operator selection mechanisms*, Annals of Mathematics and Artificial Intelligence 60, 25-64.
DOI/permanent identifier: `10.1007/s10472-010-9213-y`.
