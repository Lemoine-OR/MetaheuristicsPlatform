namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Stable component identifiers for the Ant Colony Optimization family.</summary>
public static class AntColonyComponentIds
{
    public const string AntSystemProportionalTransition =
        "aco.transition.ant-system-proportional";

    public const string AllAntsGlobalUpdate =
        "aco.update.all-ants";

    public const string ConstantDeposit =
        "aco.deposit.constant";

    public const string PositiveInverseObjectiveDeposit =
        "aco.deposit.inverse-positive-objective";

    public const string AntColonySystem =
        "aco.variant.ant-colony-system";

    public const string MaxMinAntSystem =
        "aco.variant.max-min-ant-system";
}
