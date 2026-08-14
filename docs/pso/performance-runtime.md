# PSO high-performance runtime

## Performance requirements

The platform targets research workloads where the optimizer may be:
- the primary solver;
- executed thousands of times;
- used as a subproblem solver;
- embedded in experiments with many replications.

Performance is therefore part of correctness of the architecture.

## Memory layout

Production PSO state is stored in flat particle-major buffers:

```text
positions:
[p0_d0 p0_d1 ... p0_dD | p1_d0 p1_d1 ... | ...]
```

The same layout is used for:
- velocity;
- personal-best position.

Advantages:
- one allocation per buffer;
- no `double[]` object per particle;
- contiguous dimension traversal;
- easy `Span<double>` slicing;
- good compatibility with future SIMD kernels;
- contiguous particle ranges for CPU workers.

## CPU parallelism

The runtime does not create one `Task` per particle.

Instead it creates coarse contiguous ranges and each worker executes a normal `for`
loop inside its range.

This reduces scheduling and delegate overhead for small particle-update bodies.

## Automatic mode

Parallel execution is not guaranteed to be faster for small workloads.

`PsoExecutionOptions.Mode = Auto` therefore uses a tunable work threshold based on
`particleCount * dimension`.

The default is deliberately treated as a starting heuristic. BenchmarkDotNet suites
will calibrate recommended thresholds on representative dimensions and swarm sizes.

## Deterministic parallel random streams

Each particle owns one PRNG stream derived from:

```text
run seed + particle identity
```

A particle does not use a thread-local random source.

Consequences:
- thread scheduling does not change a particle's stochastic sequence;
- sequential and parallel synchronous execution can consume the same per-particle streams;
- deterministic replication remains possible.

## Synchronization

The forthcoming synchronous PSO kernel will ensure:
- each worker writes only its particles' current position/velocity;
- personal-best data used as social information is read from a stable iteration state;
- reductions and best updates happen outside hot dimension loops;
- normal movement requires no locks.

## SIMD

Flat contiguous buffers are designed for later `Vector<double>` or hardware-intrinsic
kernels.

SIMD is not forced blindly: fused kernels will be benchmarked because RNG generation,
boundary handling and topology-dependent access can dominate simple vector arithmetic.