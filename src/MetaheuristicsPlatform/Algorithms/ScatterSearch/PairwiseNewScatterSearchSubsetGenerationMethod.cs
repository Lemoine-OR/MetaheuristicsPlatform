namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>
/// Canonical simple subset generation: all unordered pairs containing at least
/// one reference point that is new since the preceding subset-generation round.
/// </summary>
public sealed class PairwiseNewScatterSearchSubsetGenerationMethod<TSolution> :
    IScatterSearchSubsetGenerationMethod<TSolution>
{
    public IReadOnlyList<ScatterSearchSubset<TSolution>> Generate(
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> referenceSet)
    {
        ArgumentNullException.ThrowIfNull(referenceSet);

        var subsets =
            new List<ScatterSearchSubset<TSolution>>();

        for (int i = 0; i < referenceSet.Count; i++)
        {
            for (int j = i + 1; j < referenceSet.Count; j++)
            {
                if (!referenceSet[i].IsNew &&
                    !referenceSet[j].IsNew)
                {
                    continue;
                }

                subsets.Add(
                    new ScatterSearchSubset<TSolution>(
                        new[]
                        {
                            referenceSet[i],
                            referenceSet[j]
                        }));
            }
        }

        return subsets;
    }
}
