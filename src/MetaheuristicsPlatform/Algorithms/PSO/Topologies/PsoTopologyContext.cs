using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Read-only state exposed to topology construction and rebuild logic.
/// Arrays are borrowed for the duration of the topology operation and are not copied.
/// </summary>
public sealed class PsoTopologyContext
{
    private readonly double[] _currentFitness;
    private readonly double[] _personalBestFitness;
    private readonly IReadOnlyList<double[]>? _positions;

    /// <summary>Initializes a topology context.</summary>
    public PsoTopologyContext(
        int swarmSize,
        long iteration,
        OptimizationSense sense,
        double[]? currentFitness = null,
        double[]? personalBestFitness = null,
        IReadOnlyList<double[]>? positions = null)
    {
        if (swarmSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(swarmSize));
        }

        SwarmSize = swarmSize;
        Iteration = iteration;
        Sense = sense;
        _currentFitness = currentFitness ?? Array.Empty<double>();
        _personalBestFitness = personalBestFitness ?? Array.Empty<double>();
        _positions = positions;

        if (_currentFitness.Length != 0 &&
            _currentFitness.Length != swarmSize)
        {
            throw new ArgumentException(
                "Current-fitness length must equal swarm size.",
                nameof(currentFitness));
        }

        if (_personalBestFitness.Length != 0 &&
            _personalBestFitness.Length != swarmSize)
        {
            throw new ArgumentException(
                "Personal-best-fitness length must equal swarm size.",
                nameof(personalBestFitness));
        }

        if (_positions is not null && _positions.Count != swarmSize)
        {
            throw new ArgumentException(
                "Position count must equal swarm size.",
                nameof(positions));
        }
    }

    public int SwarmSize { get; }
    public long Iteration { get; }
    public OptimizationSense Sense { get; }

    public bool HasCurrentFitness => _currentFitness.Length == SwarmSize;
    public bool HasPersonalBestFitness => _personalBestFitness.Length == SwarmSize;
    public bool HasPositions => _positions is not null;

    public double GetCurrentFitness(int particle)
    {
        ValidateParticle(particle);
        if (!HasCurrentFitness)
        {
            throw new InvalidOperationException(
                "Current fitness is required but was not supplied.");
        }

        return _currentFitness[particle];
    }

    public double GetPersonalBestFitness(int particle)
    {
        ValidateParticle(particle);
        if (!HasPersonalBestFitness)
        {
            throw new InvalidOperationException(
                "Personal-best fitness is required but was not supplied.");
        }

        return _personalBestFitness[particle];
    }

    public ReadOnlySpan<double> GetPosition(int particle)
    {
        ValidateParticle(particle);
        if (_positions is null)
        {
            throw new InvalidOperationException(
                "Particle positions are required but were not supplied.");
        }

        return _positions[particle];
    }

    private void ValidateParticle(int particle)
    {
        if ((uint)particle >= (uint)SwarmSize)
        {
            throw new ArgumentOutOfRangeException(nameof(particle));
        }
    }
}