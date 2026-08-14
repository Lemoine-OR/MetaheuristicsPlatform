using MetaheuristicsPlatform.Algorithms.DE.State;
using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Deterministic current-fitness index comparer for p-best ranking.
/// Ties are broken by population index.
/// </summary>
internal sealed class DeFitnessIndexComparer :
    IComparer<int>
{
    private readonly DePopulationBuffers _population;
    private readonly OptimizationSense _sense;

    internal DeFitnessIndexComparer(
        DePopulationBuffers population,
        OptimizationSense sense)
    {
        _population =
            population ??
            throw new ArgumentNullException(
                nameof(population));

        _sense = sense;
    }

    public int Compare(
        int left,
        int right)
    {
        if (left == right)
        {
            return 0;
        }

        double leftFitness =
            _population.GetFitness(left);

        double rightFitness =
            _population.GetFitness(right);

        if (_sense.IsBetter(
                leftFitness,
                rightFitness))
        {
            return -1;
        }

        if (_sense.IsBetter(
                rightFitness,
                leftFitness))
        {
            return 1;
        }

        return left.CompareTo(right);
    }
}