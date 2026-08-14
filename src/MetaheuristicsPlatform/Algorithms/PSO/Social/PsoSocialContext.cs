using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Graphs;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Borrowed read-only PSO state required by guide selection and influence policies.
/// Production execution uses flat particle-major buffers.
/// </summary>
public sealed class PsoSocialContext
{
    private readonly double[] _positions;
    private readonly double[] _personalBestPositions;
    private readonly double[] _personalBestFitness;

    /// <summary>
    /// Initializes a no-copy social view over flat particle-major buffers.
    /// </summary>
    public PsoSocialContext(
        double[] positions,
        double[] personalBestPositions,
        double[] personalBestFitness,
        int swarmSize,
        int dimension,
        NeighborhoodGraph graph,
        OptimizationSense sense)
    {
        _positions = positions ??
            throw new ArgumentNullException(nameof(positions));

        _personalBestPositions =
            personalBestPositions ??
            throw new ArgumentNullException(
                nameof(personalBestPositions));

        _personalBestFitness =
            personalBestFitness ??
            throw new ArgumentNullException(
                nameof(personalBestFitness));

        _graph = graph ??
            throw new ArgumentNullException(nameof(graph));

        if (swarmSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(swarmSize));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        int expectedVectorLength =
            checked(swarmSize * dimension);

        if (_positions.Length != expectedVectorLength ||
            _personalBestPositions.Length !=
                expectedVectorLength ||
            _personalBestFitness.Length != swarmSize ||
            graph.NodeCount != swarmSize)
        {
            throw new ArgumentException(
                "Flat particle buffers, fitness buffer and graph dimensions must agree.");
        }

        SwarmSize = swarmSize;
        Dimension = dimension;
        Sense = sense;
    }

    /// <summary>
    /// Initializes a no-copy social view whose informer structure is implicit in
    /// a specialized fast path and therefore does not require a materialized graph.
    /// </summary>
    public PsoSocialContext(
        double[] positions,
        double[] personalBestPositions,
        double[] personalBestFitness,
        int swarmSize,
        int dimension,
        OptimizationSense sense)
    {
        _positions = positions ??
            throw new ArgumentNullException(nameof(positions));

        _personalBestPositions =
            personalBestPositions ??
            throw new ArgumentNullException(
                nameof(personalBestPositions));

        _personalBestFitness =
            personalBestFitness ??
            throw new ArgumentNullException(
                nameof(personalBestFitness));

        if (swarmSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(swarmSize));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        int expectedVectorLength =
            checked(swarmSize * dimension);

        if (_positions.Length != expectedVectorLength ||
            _personalBestPositions.Length != expectedVectorLength ||
            _personalBestFitness.Length != swarmSize)
        {
            throw new ArgumentException(
                "Flat particle buffers and fitness buffer dimensions must agree.");
        }

        SwarmSize = swarmSize;
        Dimension = dimension;
        Sense = sense;
        _graph = null;
    }
    /// <summary>
    /// Compatibility constructor for tests and callers using jagged arrays.
    /// Arrays are flattened once. Production PSO execution should use the flat constructor.
    /// </summary>
    public PsoSocialContext(
        IReadOnlyList<double[]> positions,
        IReadOnlyList<double[]> personalBestPositions,
        double[] personalBestFitness,
        NeighborhoodGraph graph,
        OptimizationSense sense)
    {
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(
            personalBestPositions);
        ArgumentNullException.ThrowIfNull(
            personalBestFitness);
        ArgumentNullException.ThrowIfNull(graph);

        if (positions.Count == 0)
        {
            throw new ArgumentException(
                "At least one particle is required.",
                nameof(positions));
        }

        if (positions.Count !=
                personalBestPositions.Count ||
            positions.Count !=
                personalBestFitness.Length ||
            positions.Count != graph.NodeCount)
        {
            throw new ArgumentException(
                "Particle state lengths and graph node count must agree.");
        }

        double[] first =
            positions[0] ??
            throw new ArgumentException(
                "Position arrays cannot contain null.",
                nameof(positions));

        int dimension = first.Length;

        if (dimension <= 0)
        {
            throw new ArgumentException(
                "Particle dimension must be strictly positive.",
                nameof(positions));
        }

        SwarmSize = positions.Count;
        Dimension = dimension;
        Sense = sense;
        _graph = graph;

        int vectorLength =
            checked(SwarmSize * Dimension);

        _positions =
            GC.AllocateUninitializedArray<double>(
                vectorLength);

        _personalBestPositions =
            GC.AllocateUninitializedArray<double>(
                vectorLength);

        _personalBestFitness =
            (double[])personalBestFitness.Clone();

        for (int particle = 0;
             particle < SwarmSize;
             particle++)
        {
            double[] position =
                positions[particle] ??
                throw new ArgumentException(
                    "Position arrays cannot contain null.",
                    nameof(positions));

            double[] personalBest =
                personalBestPositions[particle] ??
                throw new ArgumentException(
                    "Personal-best arrays cannot contain null.",
                    nameof(personalBestPositions));

            if (position.Length != Dimension ||
                personalBest.Length != Dimension)
            {
                throw new ArgumentException(
                    "All current and personal-best positions must share one dimension.");
            }

            position.CopyTo(
                _positions,
                particle * Dimension);

            personalBest.CopyTo(
                _personalBestPositions,
                particle * Dimension);
        }
    }

    public int SwarmSize { get; }
    public int Dimension { get; }
    private NeighborhoodGraph? _graph;

    /// <summary>
    /// Gets whether an explicit materialized neighborhood graph is available.
    /// </summary>
    public bool HasGraph => _graph is not null;

    /// <summary>
    /// Gets the explicit neighborhood graph.
    /// Fast paths with an implicit informer structure may intentionally omit it.
    /// </summary>
    public NeighborhoodGraph Graph =>
        _graph ??
        throw new InvalidOperationException(
            "This PSO social context uses an implicit informer structure and has no materialized graph.");

    public OptimizationSense Sense { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double> GetPosition(
        int particle) =>
        _positions.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<double>
        GetPersonalBestPosition(
            int particle) =>
        _personalBestPositions.AsSpan(
            Offset(particle),
            Dimension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetPersonalBestFitness(
        int particle)
    {
        ValidateParticle(particle);
        return _personalBestFitness[particle];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Offset(int particle)
    {
        ValidateParticle(particle);
        return particle * Dimension;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateParticle(int particle)
    {
        if ((uint)particle >= (uint)SwarmSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(particle));
        }
    }
}