using MetaheuristicsPlatform.Algorithms.DE.Adaptive;

namespace MetaheuristicsPlatform.Tests;

public sealed class LShadeParameterAndScheduleTests
{
    [Fact]
    public void TunedPaperDefaultsResolveForTenDimensions()
    {
        var parameters =
            new LShadeParameters();

        Assert.Equal(
            180,
            parameters.ResolveInitialPopulationSize(
                dimension: 10));

        Assert.Equal(
            468,
            parameters.ResolveArchiveCapacity(
                populationSize: 180));

        Assert.Equal(
            100_000,
            parameters.ResolveMaximumFunctionEvaluations(
                dimension: 10));

        Assert.Equal(
            4,
            parameters.MinimumPopulationSize);

        Assert.Equal(
            0.11,
            parameters.PBestFraction);

        Assert.Equal(
            6,
            parameters.MemorySize);
    }

    [Fact]
    public void LpsrHitsInitialMiddleAndMinimumSizes()
    {
        var schedule =
            new LShadePopulationSchedule();

        Assert.Equal(
            180,
            schedule.GetTargetPopulationSize(
                initialPopulationSize: 180,
                currentPopulationSize: 180,
                minimumPopulationSize: 4,
                functionEvaluations: 0,
                maximumFunctionEvaluations: 100_000));

        Assert.Equal(
            92,
            schedule.GetTargetPopulationSize(
                initialPopulationSize: 180,
                currentPopulationSize: 180,
                minimumPopulationSize: 4,
                functionEvaluations: 50_000,
                maximumFunctionEvaluations: 100_000));

        Assert.Equal(
            4,
            schedule.GetTargetPopulationSize(
                initialPopulationSize: 180,
                currentPopulationSize: 180,
                minimumPopulationSize: 4,
                functionEvaluations: 100_000,
                maximumFunctionEvaluations: 100_000));
    }

    [Fact]
    public void ScheduleNeverIncreasesCurrentPopulation()
    {
        var schedule =
            new LShadePopulationSchedule();

        Assert.Equal(
            20,
            schedule.GetTargetPopulationSize(
                initialPopulationSize: 180,
                currentPopulationSize: 20,
                minimumPopulationSize: 4,
                functionEvaluations: 20_000,
                maximumFunctionEvaluations: 100_000));
    }
}