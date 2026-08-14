# SHADE — Success-History Based Adaptive Differential Evolution

## Reference

Ryoji Tanabe and Alex Fukunaga,
"Success-History Based Parameter Adaptation for Differential Evolution",
2013 IEEE Congress on Evolutionary Computation, pp. 71-78.
DOI: 10.1109/CEC.2013.6557555.

Ryoji Tanabe's publication page also distributes the original SHADE source code and
later corrected SHADE 1.1.1 implementations.

## Relationship to JADE

SHADE keeps the successful JADE search structure:

```text
current-to-pbest/1/bin
external archive
Cauchy sampling for F
Normal sampling for CR
```

but replaces JADE's two evolving scalar means with historical memories.

## Historical memories

SHADE stores:

```text
M_F[0 ... H-1]
M_CR[0 ... H-1]
```

The canonical defaults exposed by this implementation are:

```text
NP = 100
H = 100
p = 0.2

M_F[h]  = 0.5
M_CR[h] = 0.5
```

At each target, one memory index r is selected uniformly.

```text
CR_i ~ Normal(M_CR[r], 0.1)
CR_i <- clip(CR_i, 0, 1)

F_i ~ Cauchy(M_F[r], 0.1)
resample while F_i <= 0
F_i <- min(F_i, 1)
```

## Success history

Only strict objective improvements contribute to success history.

For a successful trial k:

```text
Delta_f[k] = | f(parent_k) - f(trial_k) |
```

Weights are normalized improvements:

```text
w[k] =
    Delta_f[k]
    / sum_j Delta_f[j]
```

The next circular memory entry receives:

```text
M_CR[position] =
    sum_k w[k] * CR[k]
```

and:

```text
M_F[position] =
    sum_k w[k] * F[k]^2
    --------------------
    sum_k w[k] * F[k]
```

The second expression is the improvement-weighted Lehmer mean.

If the generation produces no strict successes, the memories and circular position do
not change.

## p-best mutation

For target x_i:

```text
v_i =
    x_i
    + F_i * (x_pbest - x_i)
    + F_i * (x_r1 - x_r2)
```

`x_pbest` is selected randomly from the best `ceil(p * NP)` population members.

`r1` is selected from the current population.

`r2` is selected from the current population union external archive, respecting the
current-population index exclusions.

## External archive

Strictly replaced parents are archived before their population slots are overwritten.

The implementation uses the corrected deterministic archive ownership semantics
established for JADE:

```text
1. compare parent/trial in parallel
2. archive successful old parents in deterministic target order
3. commit selected trials in parallel
4. update success-history memory
```

Archive capacity equals NP.

## Equal-fitness trials

A trial equal in fitness to its parent may be selected according to DE's non-worsening
replacement rule, but it:

```text
is not added to the success history
is not added to the external archive
does not advance the historical-memory position
```

This cleanly separates replacement from adaptation evidence.

## Parallelism

SHADE reuses the calibrated DE variation policy and the generic evaluation policy.

Random streams are owned by target indices, not worker threads, so sequential and
parallel scheduling follow the same stochastic trajectory.

## No per-target hot-loop allocation

Reusable structures include:

```text
parent/trial population buffers
parent/trial fitness
F/CR buffers
ranking indices
success feedback
selection flags
success flags
external archive
M_F / M_CR memory
```

No solution vectors are allocated per target inside the generation loop.

## Next extension: L-SHADE

L-SHADE will retain SHADE's success-history mechanism and add an evaluation-budget
driven linear reduction of active population size.

The generic `LinearDePopulationSizeReductionPolicy` introduced in v0.12.0 is already
available for that integration.