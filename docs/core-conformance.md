# Core conformance across metaheuristic search models

## Principle

Metaheuristic classification and software inheritance are intentionally separate.

The platform does not define:

```text
Metaheuristic
  -> PopulationMetaheuristic
      -> SwarmMetaheuristic
          -> PSO
```

Instead, every algorithm uses the same common lifecycle and declares descriptive
classification metadata.

## Single-solution methods

A trajectory method can:
1. start one `OptimizationContext`;
2. evaluate a current solution;
3. generate and evaluate candidates;
4. complete iterations;
5. use the common stopping and callback infrastructure.

No population abstraction is required.

## Population-based methods

A population method can:
1. own its internal population representation;
2. evaluate candidates through the same context;
3. use the common best-so-far, counters, callbacks and stopping criteria;
4. expose population-specific state through algorithm data only when requested.

The Core does not dictate whether the population is an array, pooled buffer, list,
struct-of-arrays layout, or another high-performance representation.

## Neighborhood-based methods

Neighborhood structure is algorithm/domain specific and is not forced into a universal
`IEnumerable` abstraction in the Core.

This is deliberate: neighborhood enumeration is often inside the hottest loop of
Tabu Search, VNS, local search and related methods.

Those algorithms still share the same common context while choosing an efficient
neighborhood representation appropriate to their problem.

## Why this matters

The Core standardizes what is genuinely common:
- objective evaluations;
- minimization/maximization;
- best-so-far;
- iteration/evaluation counters;
- random streams;
- callbacks;
- stopping;
- run statistics.

It does not standardize hot algorithmic structures merely for visual uniformity.