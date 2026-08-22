@page ant_system_dorigo_maniezzo_colorni_1996 Ant System - Dorigo, Maniezzo and Colorni

# Ant System - Dorigo, Maniezzo and Colorni

Stable ID: `ant-system-dorigo-maniezzo-colorni-1996`

## General description

Ant System (AS) is the original Ant Colony Optimization construction metaheuristic.
A colony of ants repeatedly builds complete feasible solutions. At every construction
decision, a feasible component is sampled proportionally to a product of pheromone
intensity and domain heuristic information. After the colony has completed the iteration,
pheromone evaporates globally and every ant reinforces the components used by its solution.

The v0.44.0 implementation is representation-independent: the application defines
feasible construction components, heuristic information and pheromone keys. It is therefore
not a Traveling-Salesman-specific engine disguised as a generic interface.

## Technical specifications

- **Algorithm:** Ant System (AS).
- **Stable ID:** `ant-system-dorigo-maniezzo-colorni-1996`.
- **Primary reference:** Dorigo, Maniezzo & Colorni (1996).
- **Family:** swarm intelligence + constructive methods.
- **Solution model:** colony/population of independently constructed complete solutions.
- **Transition:** canonical pheromone/heuristic proportional rule.
- **Global update:** evaporation followed by all-ant reinforcement.
- **Sampling implementation:** Gumbel-max in logarithmic space, distributionally equivalent
  to categorical proportional sampling and numerically safer than directly evaluating
  large powers.
- **Pheromone storage:** sparse lazy memory with exact accumulated evaporation for unseen
  keys and O(1) global evaporation.
- **Objective accounting:** exactly one common `OptimizationContext` evaluation per
  completed ant; an external evaluation stopping criterion can terminate inside a colony
  without overshoot.
- **Composition:** the domain supplies the construction model and a deposit policy.

## Complexity

Let \f$m\f$ be the number of ants, \f$L\f$ the number of construction decisions per ant,
and \f$b_t\f$ the number of feasible candidates examined at decision \f$t\f$.

- Construction time per full iteration:
  \f$O(\sum_{k=1}^{m}\sum_{t=1}^{L_k} b_{k,t})\f$, plus objective evaluations.
- Pheromone update time: proportional to the total number of used pheromone keys in the
  completed colony.
- Global evaporation itself is O(1) because the implementation uses lazy decay.
- Memory: O(|P|) for materialized pheromone keys plus the current colony solutions and
  their construction-key paths.

## Applicability

The generic engine targets finite constructive search spaces such as binary, integer,
permutation, combinatorial and mixed discrete representations. Continuous-domain ACO
variants require a different sampling model and are not claimed by this release.

The classical `PositiveInverseObjectiveAntSystemDepositPolicy<TSolution>` implements
the \f$Q/L_k\f$ reinforcement only for finite strictly-positive minimization objectives.
Other objective scales must use an explicit domain-appropriate deposit policy rather than
being silently transformed.

## Detailed operation

For iteration \f$t=1,2,\ldots\f$:

1. every ant starts from a fresh partial solution;
2. while the solution is incomplete, enumerate all currently feasible components;
3. for candidate component \f$c\f$, read its pheromone \f$\tau_c(t)\f$, obtain the domain
   heuristic \f$\eta_c\f$, and sample according to the canonical Ant System probability;
4. evaluate each completed ant solution once through the common optimization lifecycle;
5. after the entire colony completes, evaporate pheromone;
6. apply the configured non-negative deposit of every ant to every pheromone key used in
   that ant solution;
7. complete one common iteration and evaluate the stopping criterion.

A stopping criterion reached after an ant evaluation terminates immediately; a partially
completed colony is deliberately not used for a global pheromone update.

## Parameters

`AntSystemParameters` exposes:

- `AntCount`;
- `MaximumIterations`;
- `Alpha` (\f$\alpha\ge 0\f$);
- `Beta` (\f$\beta\ge 0\f$);
- `EvaporationRate` (\f$0<\rho<1\f$);
- `InitialPheromone` (\f$\tau_0>0\f$);
- `MaximumConstructionSteps`.

