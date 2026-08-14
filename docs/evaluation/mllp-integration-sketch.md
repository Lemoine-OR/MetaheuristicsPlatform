# MLLP integration sketch

This document is architectural only; it does not implement MLLP.

Example composition:

```csharp
var pipeline =
    new EvaluationPipeline<PsoEncoding, MllpSolution>(
        decoder: mllpDecoder,
        evaluator: mllpEvaluator,
        evaluationCharacteristics:
            new EvaluationCharacteristics(
                SupportsParallelEvaluation: true,
                CostHint: EvaluationCostHint.Heavy,
                VariabilityHint: EvaluationVariabilityHint.High),
        repair: mllpRepair,
        improver: mllpLocalSearch,
        feedbackMode:
            ImprovementFeedbackMode.Baldwinian);
```

The local-search implementation may itself call:
- a problem-specific neighborhood;
- a fast dynamic program;
- a MILP solver;
- ULSAlgorithms;
- another metaheuristic.

This composition remains outside the PSO engine.