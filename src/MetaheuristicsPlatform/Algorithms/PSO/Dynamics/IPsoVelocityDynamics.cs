namespace MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

/// <summary>Defines iteration-dependent PSO velocity dynamics.</summary>
public interface IPsoVelocityDynamics
{
    string Id { get; }

    PsoVelocityCoefficients GetCoefficients(
        long iteration);
}