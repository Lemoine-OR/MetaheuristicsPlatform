using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Canonical Ant System constructive transition engine.
/// Categorical sampling is performed with the Gumbel-max identity in log space,
/// which is exactly equivalent to pheromone/heuristic proportional sampling while
/// avoiding overflow in tau^alpha eta^beta.
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
        CancellationToken cancellationToken)
    {
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

            bool found = false;
            TComponent selectedComponent = default!;
            TPheromoneKey selectedKey = default!;
            double selectedScore = double.NegativeInfinity;

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
                        "Ant System requires finite strictly-positive pheromone values.");
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
                            "Ant System requires finite strictly-positive heuristic information when Beta is positive.");
                    }

                    logWeight +=
                        beta * Math.Log(eta);
                }

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
                    logWeight + gumbel;

                transitionEvaluations++;

                if (!found || score > selectedScore)
                {
                    found = true;
                    selectedScore = score;
                    selectedComponent = component;
                    selectedKey = key;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    "Ant construction reached an incomplete solution with no feasible candidate.");
            }

            _model.ApplyComponent(
                ref solution,
                in selectedComponent,
                problem);

            path.Add(selectedKey);
            steps++;
        }

        return new AntColonyConstructionResult<TSolution, TPheromoneKey>(
            solution,
            path,
            steps,
            transitionEvaluations);
    }
}
