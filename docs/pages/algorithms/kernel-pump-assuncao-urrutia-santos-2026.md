@page kernel_pump_assuncao_urrutia_santos_2026 Kernel Pump

# Kernel Pump

## General description

Kernel Pump (`KernelPumpMatheuristic`) is the public scientific identity associated with
Assunção, Urrutia & Santos (2026), *Kernel pump*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The 2026 kernel-and-bucket feasibility-pump decomposition is preserved; FeasOpt details and CPLEX-specific controls are abstracted behind platform solver callbacks.

## Technical specifications

- Stable ID: `kernel-pump-assuncao-urrutia-santos-2026`
- Class: `KernelPumpMatheuristicOptimizer`
- Parameters: `KernelPumpMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.165.0
- Primary DOI/permanent identifier: `10.1007/s12532-026-00333-2`

## Complexity

One initial relaxation plus restricted pump iterations across progressively enlarged buckets.

Space: O(n) kernel/buckets/targets plus relaxation-solver state.

## Applicability

Generic MILP feasibility search with binary variables, relaxation values/reduced costs and restricted pump subproblems.

## Detailed operation

Partitions binary variables into an initial kernel and ranked buckets using relaxation integrality distance and reduced-cost information, then runs restricted feasibility-pump projections while progressively admitting buckets.

## Parameters

`KernelPumpMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new KernelPumpMatheuristicOptimizer().Optimize(
        domain,
        new KernelPumpMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`kernel-pump-assuncao-urrutia-santos-2026`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:Ax\ge b,\ x_B\in\{0,1\}^{|B|},\ x_I\in\mathbb Z^{|I|}\}
\f]

### Update equations / iterations

\f[
\begin{aligned}d_j&=|\bar x_j-[\bar x_j]|,\\(K,\Lambda_1,\ldots,\Lambda_q)&=\operatorname{rank}(d_j,rc_j),\\x^{k+1}&\in\arg\min_{x\in P(K\cup\Lambda_1\cup\cdots\cup\Lambda_t)}\Delta(x,\tilde x^k).\end{aligned}
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

Assunção, Urrutia & Santos (2026), *Kernel pump*, Mathematical Programming Computation (online first, 2026).
DOI/permanent identifier: `10.1007/s12532-026-00333-2`.
