@page ils_hyperheuristic_soria_alcaraz_ochoa_sotelo_burke_2017 ILS Hyper-Heuristic with Effective Heuristic Subset

# ILS Hyper-Heuristic with Effective Heuristic Subset

## General description

ILS Hyper-Heuristic with Effective Heuristic Subset (`IlsBanditHyperHeuristic`) is the public scientific identity associated with
Soria-Alcaraz, Ochoa, Sotelo-Figeroa & Burke (2017), *A methodology for determining an effective subset of heuristics in selection hyper-heuristics*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017`
- Class: `IlsBanditHyperHeuristicOptimizer`
- Parameters: `IlsBanditHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.151.0
- Primary DOI/permanent identifier: `10.1016/j.ejor.2017.01.042`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

A bandit model identifies an effective low-level heuristic subset; the search iterates within that subset and periodically perturbs/refines it.

## Parameters

`IlsBanditHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new IlsBanditHyperHeuristicOptimizer().Optimize(
        domain,
        new IlsBanditHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}\mathcal H_t^\star&=\operatorname{SelectSubset}(\mathcal H,\bar r,n),\\h_t&=\arg\max_{h\in\mathcal H_t^\star}\left(\bar r_h+c\sqrt{\frac{2\log t}{n_h}}\right).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Soria-Alcaraz, Ochoa, Sotelo-Figeroa & Burke (2017), *A methodology for determining an effective subset of heuristics in selection hyper-heuristics*, European Journal of Operational Research 260(3), 972-983.
DOI/permanent identifier: `10.1016/j.ejor.2017.01.042`.
