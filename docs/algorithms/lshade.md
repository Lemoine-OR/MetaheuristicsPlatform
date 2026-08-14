# L-SHADE — SHADE with Linear Population Size Reduction

## Reference

Ryoji Tanabe and Alex S. Fukunaga,
"Improving the Search Performance of SHADE Using Linear Population Size Reduction",
2014 IEEE Congress on Evolutionary Computation, pp. 1658-1665.
DOI: 10.1109/CEC.2014.6900380.

The author's publication page distributes:
- the originally submitted L-SHADE 1.0.0 source;
- corrected L-SHADE 1.0.1 source;
- corrected SHADE 1.1.1 source.

The author notes that the submitted 1.0.0 source contained an archive-update bug.
This implementation follows corrected owned-archive semantics.

## What L-SHADE changes

L-SHADE is SHADE 1.1 plus Linear Population Size Reduction (LPSR).

Its population begins large for exploration and decreases continuously toward the
smallest size compatible with current-to-pbest mutation.

## Tuned CEC2014 settings

The paper reports the following ParamILS-tuned settings:

```text
N_init = round(18 * D)
archive ratio = 2.6
p = 0.11
H = 6
N_min = 4
```

The CEC2014 evaluation budget was:

```text
MAX_NFE = 10,000 * D
```

These are the defaults of `LShadeParameters`.

## Linear population-size reduction

After a generation:

```text
N_next =
    round(
        ((N_min - N_init) / MAX_NFE)
        * NFE
        + N_init)
```

If:

```text
N_next < N_current
```

the worst:

```text
N_current - N_next
```

individuals are removed.

The implementation uses the existing generic
`LinearDePopulationSizeReductionPolicy` through `LShadePopulationSchedule`.

## Flat-buffer implementation

The physical population allocation remains fixed at `N_init`.

Only the prefix:

```text
[0, ActivePopulationSize)
```

is active.

When LPSR removes members:
1. active indices are ranked by current fitness;
2. the best `N_next` are marked;
3. survivors are compacted toward the front in original slot order;
4. fitness and DE parameter slots are compacted with them;
5. no new population vector array is allocated.

This preserves flat contiguous memory and avoids repeated large-array allocation.

## Dynamic archive limit

The archive's maximum physical allocation is:

```text
round(ArchiveSizeRatio * N_init)
```

Its logical limit follows the current population:

```text
round(ArchiveSizeRatio * N_current)
```

After successful parents are inserted, the archive is randomly trimmed to the current
limit.

After LPSR reduces the population, the archive limit is recomputed and trimmed again.

Archive random deletion uses a dedicated deterministic random stream so archive
maintenance does not perturb target-owned variation streams.

## SHADE 1.1 memory used by L-SHADE

The 2014 paper describes SHADE 1.1, which differs in important details from the
earlier SHADE formulation.

Historical memories start at:

```text
M_F[h] = 0.5
M_CR[h] = 0.5
```

For target i, a random memory slot `r_i` is selected.

```text
if M_CR[r_i] is terminal:
    CR_i = 0
else:
    CR_i ~ Normal(M_CR[r_i], 0.1)
    CR_i <- clip(CR_i, 0, 1)

F_i ~ Cauchy(M_F[r_i], 0.1)
resample while F_i <= 0
F_i <- min(F_i, 1)
```

Strict successful trials are weighted by absolute fitness improvement.

SHADE 1.1 uses the weighted Lehmer mean for both successful CR and F.

If the current CR memory slot is already terminal, or all successful CR values are
zero, that CR slot receives the terminal value.

A terminal CR slot remains terminal thereafter when that circular slot returns.

## Selection and archive evidence

Non-worsening selection is:

```text
trial fitness <= parent fitness
```

but adaptation and archive insertion require strict improvement:

```text
trial fitness < parent fitness
```

Thus equal-fitness replacement does not provide success-history evidence.

## p-best selection

L-SHADE uses current-to-pbest/1/bin.

The implementation selects from at least two best active population members when
forming the p-best set, while respecting the tuned `p = 0.11`.

## Stopping versus LPSR budget

`MaximumFunctionEvaluations` defines the LPSR schedule.

The platform retains its generic stopping-criterion architecture rather than embedding
an algorithm-specific termination mechanism.

For a canonical fixed-budget experiment, configure the generic stopping criterion with
the same evaluation budget.

## Parallel execution

Variation and evaluation remain independent.

Every parallel call receives the current active population size, so Auto parallelism
naturally turns off as LPSR shrinks the population below the calibrated DE crossover.

Sequential and parallel paths use the same target-owned random streams and deterministic
archive/compaction ordering.