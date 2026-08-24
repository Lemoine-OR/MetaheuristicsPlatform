using System.Collections.Concurrent;

namespace MetaheuristicsPlatform.Catalog;

/// <summary>
/// Stable-ID factory with typed creation and explicit composition registration.
/// </summary>
/// <remarks>
/// Parameterless built-in algorithms are discovered inside the current assembly by
/// their stable public IDs. Algorithms requiring domain components, such as generic
/// Simulated Annealing neighborhoods/moves, are registered explicitly with the same ID.
/// This preserves strong typing instead of returning a weak common object API.
/// </remarks>
public static class MetaheuristicFactory
{
    private static readonly ConcurrentDictionary<string, Func<object>>
        Registrations =
        new(StringComparer.Ordinal);

    static MetaheuristicFactory()
    {
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ParticleSwarm,
            "MetaheuristicsPlatform.Algorithms.PSO.ParticleSwarmOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ArtificialBeeColony,
            "MetaheuristicsPlatform.Algorithms.ArtificialBeeColony.ArtificialBeeColonyOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.Firefly,
            "MetaheuristicsPlatform.Algorithms.Firefly.FireflyOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.HarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.HarmonySearchOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ImprovedHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.ImprovedHarmonySearchOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.GlobalBestHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.GlobalBestHarmonySearchOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.SelfAdaptiveGlobalBestHarmonySearchOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.NovelGlobalHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.NovelGlobalHarmonySearchOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ParameterSettingFreeHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.ParameterSettingFreeHarmonySearchOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.AdvancedParameterSettingFreeHarmonySearchIteration,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.AdvancedParameterSettingFreeHarmonySearchIterationOptimizer");
        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.AdvancedParameterSettingFreeHarmonySearchObject,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.AdvancedParameterSettingFreeHarmonySearchObjectOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.DifferentialHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.DifferentialHarmonySearchOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ExploratoryHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.ExploratoryHarmonySearchOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ImprovedHarmonySearchDifferentialMutation,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.ImprovedHarmonySearchDifferentialMutationOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.NovelSelfAdaptiveHarmonySearch,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.NovelSelfAdaptiveHarmonySearchOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.AdaptiveHarmonySearchDifferentialEvolution,
            "MetaheuristicsPlatform.Algorithms.HarmonySearch.AdaptiveHarmonySearchDifferentialEvolutionOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.CmaEs,
            "MetaheuristicsPlatform.Algorithms.CMAES.CmaEsOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ActiveCmaEs,
            "MetaheuristicsPlatform.Algorithms.CMAES.ActiveCmaEsOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.SeparableCmaEs,
            "MetaheuristicsPlatform.Algorithms.CMAES.SeparableCmaEsOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.IpopCmaEs,
            "MetaheuristicsPlatform.Algorithms.CMAES.IpopCmaEsOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.BipopCmaEs,
            "MetaheuristicsPlatform.Algorithms.CMAES.BipopCmaEsOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.ContinuousCrossEntropy,
            "MetaheuristicsPlatform.Algorithms.CrossEntropy.ContinuousCrossEntropyOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.DifferentialEvolution,
            "MetaheuristicsPlatform.Algorithms.DE.DifferentialEvolutionOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.Jde,
            "MetaheuristicsPlatform.Algorithms.DE.Adaptive.SelfAdaptiveDifferentialEvolutionOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.Jade,
            "MetaheuristicsPlatform.Algorithms.DE.Adaptive.JadeOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.Shade,
            "MetaheuristicsPlatform.Algorithms.DE.Adaptive.ShadeOptimizer");

        RegisterAssemblyType(
            MetaheuristicAlgorithmIds.LShade,
            "MetaheuristicsPlatform.Algorithms.DE.Adaptive.LShadeOptimizer");
    }

    public static IReadOnlyCollection<string> RegisteredIds =>
        Registrations.Keys
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

    public static void Register<TAlgorithm>(
        string id,
        Func<TAlgorithm> factory,
        bool replace = false)
        where TAlgorithm : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(factory);

        _ = MetaheuristicCatalog.GetRequired(id);

        Func<object> boxedFactory =
            () => factory();

        if (replace)
        {
            Registrations[id] =
                boxedFactory;

            return;
        }

        if (!Registrations.TryAdd(
                id,
                boxedFactory))
        {
            throw new InvalidOperationException(
                $"A factory is already registered for algorithm id '{id}'.");
        }
    }

    public static TAlgorithm Create<TAlgorithm>(
        string id)
        where TAlgorithm : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        MetaheuristicCatalogEntry metadata =
            MetaheuristicCatalog.GetRequired(id);

        if (!Registrations.TryGetValue(
                id,
                out Func<object>? factory))
        {
            string compositionHint =
                metadata.RequiresComposition
                    ? " Register a typed factory after supplying the required neighborhood/move/domain components."
                    : string.Empty;

            throw new InvalidOperationException(
                $"No runtime factory is registered for algorithm id '{id}'.{compositionHint}");
        }

        object instance =
            factory();

        if (instance is not TAlgorithm typed)
        {
            throw new InvalidOperationException(
                $"Algorithm id '{id}' is registered as '{instance.GetType().FullName}', " +
                $"which cannot be returned as '{typeof(TAlgorithm).FullName}'.");
        }

        return typed;
    }

    private static void RegisterAssemblyType(
        string id,
        string fullTypeName)
    {
        Type? type =
            typeof(MetaheuristicFactory)
                .Assembly
                .GetType(
                    fullTypeName,
                    throwOnError: false,
                    ignoreCase: false);

        if (type is null ||
            type.IsAbstract ||
            type.GetConstructor(Type.EmptyTypes) is null)
        {
            return;
        }

        Registrations.TryAdd(
            id,
            () =>
                Activator.CreateInstance(type) ??
                throw new InvalidOperationException(
                    $"Unable to instantiate '{fullTypeName}'."));
    }
}
