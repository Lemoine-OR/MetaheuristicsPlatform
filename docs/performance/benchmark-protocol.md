# Performance benchmark protocol

## Purpose

Performance decisions in MetaheuristicsPlatform must be based on measurements rather
than assumptions.

The PSO calibration suite measures:
- end-to-end runtime;
- sequential versus parallel execution;
- objective-cost sensitivity;
- topology/influence cost;
- managed allocations.

## Environment

For comparable measurements:
- build and run `Release`;
- do not attach the Visual Studio debugger;
- keep the machine on a stable performance/power profile;
- avoid running other CPU-intensive workloads;
- use the same machine when comparing changes;
- record CPU/runtime information with the benchmark artifacts.

BenchmarkDotNet performs warmup and repeated measurements to reduce JIT and transient
noise.

## Scientific fairness

Performance and optimization quality are distinct.

Timing comparisons use:
- identical problem;
- identical swarm size;
- identical dimension;
- identical iteration budget;
- identical seed;
- callbacks disabled unless callbacks themselves are under study.

Algorithm-quality comparisons will later use multiple independent seeds and benchmark
functions; a single deterministic timing seed is not a quality experiment.

## Parallel crossover calibration

`PsoParallelCalibrationBenchmarks` includes representative workloads around and above
the current default `particleCount * dimension = 8192` Auto threshold.

The threshold will be changed only after examining actual crossover results.

## Objective evaluation

`PsoObjectiveCostCalibrationBenchmarks` compares a cheap Sphere objective with a more
CPU-intensive pure objective.

This determines whether movement parallelism and objective parallelism should share
one threshold or use separate policies.

## Topology and influence

`PsoSocialTopologyCalibrationBenchmarks` compares:
- canonical fully connected;
- canonical ring;
- FIPS fully connected;
- FIPS ring;
- DCluster + FIPS.

This helps identify when social-information processing rather than objective evaluation
dominates runtime.

## Running

From the repository root:

```powershell
.\benchmarks\run-pso-calibration.ps1 -Suite Parallel
```

Then, if needed:

```powershell
.\benchmarks\run-pso-calibration.ps1 -Suite Objective
.\benchmarks\run-pso-calibration.ps1 -Suite Social
```

Run all suites only when a complete calibration is wanted:

```powershell
.\benchmarks\run-pso-calibration.ps1 -Suite All
```

Artifacts are copied to:

```text
benchmarks\Reports\PsoCalibration\<timestamp>\
```

## Next decisions

The results determine:
1. `PsoExecutionOptions.MinimumParallelWork`;
2. whether objective parallelism needs an independent threshold;
3. whether additional topology-specific kernels are justified;
4. whether SIMD experiments should target objective functions, movement, or both;
5. where remaining allocations matter enough to remove.