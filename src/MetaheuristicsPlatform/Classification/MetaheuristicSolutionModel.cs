namespace MetaheuristicsPlatform.Classification;

/// <summary>
/// Describes how many candidate solutions are actively manipulated by a metaheuristic.
/// </summary>
public enum MetaheuristicSolutionModel
{
    SingleSolution = 0,
    Population = 1,
    VariablePopulation = 2,
    Hybrid = 3
}