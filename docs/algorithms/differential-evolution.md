# Differential Evolution

## Reference

R. Storn and K. Price,
"Differential Evolution — A Simple and Efficient Heuristic for Global Optimization
over Continuous Spaces",
Journal of Global Optimization, 11(4), 341–359, 1997.
DOI: 10.1023/A:1008202821328.

## Platform role

DE is the second production metaheuristic in MetaheuristicsPlatform.

Its purpose is also architectural: it demonstrates that the common lifecycle,
stopping criteria, callbacks, search spaces, deterministic randomness and adaptive
evaluation runtime are not PSO-specific.

## Implemented mutation strategies

### DE/rand/1

```text
v = x_r1 + F (x_r2 - x_r3)
```

### DE/best/1

```text
v = x_best + F (x_r1 - x_r2)
```

### DE/current-to-best/1

```text
v = x_i
    + F (x_best - x_i)
    + F (x_r1 - x_r2)
```

### DE/rand/2

```text
v = x_r1
    + F (x_r2 - x_r3)
    + F (x_r4 - x_r5)
```

Donor indices are distinct and exclude the target individual.

## Crossover

Both classical binomial and exponential crossover are supported.

Binomial crossover forces one random component to come from the mutant vector, so the
trial vector cannot be identical to the target only because every crossover draw failed.

Exponential crossover copies at least one contiguous mutant segment, wrapping at the
dimension boundary.

## Selection semantics

The implementation is synchronous/generational.

All trial vectors are constructed from the same completed parent population.
After all trial fitness values are available, each trial competes only with its
corresponding target.

Ties are accepted.

## Runtime design

Population data is stored in flat target-major arrays.

The implementation does not allocate a full mutant population:
mutation and crossover are fused directly into the trial buffer.

The main arrays are:
- parent population;
- trial population;
- parent fitness;
- trial fitness.

## Parallel determinism

Each target has a deterministic random stream derived from:
- the run seed;
- the target index.

Random streams belong to targets, not worker threads.

Thus sequential and parallel variation generate the same trial vectors for a fixed
seed and completed generation state.

## Objective evaluation

DE reuses the generic `EvaluationExecutor` and `EvaluationCharacteristics`.

This means an expensive objective can be parallelized independently of the DE
variation kernel.

## Future adaptive DE

This release deliberately keeps fixed F and CR as the classical baseline.

Self-adaptive/adaptive families such as jDE, JADE and SHADE should be implemented as
explicit parameter-adaptation policies rather than being hidden inside the baseline
operator.