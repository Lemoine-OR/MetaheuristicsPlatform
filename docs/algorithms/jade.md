# JADE — Adaptive Differential Evolution With Optional External Archive

## Reference

Jingqiao Zhang and Arthur C. Sanderson,
"JADE: Adaptive Differential Evolution With Optional External Archive",
IEEE Transactions on Evolutionary Computation, 13(5), 945-958, 2009.
DOI: 10.1109/TEVC.2009.2014613.

## Mutation

JADE uses:

```text
DE/current-to-pbest/1
```

For target x_i:

```text
v_i =
    x_i
    + F_i (x_pbest - x_i)
    + F_i (x_r1 - x_r2)
```

`x_pbest` is chosen uniformly from the best `ceil(p * NP)` members of the current
population.

`r1` is drawn from the current population excluding the target.

With the external archive enabled, `r2` is drawn from:

```text
current population union archive
```

while respecting the DE current-population index exclusions.

## External archive

When a trial strictly improves its parent:
- the replaced parent is inserted into the external archive;
- if archive capacity is already full, the insertion uses the exact
  append-then-uniform-random-removal semantics without allocating a temporary vector.

Archive capacity equals the current population size in v0.14.0.

The archive is optional as in the original JADE algorithm.

## Parameter adaptation

Each generation starts with global means:

```text
mu_F
mu_CR
```

Per target:

```text
CR_i ~ Normal(mu_CR, 0.1)
CR_i <- clip(CR_i, 0, 1)

F_i ~ Cauchy(mu_F, 0.1)
resample while F_i <= 0
F_i <- min(F_i, 1)
```

After strict improvements, successful values form `S_CR` and `S_F`.

```text
mu_CR <-
    (1-c) mu_CR
    + c mean_A(S_CR)

mu_F <-
    (1-c) mu_F
    + c mean_L(S_F)
```

where:

```text
mean_L(S_F) = sum(F^2) / sum(F)
```

The default adaptation rate is:

```text
c = 0.1
```

## Boundary handling

The canonical default is `MidpointToTarget`.

For component j:

```text
if v_j < lower_j:
    v_j = (lower_j + x_i,j) / 2

if v_j > upper_j:
    v_j = (upper_j + x_i,j) / 2
```

Clamp and reflection remain explicit experimental alternatives.

## Deterministic parallel archive semantics

JADE uses a two-phase selection design:

```text
1. parallel compare parent/trial
2. deterministic archive insertion of successful old parents
3. parallel commit selected trials
4. update mu_F / mu_CR from strict successes
```

This prevents concurrent archive mutation and guarantees that the archived vector is the
old parent rather than the already-overwritten trial.

Target-owned random streams preserve sequential/parallel reproducibility.

## Allocation behavior

The generation loop reuses:
- flat parent/trial population arrays;
- flat F/CR arrays;
- ranking indices;
- selection/success flags;
- feedback array;
- flat external archive.

No per-target vector or parameter objects are allocated inside the hot generation loop.

## Relationship to jDE

jDE:
- stores persistent F_i / CR_i with each individual;
- proposes changes independently;
- inherits proposals only after successful selection.

JADE:
- samples F_i / CR_i from global evolving distributions;
- learns those distributions from successful trials;
- introduces p-best guidance and an optional historical archive.