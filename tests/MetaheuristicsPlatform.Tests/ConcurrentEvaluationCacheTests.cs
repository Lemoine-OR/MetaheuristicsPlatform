using MetaheuristicsPlatform.Evaluation.Caching;

namespace MetaheuristicsPlatform.Tests;

public sealed class ConcurrentEvaluationCacheTests
{
    [Fact]
    public async Task ConcurrentDuplicateKeys_EvaluateFactoryOnce()
    {
        var cache =
            new ConcurrentEvaluationCache<int, int>();

        int factoryCalls = 0;

        Task<int>[] tasks =
            Enumerable.Range(0, 32)
                .Select(
                    _ =>
                        Task.Run(
                            () =>
                                cache.GetOrAdd(
                                    7,
                                    _ =>
                                    {
                                        Interlocked.Increment(
                                            ref factoryCalls);

                                        Thread.Sleep(10);

                                        return 49;
                                    }).Value))
                .ToArray();

        int[] values =
            await Task.WhenAll(tasks);

        Assert.All(
            values,
            static value =>
                Assert.Equal(49, value));

        Assert.Equal(
            1,
            factoryCalls);

        Assert.Equal(
            1,
            cache.Count);
    }

    [Fact]
    public void FaultedFactory_DoesNotPoisonCache()
    {
        var cache =
            new ConcurrentEvaluationCache<int, int>();

        Assert.Throws<InvalidOperationException>(
            () =>
                cache.GetOrAdd(
                    1,
                    static _ =>
                        throw new InvalidOperationException(
                            "Transient failure")));

        Assert.Equal(
            0,
            cache.Count);

        EvaluationCacheLookup<int> retry =
            cache.GetOrAdd(
                1,
                static _ => 10);

        Assert.False(
            retry.IsHit);

        Assert.Equal(
            10,
            retry.Value);
    }
}