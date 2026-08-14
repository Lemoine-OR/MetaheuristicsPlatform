# Reproducibility

## Goal

A published or benchmarked run should be reproducible from:
- algorithm identity and version;
- problem instance;
- algorithm-specific parameters;
- generic runtime options;
- deterministic seed;
- random-source implementation.

## Platform rule

Metaheuristics consume randomness from `OptimizationContext<TSolution>.Random`.

Algorithm implementations must not create hidden `System.Random` instances.

## Default PRNG

The platform default is Xoshiro256** with a 256-bit internal state.
The state is initialized from the user-visible 64-bit seed using SplitMix64.

The implementation is non-cryptographic and is intended for simulation,
optimization and research workloads.

## Custom generators

Users can replace the generator by supplying another `IRandomSourceFactory`
through `OptimizationOptions.RandomSourceFactory`.

This keeps algorithm code independent from the concrete pseudo-random generator.

## Seed recording

Every `OptimizationResult<TSolution>` stores the seed used for the run.

Later benchmark and experiment infrastructure will record the generator identity
together with the seed and complete parameter set.