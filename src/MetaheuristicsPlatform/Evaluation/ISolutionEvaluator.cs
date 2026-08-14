namespace MetaheuristicsPlatform.Evaluation;

/// <summary>Computes the objective/fitness of a decoded solution.</summary>
public interface ISolutionEvaluator<in TSolution>
{
    double Evaluate(
        TSolution solution,
        CancellationToken cancellationToken = default);
}