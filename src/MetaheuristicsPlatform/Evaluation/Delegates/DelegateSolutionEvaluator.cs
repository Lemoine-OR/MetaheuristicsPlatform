namespace MetaheuristicsPlatform.Evaluation.Delegates;

public sealed class DelegateSolutionEvaluator<TSolution> :
    ISolutionEvaluator<TSolution>
{
    private readonly SolutionEvaluatorDelegate<TSolution> _evaluate;

    public DelegateSolutionEvaluator(
        SolutionEvaluatorDelegate<TSolution> evaluate)
    {
        _evaluate =
            evaluate ??
            throw new ArgumentNullException(
                nameof(evaluate));
    }

    public double Evaluate(
        TSolution solution,
        CancellationToken cancellationToken = default) =>
        _evaluate(
            solution,
            cancellationToken);
}