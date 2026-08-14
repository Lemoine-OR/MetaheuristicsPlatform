# Differential Evolution parallel calibration

## Initial observation

The first end-to-end DE benchmark showed strong parallel speedups:
- 64x32 already favored parallel execution;
- larger dimensions and populations increased the gain substantially.

However, that benchmark forced both:
- DE variation;
- objective evaluation

into parallel mode.

Because evaluation already has its own generic execution policy, DE-specific Auto
must be calibrated against variation independently.

## VariationCrossover

Fixed dimension 32:

```text
16x32 = 512
24x32 = 768
32x32 = 1024
40x32 = 1280
48x32 = 1536
56x32 = 1792
64x32 = 2048
80x32 = 2560
```

Objective evaluation is always sequential.

## VariationShape

Constant product 2048:

```text
16x128
32x64
64x32
128x16
```

This determines whether a scalar work threshold is sufficient for DE variation.

## EndToEnd

The original four sizes are retained to measure the complete effect of parallel
variation plus adaptive-capable evaluation.

## Decision rule

Do not change `DeExecutionOptions.Auto` until VariationCrossover and VariationShape
have both been measured on the reference machine.