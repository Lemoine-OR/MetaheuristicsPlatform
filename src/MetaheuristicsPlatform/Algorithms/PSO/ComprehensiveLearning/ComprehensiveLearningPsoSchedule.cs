namespace MetaheuristicsPlatform.Algorithms.PSO.ComprehensiveLearning;

public static class ComprehensiveLearningPsoSchedule
{
    public static double LearningProbability(int particleIndex, int swarmSize)
    {
        if (swarmSize <= 1) throw new ArgumentOutOfRangeException(nameof(swarmSize));
        if ((uint)particleIndex >= (uint)swarmSize) throw new ArgumentOutOfRangeException(nameof(particleIndex));

        double exponent =
            10.0 * particleIndex / (swarmSize - 1.0);

        return
            0.05 +
            0.45 *
            (Math.Exp(exponent) - 1.0) /
            (Math.Exp(10.0) - 1.0);
    }

    public static double InertiaWeight(
        int completedIterations,
        int maximumIterations,
        double initialWeight,
        double finalWeight)
    {
        if (maximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(maximumIterations));
        double progress = Math.Clamp((double)completedIterations / maximumIterations, 0.0, 1.0);
        return initialWeight + (finalWeight - initialWeight) * progress;
    }
}
