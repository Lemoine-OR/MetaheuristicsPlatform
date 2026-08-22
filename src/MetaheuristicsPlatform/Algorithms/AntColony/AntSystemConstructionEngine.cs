using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Shared constructive transition engine for Ant System descendants.
/// Canonical Ant System uses q0=0 and therefore keeps pure proportional
/// selection. ACS uses the pseudo-random proportional rule with q0>0.
/// </summary>
internal sealed class AntSystemConstructionEngine<
    TSolution,
    TComponent,
    TPheromoneKey,
    TEnumerator>
    where TPheromoneKey : notnull
    where TEnumerator : struct, IAntColonyCandidateEnumerator<TComponent>
{
    private readonly IAntColonyConstructionModel<
        TSolution,
        TComponent,
        TPheromoneKey,
        TEnumerator> _model;

    public AntSystemConstructionEngine(
        IAntColonyConstructionModel<
            TSolution,
            TComponent,
            TPheromoneKey,
            TEnumerator> model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public AntColonyConstructionResult<TSolution, TPheromoneKey> Construct(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        AntSystemPheromoneMemory<TPheromoneKey> pheromones,
        double alpha,
        double beta,
        int maximumConstructionSteps,
        CancellationToken cancellationToken,
        double exploitationProbability = 0.0,
        Action<TPheromoneKey>? selectedKeyUpdate = null)
    {
        if (!double.IsFinite(exploitationProbability) ||
            exploitationProbability < 0.0 ||
            exploitationProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(exploitationProbability));
        }

        TSolution solution =
            _model.CreateInitialSolution(problem, random);

        var path =
            new List<TPheromoneKey>();

        int steps = 0;
        long transitionEvaluations = 0;

        while (!_model.IsComplete(in solution, problem))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (steps >= maximumConstructionSteps)
            {
                throw new InvalidOperationException(
                    "Ant construction exceeded MaximumConstructionSteps before producing a complete solution.");
            }

            TEnumerator enumerator =
                _model.GetCandidateEnumerator(in solution, problem);

            var components = new List<TComponent>();
            var keys = new List<TPheromoneKey>();
            var logWeights = new List<double>();

            while (enumerator.MoveNext(out TComponent component))
            {
                cancellationToken.ThrowIfCancellationRequested();

                TPheromoneKey key =
                    _model.GetPheromoneKey(
                        in solution,
                        in component,
                        problem);

                double tau =
                    pheromones.Get(key);

                if (!double.IsFinite(tau) || tau <= 0.0)
                {
                    throw new InvalidOperationException(
                        "Ant Colony Optimization requires finite strictly-positive pheromone values.");
                }

                double logWeight =
                    alpha == 0.0
                        ? 0.0
                        : alpha * Math.Log(tau);

                if (beta > 0.0)
                {
                    double eta =
                        _model.EvaluateHeuristic(
                            in solution,
                            in component,
                            problem);

                    if (!double.IsFinite(eta) || eta <= 0.0)
                    {
                        throw new InvalidOperationException(
                            "Ant Colony Optimization requires finite strictly-positive heuristic information when Beta is positive.");
                    }

                    logWeight +=
                        beta * Math.Log(eta);
                }

                components.Add(component);
                keys.Add(key);
                logWeights.Add(logWeight);
                transitionEvaluations++;
            }

            if (components.Count == 0)
            {
                throw new InvalidOperationException(
                    "Ant construction reached an incomplete solution with no feasible candidate.");
            }

            int selectedIndex;

            if (exploitationProbability > 0.0 &&
                random.NextDouble() < exploitationProbability)
            {
                selectedIndex = 0;

                for (int i = 1; i < logWeights.Count; i++)
                {
                    if (logWeights[i] > logWeights[selectedIndex])
                    {
                        selectedIndex = i;
                    }
                }
            }
            else
            {
                // Gumbel-max is exactly equivalent to proportional categorical
                // sampling while remaining numerically stable in log space.
                selectedIndex = 0;
                double selectedScore = double.NegativeInfinity;

                for (int i = 0; i < logWeights.Count; i++)
                {
                    double u = random.NextDouble();

                    if (!double.IsFinite(u) || u < 0.0 || u >= 1.0)
                    {
                        throw new InvalidOperationException(
                            "The random source returned a value outside [0,1).");
                    }

                    u = Math.Max(double.Epsilon, u);

                    double gumbel =
                        -Math.Log(-Math.Log(u));

                    double score =
                        logWeights[i] + gumbel;

                    if (score > selectedScore)
                    {
                        selectedScore = score;
                        selectedIndex = i;
                    }
                }
            }

            TComponent selectedComponent =
                components[selectedIndex];

            TPheromoneKey selectedKey =
                keys[selectedIndex];

            _model.ApplyComponent(
                ref solution,
                in selectedComponent,
                problem);

            path.Add(selectedKey);
            selectedKeyUpdate?.Invoke(selectedKey);
            steps++;
        }

        return new AntColonyConstructionResult<TSolution, TPheromoneKey>(
            solution,
            path,
            steps,
            transitionEvaluations);
    }
}
