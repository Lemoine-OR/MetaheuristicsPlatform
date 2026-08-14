# Differential Evolution Auto execution policy

## Calibration result

The DE variation kernel was calibrated independently from objective evaluation.

On the 16-logical-core reference machine, dimension-32 crossover measurements showed:

```text
32 x 32 = 1024 : parallel slower
40 x 32 = 1280 : parallel slower
48 x 32 = 1536 : parallel faster
56 x 32 = 1792 : parallel faster
64 x 32 = 2048 : parallel faster
80 x 32 = 2560 : parallel faster
```

A fixed-work shape experiment at 2048 components showed:

```text
16 x 128 : parallel slower
32 x 64  : parallel faster
64 x 32  : parallel faster
128 x 16 : parallel faster
```

Therefore total scalar work alone is not sufficient.

## Policy

`DeAutoExecutionPolicy` requires both:

```text
minimumPopulation = max(16, 2 * logicalProcessorCount)
minimumWork       = max(768, 96 * logicalProcessorCount)
```

and parallelizes variation only when:

```text
populationSize >= minimumPopulation
and
populationSize * dimension >= minimumWork
```

On the reference 16-thread machine this becomes:

```text
populationSize >= 32
and
populationSize * dimension >= 1536
```

## Why this differs from PSO

PSO and DE have different hot-loop structures.

DE performs:
- distinct donor sampling;
- differential mutation;
- crossover;
- boundary handling;
- one-to-one selection.

PSO performs:
- velocity dynamics;
- social influence/topology access;
- position updates;
- personal-best logic.

The platform therefore keeps algorithm-specific variation execution policies while
sharing generic objective-evaluation execution.

## Explicit override

A positive `DeExecutionOptions.MinimumParallelWork` keeps the legacy scalar override.

This is useful for:
- unusual machines;
- experiments;
- custom runtimes;
- future machine-specific calibration.