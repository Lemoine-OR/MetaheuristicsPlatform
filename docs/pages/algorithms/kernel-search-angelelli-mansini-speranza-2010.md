@page kernel_search_angelelli_mansini_speranza_2010 Kernel Search

# Kernel Search

## General description

Kernel Search (`KernelSearchMatheuristic`) is the public scientific identity associated with
Angelelli, Mansini & Speranza (2010), *Kernel search: A general heuristic for the multi-dimensional knapsack problem*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The kernel/bucket exact-subproblem mechanism is preserved; ranking uses generic relaxation values and reduced costs.

## Technical specifications

- Stable ID: `kernel-search-angelelli-mansini-speranza-2010`
- Class: `KernelSearchMatheuristicOptimizer`
- Parameters: `KernelSearchMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.160.0
- Primary DOI/permanent identifier: `10.1016/j.cor.2010.02.002`

## Complexity

One relaxation solve plus one exact restricted solve per bucket.

Space: O(n) ranking/kernel/bucket state plus exact-solver state.

## Applicability

Binary-selection MILPs where relaxation values/reduced costs can rank promising active variables.

## Detailed operation

Ranks binary variables from relaxation information, solves exact subproblems on a kernel plus one bucket, and promotes useful bucket variables into the kernel.

## Parameters

`KernelSearchMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new KernelSearchMatheuristicOptimizer().Optimize(
        domain,
        new KernelSearchMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`kernel-search-angelelli-mansini-speranza-2010`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X,\ x_B\in\{0,1\}^{|B|}\}
\f]

### Update equations / iterations

\f[
\begin{aligned}K_0&=\operatorname{top}_q(\rho_j),\\x^t&\in\arg\min\{c^\top x:x\in X,\operatorname{supp}(x)\subseteq K_t\cup B_t\},\\K_{t+1}&=K_t\cup(\operatorname{supp}(x^t)\cap B_t).\end{aligned}
\f]

### Assumptions

The domain exposes finite objective values, variable-kind metadata, integer-feasibility testing,
a feasible incumbent constructor, and deterministic exact/relaxation solve callbacks for a fixed
platform seed and solver configuration.

### Convergence conditions

No universal finite-time global-convergence claim is asserted for the matheuristic wrapper.
Whenever an exact restricted subproblem is solved to optimality by the supplied domain, the
returned point is exact only for that restricted subproblem; global optimality is not implied.

### Scientific references

Angelelli, Mansini & Speranza (2010), *Kernel search: A general heuristic for the multi-dimensional knapsack problem*, Computers & Operations Research 37(11), 2017-2026.
DOI/permanent identifier: `10.1016/j.cor.2010.02.002`.
