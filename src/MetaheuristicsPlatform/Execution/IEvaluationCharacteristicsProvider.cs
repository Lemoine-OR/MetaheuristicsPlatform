namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Implemented by optimization problems that can describe evaluation scheduling characteristics.
/// </summary>
public interface IEvaluationCharacteristicsProvider
{
    EvaluationCharacteristics EvaluationCharacteristics { get; }
}