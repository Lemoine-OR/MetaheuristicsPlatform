using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public sealed class AdaptiveLargeNeighborhoodDestroyOperator<TSolution,TRemoved>
{
    public AdaptiveLargeNeighborhoodDestroyOperator(
        string id,
        ILargeNeighborhoodDestroyOperator<TSolution,TRemoved> @operator)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Adaptive LNS destroy-operator ID must be non-empty.", nameof(id));

        Id = id;
        Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
    }

    public string Id { get; }
    public ILargeNeighborhoodDestroyOperator<TSolution,TRemoved> Operator { get; }
}

public sealed class AdaptiveLargeNeighborhoodRepairOperator<TSolution,TRemoved>
{
    public AdaptiveLargeNeighborhoodRepairOperator(
        string id,
        ILargeNeighborhoodRepairOperator<TSolution,TRemoved> @operator)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Adaptive LNS repair-operator ID must be non-empty.", nameof(id));

        Id = id;
        Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
    }

    public string Id { get; }
    public ILargeNeighborhoodRepairOperator<TSolution,TRemoved> Operator { get; }
}
