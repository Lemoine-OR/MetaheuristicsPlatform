namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public enum DeParameterAdaptationKind
{
    Fixed = 0,
    SelfAdaptive = 1,
    CurrentSuccessMean = 2,
    SuccessHistory = 3
}