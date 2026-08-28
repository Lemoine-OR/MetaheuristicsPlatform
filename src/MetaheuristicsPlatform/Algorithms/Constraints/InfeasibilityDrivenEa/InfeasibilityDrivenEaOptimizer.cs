using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.InfeasibilityDrivenEa;

public sealed class InfeasibilityDrivenEaOptimizer : IConstrainedOptimizer<InfeasibilityDrivenEaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.InfeasibilityDrivenEa, Name = "Infeasibility Driven Evolutionary Algorithm", Acronym = "IDEA",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { InfeasibilityDrivenEaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem,InfeasibilityDrivenEaParameters parameters,OptimizationOptions? options=null,CancellationToken cancellationToken=default){ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);for(int i=0;i<parameters.PopulationSize;i++){double[] trial=ConstraintToolkit.DifferentialTrial(population,i,problem.SearchSpace,random,parameters.DifferentialWeight,parameters.CrossoverProbability);offspring.Add(ConstraintToolkit.Evaluate(problem,trial,ref evaluations));}population=SelectIdeaPopulation(population.Concat(offspring),parameters.PopulationSize,parameters.InfeasibleFraction,problem.Sense);}var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);}
    private static List<ConstrainedCandidate> SelectIdeaPopulation(IEnumerable<ConstrainedCandidate> candidates,int populationSize,double infeasibleFraction,OptimizationSense sense){var feasible=candidates.Where(x=>x.Constraints.IsFeasible).OrderBy(x=>ConstraintToolkit.ObjectiveKey(x.Objective,sense)).ToList();var infeasible=candidates.Where(x=>!x.Constraints.IsFeasible).OrderBy(x=>x.Constraints.TotalViolation).ThenBy(x=>ConstraintToolkit.ObjectiveKey(x.Objective,sense)).ToList();int infeasibleSlots=Math.Min(infeasible.Count,(int)Math.Floor(populationSize*infeasibleFraction));int feasibleSlots=Math.Min(feasible.Count,populationSize-infeasibleSlots);List<ConstrainedCandidate> selected=feasible.Take(feasibleSlots).Concat(infeasible.Take(infeasibleSlots)).ToList();if(selected.Count<populationSize)selected.AddRange(feasible.Skip(feasibleSlots).Take(populationSize-selected.Count));if(selected.Count<populationSize)selected.AddRange(infeasible.Skip(infeasibleSlots).Take(populationSize-selected.Count));return selected;}
}
