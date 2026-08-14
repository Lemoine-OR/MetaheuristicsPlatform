namespace MetaheuristicsPlatform.Graphs;

/// <summary>Computes diagnostic structural metrics for undirected topology graphs.</summary>
public static class GraphMetricsCalculator
{
    /// <summary>Computes graph metrics.</summary>
    public static GraphMetrics Compute(NeighborhoodGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        int n = graph.NodeCount;
        int[] degrees = new int[n];
        int minDegree = int.MaxValue;
        int maxDegree = int.MinValue;
        long degreeSum = 0;

        for (int i = 0; i < n; i++)
        {
            int degree = StructuralDegree(graph, i);
            degrees[i] = degree;
            minDegree = Math.Min(minDegree, degree);
            maxDegree = Math.Max(maxDegree, degree);
            degreeSum += degree;
        }

        double averageDegree = degreeSum / (double)n;

        double degreeVariance = 0.0;
        for (int i = 0; i < n; i++)
        {
            double delta = degrees[i] - averageDegree;
            degreeVariance += delta * delta;
        }
        degreeVariance /= n;

        int structuralEdges = graph.EdgeCount - graph.SelfLoopCount;
        double maximumEdges = n <= 1 ? 0.0 : n * (n - 1) / 2.0;
        double density = maximumEdges > 0.0 ? structuralEdges / maximumEdges : 0.0;

        int connectedComponents = CountConnectedComponents(graph);

        (int diameter, double averagePathLength) = ComputePathMetrics(graph);

        double clustering = ComputeAverageClustering(graph);

        return new GraphMetrics(
            n,
            graph.EdgeCount,
            graph.SelfLoopCount,
            connectedComponents,
            minDegree,
            maxDegree,
            averageDegree,
            degreeVariance,
            density,
            diameter,
            averagePathLength,
            clustering);
    }

    private static int StructuralDegree(NeighborhoodGraph graph, int node)
    {
        int degree = 0;
        foreach (int neighbor in graph.GetNeighbors(node))
        {
            if (neighbor != node)
            {
                degree++;
            }
        }

        return degree;
    }

    private static int CountConnectedComponents(NeighborhoodGraph graph)
    {
        int n = graph.NodeCount;
        bool[] visited = new bool[n];
        int components = 0;
        Queue<int> queue = new();

        for (int start = 0; start < n; start++)
        {
            if (visited[start])
            {
                continue;
            }

            components++;
            visited[start] = true;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                foreach (int neighbor in graph.GetNeighbors(current))
                {
                    if (neighbor == current || visited[neighbor])
                    {
                        continue;
                    }

                    visited[neighbor] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return components;
    }

    private static (int Diameter, double AveragePathLength) ComputePathMetrics(
        NeighborhoodGraph graph)
    {
        int n = graph.NodeCount;
        long reachablePairCount = 0;
        long distanceSum = 0;
        int diameter = 0;

        int[] distances = new int[n];
        Queue<int> queue = new();

        for (int source = 0; source < n; source++)
        {
            Array.Fill(distances, -1);
            distances[source] = 0;
            queue.Clear();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                int nextDistance = distances[current] + 1;

                foreach (int neighbor in graph.GetNeighbors(current))
                {
                    if (neighbor == current || distances[neighbor] >= 0)
                    {
                        continue;
                    }

                    distances[neighbor] = nextDistance;
                    queue.Enqueue(neighbor);
                }
            }

            for (int target = source + 1; target < n; target++)
            {
                int distance = distances[target];
                if (distance < 0)
                {
                    continue;
                }

                reachablePairCount++;
                distanceSum += distance;
                diameter = Math.Max(diameter, distance);
            }
        }

        double average = reachablePairCount > 0
            ? distanceSum / (double)reachablePairCount
            : 0.0;

        return (diameter, average);
    }

    private static double ComputeAverageClustering(NeighborhoodGraph graph)
    {
        int n = graph.NodeCount;
        double sum = 0.0;

        for (int node = 0; node < n; node++)
        {
            List<int> structuralNeighbors = [];
            foreach (int neighbor in graph.GetNeighbors(node))
            {
                if (neighbor != node)
                {
                    structuralNeighbors.Add(neighbor);
                }
            }

            int degree = structuralNeighbors.Count;
            if (degree < 2)
            {
                continue;
            }

            int connectedPairs = 0;
            for (int i = 0; i < degree; i++)
            {
                for (int j = i + 1; j < degree; j++)
                {
                    if (graph.ContainsEdge(
                        structuralNeighbors[i],
                        structuralNeighbors[j]))
                    {
                        connectedPairs++;
                    }
                }
            }

            double possiblePairs = degree * (degree - 1) / 2.0;
            sum += connectedPairs / possiblePairs;
        }

        return sum / n;
    }
}