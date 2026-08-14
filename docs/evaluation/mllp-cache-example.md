# MLLP cache and instrumentation sketch

A future MLLP pipeline may combine:

```text
particle
 -> decode
 -> repair
 -> local search
 -> ULS subproblem solver
 -> objective
```

Instrumentation can expose the cost of each stage.

If candidate encodings repeat, a safe cache can store the complete MLLP outcome.

For Lamarckian use, the cached outcome must include the improved candidate encoding,
not only its objective value.

A structural MLLP key should include every encoding component that influences:
- decoded production decisions;
- repair;
- local-search starting point;
- subproblem generation;
- final objective.