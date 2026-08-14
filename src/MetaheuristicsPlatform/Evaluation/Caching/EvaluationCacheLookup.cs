namespace MetaheuristicsPlatform.Evaluation.Caching;

public readonly record struct EvaluationCacheLookup<TValue>(
    TValue Value,
    bool IsHit);