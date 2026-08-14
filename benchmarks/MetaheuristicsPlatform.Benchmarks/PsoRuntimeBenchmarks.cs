using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.State;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class PsoRuntimeBenchmarks
{
    private PsoSwarmBuffers _buffers = null!;
    private double[] _result = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [Params(32, 256, 1024)]
    public int SwarmSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _buffers =
            new PsoSwarmBuffers(
                SwarmSize,
                Dimension);

        _result =
            new double[SwarmSize];

        for (int particle = 0;
             particle < SwarmSize;
             particle++)
        {
            Span<double> position =
                _buffers.GetPosition(particle);

            for (int d = 0;
                 d < Dimension;
                 d++)
            {
                position[d] =
                    particle + d * 0.001;
            }
        }
    }

    [Benchmark(Baseline = true)]
    public void SequentialFlatTraversal()
    {
        for (int particle = 0;
             particle < SwarmSize;
             particle++)
        {
            ReadOnlySpan<double> position =
                _buffers.GetPositionReadOnly(
                    particle);

            double sum = 0.0;

            for (int d = 0;
                 d < position.Length;
                 d++)
            {
                sum += position[d] *
                    position[d];
            }

            _result[particle] = sum;
        }
    }

    [Benchmark]
    public void ParallelRangeTraversal()
    {
        PsoRangeExecutor.ForParticles(
            SwarmSize,
            Dimension,
            new PsoExecutionOptions
            {
                Mode = PsoExecutionMode.Parallel
            },
            (start, end) =>
            {
                for (int particle = start;
                     particle < end;
                     particle++)
                {
                    ReadOnlySpan<double> position =
                        _buffers.GetPositionReadOnly(
                            particle);

                    double sum = 0.0;

                    for (int d = 0;
                         d < position.Length;
                         d++)
                    {
                        sum += position[d] *
                            position[d];
                    }

                    _result[particle] = sum;
                }
            });
    }
}