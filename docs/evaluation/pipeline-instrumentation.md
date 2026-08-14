# Evaluation pipeline instrumentation

Instrumentation is opt-in.

Without a metrics sink:

```text
EvaluationPipeline.Evaluate
 -> uninstrumented direct path
```

No Stopwatch measurements or metrics updates are performed.

With an `IEvaluationPipelineMetricsSink`:

```text
decode ticks
repair ticks
improve/local-search ticks
evaluate ticks
feedback ticks
total ticks
```

are recorded.

`EvaluationPipelineMetrics` uses interlocked counters and a fixed-size power-of-two
histogram. It does not allocate per evaluation.

The histogram snapshot is allocated only when explicitly requested.

## Cache metrics

The same sink contract also supports:
- cache hits;
- cache misses;
- hit ratio.

A `CachedEvaluationPipeline` may receive the same metrics instance as the inner
instrumented pipeline so one object exposes both stage timing and cache efficiency.

## Research use

These measurements are intended to answer questions such as:
- Is local search dominating objective evaluation?
- Does repair activate frequently?
- Is the evaluation workload heterogeneous?
- Is the cache useful or just consuming memory?
- Should evaluation parallelism use fine-grained scheduling?