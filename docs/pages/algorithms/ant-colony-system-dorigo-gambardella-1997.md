@page ant_colony_system_dorigo_gambardella_1997 Ant Colony System - Dorigo-Gambardella

# Ant Colony System - Dorigo-Gambardella

## General description

Ant Colony System (ACS) is the 1997 Dorigo-Gambardella refinement of Ant System.
This implementation keeps the platform's generic constructive-domain contract while
adding the ACS pseudo-random proportional transition rule, local pheromone update,
and best-so-far global reinforcement.

## Technical specifications

- Stable ID: `ant-colony-system-dorigo-gambardella-1997`
- Class: `AntColonySystemOptimizer<TSolution,TComponent,TPheromoneKey,TEnumerator>`
- Family: Swarm intelligence / constructive
- Public since: v0.45.0
- Scientific component page: @subpage advanced_ant_colony_optimization

## Complexity

For each ant, every construction step scans the feasible candidate set. The full
iteration cost is therefore proportional to the sum of feasible-candidate scans,
plus one objective evaluation per completed ant.

## Applicability

Finite constructive discrete search spaces for which the domain supplies feasible
next components, positive heuristic information, and stable pheromone keys.

## Detailed operation

At each construction step ACS first draws \f$q\f$. With probability controlled by
\f$q_0\f$, the highest \f$\tau\eta^\beta\f$ candidate is selected; otherwise a
proportional categorical draw is made. The selected trail immediately receives the
local update. After the colony has completed, the best-so-far path receives the ACS
global update.

## Parameters

`AntCount`, `MaximumIterations`, `Beta`, `ExploitationProbability`,
`GlobalEvaporationRate`, `LocalUpdateRate`, `InitialPheromone`, and
`MaximumConstructionSteps`.

## API example

```csharp
var acs =
    new AntColonySystemOptimizer<MySolution, MyComponent, MyKey, MyEnumerator>(
        constructionModel,
        depositPolicy);

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.AntColonySystem,
    () => acs,
    replace: true);

var parameters =
    new AntColonySystemParameters
    {
        AntCount = 20,
        MaximumIterations = 200,
        Beta = 2.0,
        ExploitationProbability = 0.9,
        GlobalEvaporationRate = 0.1,
        LocalUpdateRate = 0.1,
        InitialPheromone = 0.1
    };
```

## Stable factory ID

`ant-colony-system-dorigo-gambardella-1997`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)
\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

\f[
\begin{aligned}
j&=
\begin{cases}
\arg\max_{u\in\mathcal N(s)}\tau_u\eta_u^\beta,&q\le q_0,\\
J,&q>q_0,
\end{cases}\\
\tau_e&\leftarrow(1-\xi)\tau_e+\xi\tau_0,\\
\tau_e&\leftarrow(1-\rho)\tau_e+\rho\Delta\tau_e^{gb},
\qquad e\in s^{gb}.
\end{aligned}
\f]

### Assumptions

The construction model must expose a finite nonempty feasible candidate set until
completion. Pheromone values are strictly positive, and heuristic information must
be strictly positive when \f$\beta>0\f$.

### Convergence conditions

No universal finite-time global convergence claim is made. The implementation
faithfully preserves persistent stochastic construction when \f$q_0<1\f$, but
problem-specific convergence guarantees require additional assumptions.

### Scientific references

Dorigo & Gambardella (1997), *Ant Colony System: A Cooperative Learning Approach
to the Traveling Salesman Problem*, IEEE Transactions on Evolutionary Computation
1(1), 53-66. DOI: `10.1109/4235.585892`.
