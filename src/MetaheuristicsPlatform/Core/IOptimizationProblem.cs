namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Defines the problem-facing contract shared by all metaheuristics.
/// </summary>
/// <typeparam name="TSolution">Solution representation evaluated by the problem.</typeparam>
public interface IOptimizationProblem<in TSolution>
{
    /// <summary>Gets the optimization sense.</summary>
    OptimizationSense Sense { get; }

    /// <summary>Evaluates one candidate solution.</summary>
    double Evaluate(TSolution solution);
}