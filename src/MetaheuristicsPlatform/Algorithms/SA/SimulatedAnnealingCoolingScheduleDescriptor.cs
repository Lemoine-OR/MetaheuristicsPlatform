using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Discoverable scientific metadata for a built-in SA cooling law.
/// </summary>
public sealed class SimulatedAnnealingCoolingScheduleDescriptor
{
    public SimulatedAnnealingCoolingScheduleDescriptor(
        string id,
        string name,
        SimulatedAnnealingCoolingScheduleKind kind,
        Type implementationType,
        bool isAdaptive,
        bool requiresLevelObjectiveStatistics,
        bool isComponentOfBroaderAnnealingAlgorithm,
        ScientificReference? reference,
        string scientificScope)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "A stable cooling-schedule ID is required.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A cooling-schedule name is required.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(
            implementationType);

        if (string.IsNullOrWhiteSpace(
                scientificScope))
        {
            throw new ArgumentException(
                "Scientific scope is required.",
                nameof(scientificScope));
        }

        Id = id;
        Name = name;
        Kind = kind;
        ImplementationType = implementationType;
        IsAdaptive = isAdaptive;
        RequiresLevelObjectiveStatistics =
            requiresLevelObjectiveStatistics;
        IsComponentOfBroaderAnnealingAlgorithm =
            isComponentOfBroaderAnnealingAlgorithm;
        Reference = reference;
        ScientificScope = scientificScope;
    }

    public string Id { get; }

    public string Name { get; }

    public SimulatedAnnealingCoolingScheduleKind Kind { get; }

    public Type ImplementationType { get; }

    public bool IsAdaptive { get; }

    public bool RequiresLevelObjectiveStatistics { get; }

    public bool IsComponentOfBroaderAnnealingAlgorithm { get; }

    public ScientificReference? Reference { get; }

    public string ScientificScope { get; }
}
