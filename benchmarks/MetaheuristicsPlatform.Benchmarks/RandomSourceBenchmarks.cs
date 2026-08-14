using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class RandomSourceBenchmarks
{
    private Xoshiro256StarStarRandomSource _random = null!;

    [GlobalSetup]
    public void Setup()
    {
        _random = new Xoshiro256StarStarRandomSource(123456789UL);
    }

    [Benchmark]
    public ulong NextUInt64() => _random.NextUInt64();

    [Benchmark]
    public double NextDouble() => _random.NextDouble();

    [Benchmark]
    public int NextInt32() => _random.NextInt32(1_000_000);
}