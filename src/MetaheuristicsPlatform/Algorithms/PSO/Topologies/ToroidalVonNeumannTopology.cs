using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Toroidal two-dimensional Von Neumann neighborhood:
/// north, south, east and west.
/// </summary>
public sealed class ToroidalVonNeumannTopology : IPsoTopology
{
    public ToroidalVonNeumannTopology(
        int? rows = null,
        int? columns = null,
        bool includeSelf = true)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows));
        }

        if (columns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns));
        }

        Rows = rows;
        Columns = columns;
        IncludeSelf = includeSelf;
    }

    public int? Rows { get; }
    public int? Columns { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "toroidal-von-neumann",
        Name = "Toroidal Von Neumann",
        Aliases = new[] { "von Neumann", "square (Mendes et al. 2004)" },
        Dynamics = PsoTopologyDynamics.Static,
        IsPublishedExactVariant = true,
        References = new[]
        {
            PsoTopologyReferences.KennedyMendes2002,
            PsoTopologyReferences.MendesKennedyNeves2004
        }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        (int rows, int columns) = ResolveDimensions(context.SwarmSize);

        var builder = new UndirectedGraphBuilder(context.SwarmSize);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int node = Index(row, column, columns);

                builder.AddEdge(
                    node,
                    Index(Mod(row - 1, rows), column, columns));

                builder.AddEdge(
                    node,
                    Index(Mod(row + 1, rows), column, columns));

                builder.AddEdge(
                    node,
                    Index(row, Mod(column - 1, columns), columns));

                builder.AddEdge(
                    node,
                    Index(row, Mod(column + 1, columns), columns));
            }
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }

    private (int Rows, int Columns) ResolveDimensions(int swarmSize)
    {
        if (Rows.HasValue && Columns.HasValue)
        {
            if (Rows.Value * Columns.Value != swarmSize)
            {
                throw new ArgumentException(
                    "rows * columns must equal swarm size.");
            }

            return (Rows.Value, Columns.Value);
        }

        if (Rows.HasValue)
        {
            if (swarmSize % Rows.Value != 0)
            {
                throw new ArgumentException(
                    "Swarm size must be divisible by rows.");
            }

            return (Rows.Value, swarmSize / Rows.Value);
        }

        if (Columns.HasValue)
        {
            if (swarmSize % Columns.Value != 0)
            {
                throw new ArgumentException(
                    "Swarm size must be divisible by columns.");
            }

            return (swarmSize / Columns.Value, Columns.Value);
        }

        int rows = (int)Math.Floor(Math.Sqrt(swarmSize));

        while (rows > 1 && swarmSize % rows != 0)
        {
            rows--;
        }

        return (rows, swarmSize / rows);
    }

    private static int Index(int row, int column, int columns) =>
        (row * columns) + column;

    private static int Mod(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}