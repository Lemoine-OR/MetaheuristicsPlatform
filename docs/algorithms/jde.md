# jDE — Self-Adaptive Differential Evolution

## Reference

Janez Brest, Saso Greiner, Borko Boskovic, Marjan Mernik, Viljem Zumer,
"Self-Adapting Control Parameters in Differential Evolution:
A Comparative Study on Numerical Benchmark Problems",
IEEE Transactions on Evolutionary Computation, 10(6), 646-657, 2006.
DOI: 10.1109/TEVC.2006.872133.

## Canonical search strategy

The v0.13.0 implementation uses the algorithmic scheme described in the paper:

```text
DE/rand/1/bin
```

For each target i, jDE stores:

```text
x_i
F_i
CR_i
```

The control parameters are part of the inherited state of the individual.

## Parameter proposal

Before mutation/crossover, four independent uniform random variables are drawn.

```text
F_trial =
    F_lower + rand1 * F_range
    if rand2 < tau1
    else F_parent

CR_trial =
    rand3
    if rand4 < tau2
    else CR_parent
```

Defaults:

```text
F_initial = 0.5
CR_initial = 0.9

F_lower = 0.1
F_range = 0.9

tau1 = 0.1
tau2 = 0.1
```

Thus newly generated F lies in `[0.1, 1.0)` and newly generated CR lies in `[0, 1)`.

## Inheritance

The proposed F and CR values are used to generate the trial vector.

If and only if the trial strictly improves the parent:

```text
x_parent <- x_trial
F_parent <- F_trial
CR_parent <- CR_trial
```

Otherwise all three inherited values remain unchanged.

This success-linked inheritance is the essential self-adaptive mechanism.

## Runtime implementation

The hot state is flat:

```text
parent population
trial population
parent fitness
trial fitness

parent F
parent CR
trial F
trial CR
```

There is no per-individual object allocation inside the generation loop.

The same target-owned random stream is used for:
- jDE parameter adaptation;
- donor sampling;
- forced crossover dimension;
- binomial crossover draws.

Because streams are target-owned rather than worker-owned, parallel scheduling does not
change the random trajectory of a target.

## Parallel execution

jDE reuses:
- calibrated `DeExecutionOptions` for variation/selection;
- generic `EvaluationExecutionOptions` for objective evaluation.

The two decisions remain independent.

## Boundary handling

The canonical default is clamp, matching the boundary treatment described for the
classical DE used in the paper.

Reflection is retained as an explicit platform option for controlled experiments.

## Relationship to later variants

jDE adapts parameters through inheritance at the individual level.

It does not use:
- p-best guidance;
- an external archive;
- current-generation success means;
- success-history memory;
- population-size reduction.

Those mechanisms belong to JADE, SHADE and L-SHADE and remain separate.