namespace MetaheuristicsPlatform.Algorithms.TS;

public delegate ulong TabuSearchSolutionSignatureFactory<TSolution>(
    in TSolution solution);

/// <summary>
/// Delegate adapter for reactive-search configuration signatures.
/// </summary>
public sealed class DelegateTabuSearchSolutionSignatureProvider<TSolution> :
    ITabuSearchSolutionSignatureProvider<TSolution>
{
    private readonly TabuSearchSolutionSignatureFactory<TSolution> _factory;

    public DelegateTabuSearchSolutionSignatureProvider(
        TabuSearchSolutionSignatureFactory<TSolution> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public ulong GetSignature(in TSolution solution) =>
        _factory(in solution);
}
