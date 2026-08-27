namespace MetaheuristicsPlatform.Algorithms.PSO.Scientific;

public static class FullyInformedPsoKernel
{
    public static double CoefficientPerInformer(
        double totalAccelerationCoefficient,
        int informerCount)
    {
        if (!double.IsFinite(totalAccelerationCoefficient) || totalAccelerationCoefficient < 0.0)
            throw new ArgumentOutOfRangeException(nameof(totalAccelerationCoefficient));
        if (informerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(informerCount));

        return totalAccelerationCoefficient / informerCount;
    }
}
