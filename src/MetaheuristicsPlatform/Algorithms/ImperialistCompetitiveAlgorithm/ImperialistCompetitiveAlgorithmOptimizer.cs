using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.ImperialistCompetitiveAlgorithm;

public sealed class ImperialistCompetitiveAlgorithmOptimizer :
    IMetaheuristic<double[], ImperialistCompetitiveAlgorithmParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.ImperialistCompetitiveAlgorithm,
        Name = "Imperialist Competitive Algorithm",
        Acronym = "ICA",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Other,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = [ImperialistCompetitiveAlgorithmReferences.AtashpazGargariLucas2007]
    };

    public ImperialistCompetitiveAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ImperialistCompetitiveAlgorithmParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);
        parameters.Validate();

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
            throw new NotSupportedException("ICA requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0) throw new InvalidOperationException("ICA requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] countries = CreatePopulation(n, dimension);
        double[] objectives = new double[n];

        var context = new OptimizationContext<double[]>(Descriptor, problem, solutionCloner, stoppingCriterion, options, callback, cancellationToken);
        var state = new ImperialistCompetitiveAlgorithmState(0, ImperialistCompetitiveAlgorithmPhase.Initialization, parameters.InitialImperialistCount, null);
        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, countries[i]);
            objectives[i] = context.Evaluate(countries[i], state);
            RequireFinite(objectives[i]);
            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop) return context.Complete(stop, state);
        }

        List<Empire> empires = CreateInitialEmpires(objectives, problem.Sense, parameters.InitialImperialistCount, context.Random);
        double[] candidate = new double[dimension];
        double[] direction = new double[dimension];
        double[] orthogonal = new double[dimension];

        for (int iteration = 1; iteration <= parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            state = new ImperialistCompetitiveAlgorithmState(iteration - 1, ImperialistCompetitiveAlgorithmPhase.Assimilation, empires.Count, null);

            foreach (Empire empire in empires)
            {
                foreach (int colony in empire.Colonies.ToArray())
                {
                    Assimilate(countries[colony], countries[empire.ImperialistIndex], candidate, direction, orthogonal, parameters, context.Random);
                    searchSpace.Clamp(candidate);
                    double value = context.Evaluate(candidate, state);
                    RequireFinite(value);
                    Array.Copy(candidate, countries[colony], dimension);
                    objectives[colony] = value;
                    if (problem.Sense.IsBetter(objectives[colony], objectives[empire.ImperialistIndex]))
                        SwapCountries(countries, objectives, colony, empire.ImperialistIndex);
                    StoppingDecision stop = context.EvaluateStopping(state);
                    if (stop.ShouldStop) return context.Complete(stop, state);
                }
            }

            state = new ImperialistCompetitiveAlgorithmState(iteration - 1, ImperialistCompetitiveAlgorithmPhase.Revolution, empires.Count, null);
            foreach (Empire empire in empires)
            {
                foreach (int colony in empire.Colonies.ToArray())
                {
                    if (context.Random.NextDouble() >= parameters.RevolutionRate) continue;
                    searchSpace.Sample(context.Random, countries[colony]);
                    objectives[colony] = context.Evaluate(countries[colony], state);
                    RequireFinite(objectives[colony]);
                    if (problem.Sense.IsBetter(objectives[colony], objectives[empire.ImperialistIndex]))
                        SwapCountries(countries, objectives, colony, empire.ImperialistIndex);
                    StoppingDecision stop = context.EvaluateStopping(state);
                    if (stop.ShouldStop) return context.Complete(stop, state);
                }
            }

            state = new ImperialistCompetitiveAlgorithmState(iteration - 1, ImperialistCompetitiveAlgorithmPhase.Competition, empires.Count, null);
            if (empires.Count > 1)
                ImperialisticCompetition(empires, objectives, problem.Sense, parameters.ColonyCostWeight, context.Random);

            int bestIndex = BestIndex(objectives, problem.Sense);
            state = new ImperialistCompetitiveAlgorithmState(iteration, ImperialistCompetitiveAlgorithmPhase.CompletedIteration, empires.Count, objectives[bestIndex]);
            context.CompleteIteration(state.BestFitness, state);
            StoppingDecision iterationStop = context.EvaluateStopping(state);
            if (iterationStop.ShouldStop) return context.Complete(iterationStop, state);
        }

        return context.Complete(StoppingDecision.Stop("MaximumICAIterations", "The configured ICA iteration limit was reached."), state);
    }

    private static void Assimilate(double[] colony, double[] imperialist, double[] candidate, double[] direction, double[] orthogonal, ImperialistCompetitiveAlgorithmParameters parameters, IRandomSource random)
    {
        double distanceSquared = 0.0;
        for (int d = 0; d < colony.Length; d++) { direction[d] = imperialist[d] - colony[d]; distanceSquared += direction[d] * direction[d]; }
        double distance = Math.Sqrt(distanceSquared);
        if (distance == 0.0) { Array.Copy(colony, candidate, colony.Length); return; }
        for (int d = 0; d < colony.Length; d++) direction[d] /= distance;
        double step = parameters.AssimilationCoefficient * distance * random.NextDouble();
        double angle = parameters.AssimilationAngleCoefficient * (2.0 * random.NextDouble() - 1.0);
        if (colony.Length == 1) { candidate[0] = colony[0] + step * direction[0]; return; }
        double dot = 0.0;
        double normSquared = 0.0;
        for (int d = 0; d < colony.Length; d++) { orthogonal[d] = 2.0 * random.NextDouble() - 1.0; dot += orthogonal[d] * direction[d]; }
        for (int d = 0; d < colony.Length; d++) { orthogonal[d] -= dot * direction[d]; normSquared += orthogonal[d] * orthogonal[d]; }
        if (normSquared <= double.Epsilon) { Array.Clear(orthogonal, 0, orthogonal.Length); orthogonal[0] = -direction[1]; orthogonal[1] = direction[0]; normSquared = orthogonal[0]*orthogonal[0] + orthogonal[1]*orthogonal[1]; }
        double invNorm = 1.0 / Math.Sqrt(normSquared);
        double ca = Math.Cos(angle), sa = Math.Sin(angle);
        for (int d = 0; d < colony.Length; d++) candidate[d] = colony[d] + step * (ca * direction[d] + sa * orthogonal[d] * invNorm);
    }

    private static List<Empire> CreateInitialEmpires(double[] objectives, OptimizationSense sense, int imperialistCount, IRandomSource random)
    {
        int[] order = Enumerable.Range(0, objectives.Length).OrderBy(i => OrientedCost(objectives[i], sense)).ToArray();
        var empires = new List<Empire>(imperialistCount);
        for (int i = 0; i < imperialistCount; i++) empires.Add(new Empire(order[i]));
        double[] power = new double[imperialistCount];
        double worst = Enumerable.Range(0, imperialistCount).Max(i => OrientedCost(objectives[order[i]], sense));
        double total = 0.0;
        for (int i = 0; i < imperialistCount; i++) { power[i] = worst - OrientedCost(objectives[order[i]], sense) + 1e-12; total += power[i]; }
        for (int k = imperialistCount; k < order.Length; k++) empires[Roulette(power, total, random)].Colonies.Add(order[k]);
        return empires;
    }

    private static void ImperialisticCompetition(List<Empire> empires, double[] objectives, OptimizationSense sense, double colonyWeight, IRandomSource random)
    {
        double[] totalCosts = empires.Select(e => TotalCost(e, objectives, sense, colonyWeight)).ToArray();
        int weakest = 0;
        for (int i = 1; i < totalCosts.Length; i++) if (totalCosts[i] > totalCosts[weakest]) weakest = i;
        Empire weak = empires[weakest];
        int transferable = weak.Colonies.Count > 0 ? weak.Colonies.OrderByDescending(i => OrientedCost(objectives[i], sense)).First() : weak.ImperialistIndex;
        double maxCost = totalCosts.Max();
        double[] power = new double[empires.Count];
        double totalPower = 0.0;
        for (int i = 0; i < empires.Count; i++) { if (i == weakest) continue; power[i] = maxCost - totalCosts[i] + 1e-12; totalPower += power[i]; }
        int winner = Roulette(power, totalPower, random);
        if (winner == weakest) winner = Enumerable.Range(0, empires.Count).First(i => i != weakest);
        if (weak.Colonies.Count > 0) weak.Colonies.Remove(transferable);
        empires[winner].Colonies.Add(transferable);
        if (weak.Colonies.Count == 0)
        {
            if (transferable != weak.ImperialistIndex) empires[winner].Colonies.Add(weak.ImperialistIndex);
            empires.RemoveAt(weakest);
        }
    }

    private static double TotalCost(Empire e, double[] objectives, OptimizationSense sense, double weight)
    {
        double result = OrientedCost(objectives[e.ImperialistIndex], sense);
        if (e.Colonies.Count == 0) return result;
        double mean = e.Colonies.Average(i => OrientedCost(objectives[i], sense));
        return result + weight * mean;
    }

    private static int Roulette(double[] weights, double total, IRandomSource random)
    {
        if (!(total > 0.0) || !double.IsFinite(total)) return 0;
        double threshold = random.NextDouble() * total, cumulative = 0.0;
        for (int i = 0; i < weights.Length; i++) { cumulative += weights[i]; if (threshold < cumulative) return i; }
        return weights.Length - 1;
    }

    private static double OrientedCost(double value, OptimizationSense sense) => sense == OptimizationSense.Minimize ? value : -value;
    private static void SwapCountries(double[][] x, double[] f, int a, int b) { (x[a], x[b]) = (x[b], x[a]); (f[a], f[b]) = (f[b], f[a]); }
    private static int BestIndex(ReadOnlySpan<double> values, OptimizationSense sense) { int b=0; for(int i=1;i<values.Length;i++) if(sense.IsBetter(values[i],values[b])) b=i; return b; }
    private static double[][] CreatePopulation(int count, int dimension) { var x=new double[count][]; for(int i=0;i<count;i++) x[i]=new double[dimension]; return x; }
    private static void RequireFinite(double value) { if (!double.IsFinite(value)) throw new InvalidOperationException("ICA requires finite objective values."); }
    private sealed class Empire(int imperialistIndex) { public int ImperialistIndex { get; } = imperialistIndex; public List<int> Colonies { get; } = []; }
}
