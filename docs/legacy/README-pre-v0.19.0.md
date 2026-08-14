# MetaheuristicsPlatform

High-performance, research-oriented C# platform for generic metaheuristics.

> Status: Core, PSO topology, PSO social model and PSO high-performance runtime are implemented.
> PSO Dynamics is next.

## Performance is an architectural requirement

The production PSO runtime uses:

- flat particle-major `double[]` buffers;
- `Span<double>` / `ReadOnlySpan<double>` particle views;
- one random stream per particle;
- deterministic stream derivation from the run seed;
- coarse contiguous range partitioning for CPU parallelism;
- no per-particle `Task.Run`;
- no per-iteration jagged-array construction;
- no locks in normal synchronous particle movement.

This allows synchronous movement to be parallelized while preserving reproducibility.

## Parallelism model

```text
swarm
  ├─ range 0  -> particles [0..a)
  ├─ range 1  -> particles [a..b)
  ├─ ...
  └─ range k  -> particles [x..N)
```

A worker receives a range and executes an ordinary tight `for` loop over that range.

`Auto` execution uses a workload threshold because parallel execution is not always
faster for small loops.

## Randomness

Random streams are attached to particle identities rather than worker threads.

Therefore the random sequence consumed by particle `i` is stable regardless of which
thread executes particle `i`.

## Next

PSO Dynamics:
- inertia;
- Clerc-Kennedy constriction;
- fused built-in movement kernels;
- velocity initialization and clamping;
- boundary policies;
- SIMD where benchmarks show a gain;
- synchronous engine;
- parallel objective evaluation contract.