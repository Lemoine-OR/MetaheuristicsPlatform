using MetaheuristicsPlatform.Algorithms.SA;
using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Tests;

public sealed class MetropolisAcceptancePolicyTests
{
    [Fact]
    public void AcceptanceProbabilityMatchesMetropolisEquation()
    {
        double probability =
            MetropolisAcceptancePolicy.AcceptanceProbability(
                degradation: 2.0,
                temperature: 2.0);

        Assert.Equal(
            Math.Exp(-1.0),
            probability,
            precision: 12);
    }

    [Theory]
    [InlineData(
        OptimizationSense.Minimize,
        10.0,
        13.0,
        3.0)]
    [InlineData(
        OptimizationSense.Maximize,
        10.0,
        7.0,
        3.0)]
    [InlineData(
        OptimizationSense.Minimize,
        10.0,
        7.0,
        0.0)]
    [InlineData(
        OptimizationSense.Maximize,
        10.0,
        13.0,
        0.0)]
    public void DegradationIsSenseAware(
        OptimizationSense sense,
        double current,
        double candidate,
        double expected)
    {
        Assert.Equal(
            expected,
            MetropolisAcceptancePolicy.ComputeDegradation(
                sense,
                current,
                candidate));
    }

    [Fact]
    public void TargetProbabilityTemperatureIsExactInverse()
    {
        double temperature =
            SimulatedAnnealingTemperature
                .FromWorseningAcceptanceProbability(
                    degradation: 5.0,
                    targetAcceptanceProbability: 0.8);

        double recovered =
            MetropolisAcceptancePolicy
                .AcceptanceProbability(
                    degradation: 5.0,
                    temperature);

        Assert.Equal(
            0.8,
            recovered,
            precision: 12);
    }
}