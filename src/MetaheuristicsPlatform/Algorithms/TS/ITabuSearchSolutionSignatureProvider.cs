namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Computes a stable 64-bit signature for a visited solution.
/// </summary>
/// <remarks>
/// Reactive Tabu Search uses the signature as the key of the configuration-repetition
/// memory. The provider is domain-owned: equal configurations must return the same signature,
/// and applications that require exact collision freedom should encode or verify that property
/// in the provider rather than assuming that an arbitrary hash is injective.
/// </remarks>
public interface ITabuSearchSolutionSignatureProvider<TSolution>
{
    ulong GetSignature(in TSolution solution);
}
