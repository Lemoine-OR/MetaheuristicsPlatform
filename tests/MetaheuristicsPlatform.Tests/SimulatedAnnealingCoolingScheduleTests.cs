using MetaheuristicsPlatform.Algorithms.SA;

namespace MetaheuristicsPlatform.Tests;

public sealed class SimulatedAnnealingCoolingScheduleTests
{
    [Fact]
    public void GeometricScheduleMultipliesTemperature()
    {
        var schedule =
            new GeometricCoolingSchedule(
                alpha: 0.9);

        var context =
            new SimulatedAnnealingCoolingContext(
                CompletedTemperatureLevels: 1,
                AttemptedTransitions: 100,
                AcceptedTransitions: 50,
                InitialTemperature: 10.0,
                CurrentTemperature: 10.0);

        Assert.Equal(
            9.0,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void LundyMeesUsesPublishedRecurrence()
    {
        var schedule =
            new LundyMeesCoolingSchedule(
                beta: 0.01);

        var context =
            new SimulatedAnnealingCoolingContext(
                CompletedTemperatureLevels: 1,
                AttemptedTransitions: 100,
                AcceptedTransitions: 50,
                InitialTemperature: 10.0,
                CurrentTemperature: 10.0);

        Assert.Equal(
            10.0 / 1.1,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }
}