The implementation does not introduce MAX-MIN pheromone bounds, ACS local updates,
pseudo-random proportional exploitation, elitist reinforcement or rank-based deposits
under the Ant System identity.

## API example

A generic AS instance requires a typed construction model and a typed deposit policy.
Once configured, it can be registered under the stable factory ID:

```csharp
using MetaheuristicsPlatform.Algorithms.AntColony;
using MetaheuristicsPlatform.Catalog;

var antSystem =
    new AntSystemOptimizer<MySolution, MyComponent, MyPheromoneKey, MyEnumerator>(
        constructionModel,
        new PositiveInverseObjectiveAntSystemDepositPolicy<MySolution>(q: 1.0));

MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.AntSystem,
    () => antSystem,
    replace: true);
```

## Stable factory ID

`MetaheuristicAlgorithmIds.AntSystem` is
`ant-system-dorigo-maniezzo-colorni-1996`.

Because the algorithm requires domain construction components, the runtime factory uses
explicit typed registration rather than pretending that a meaningful parameterless
instance exists.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)
\f]

The generic runtime also supports maximization when the chosen deposit policy is
mathematically meaningful for that objective scale.

### Update equations / iterations

\f[
\begin{aligned}
p(c\mid s,t)
&=
\frac{\tau_c(t)^{\alpha}\eta_c(s)^{\beta}}
{\sum_{u\in\mathcal F(s)}\tau_u(t)^{\alpha}\eta_u(s)^{\beta}},
\\
\tau_e(t+1)
&=
(1-\rho)\tau_e(t)
+
\sum_{k=1}^{m}\Delta\tau_e^{(k)},
\\
\Delta\tau_e^{(k)}
&=
\begin{cases}
D_k,& e\in P_k,\\
0,& e\notin P_k,
\end{cases}
\\
D_k
&=
Q/f(x_k)
\quad\text{for the positive-minimization }Q/L\text{ policy.}
\end{aligned}
\f]

The implementation samples the first equation by maximizing
\f$\log(\tau_c^\alpha\eta_c^\beta)+G_c\f$, where independent \f$G_c\f$ are standard Gumbel
variates. This is exactly the Gumbel-max categorical identity; it changes numerical
implementation, not transition probabilities.

### Assumptions

- the construction model eventually reaches a complete solution;
- every incomplete partial solution exposes at least one feasible next component;
- pheromone is finite and strictly positive;
- heuristic information is finite and strictly positive when \f$\beta>0\f$;
- the deposit policy returns a finite non-negative reinforcement;
- the user-selected pheromone key uniquely represents the decision feature that should
  receive global reinforcement.

### Convergence conditions

The 1996 Ant System paper defines the canonical mechanism but does not justify a universal
finite-time convergence guarantee for arbitrary problem encodings and parameterizations.
This library therefore makes no blanket optimal-convergence claim for the generic AS
implementation. Later ACO convergence analyses and bounded variants have distinct
conditions and identities.

### Scientific references

- Dorigo, M.; Maniezzo, V.; Colorni, A. (1996).
  *Ant System: Optimization by a Colony of Cooperating Agents*.
  IEEE Transactions on Systems, Man, and Cybernetics, Part B 26(1), 29-41.
  DOI `10.1109/3477.484436`.
- Dorigo, M.; Gambardella, L. M. (1997).
  *Ant Colony System: A Cooperative Learning Approach to the Traveling Salesman Problem*.
  IEEE Transactions on Evolutionary Computation 1(1), 53-66.
  DOI `10.1109/4235.585892`. Reviewed for the next ACO layer; not implemented under AS.
- Stützle, T.; Hoos, H. H. (2000).
  *MAX-MIN Ant System*. Future Generation Computer Systems 16(8), 889-914.
  DOI `10.1016/S0167-739X(00)00043-1`. Reviewed/deferred to the advanced ACO release.
