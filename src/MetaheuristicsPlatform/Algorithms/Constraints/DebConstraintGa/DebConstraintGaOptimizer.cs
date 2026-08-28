using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.DebConstraintGa;

public sealed class DebConstraintGaOptimizer : IConstrainedOptimizer<DebConstraintGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.DebConstraintGa, Name = "Deb Feasibility Rules Genetic Algorithm", Acronym = "Deb-FR-GA",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { DebConstraintGaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, DebConstraintGaParameters parameters, OptimizationOptions? options=null, CancellationToken cancellationToken=default)
{
    ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();
    IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;
    List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);
    double mutationProbability=parameters.MutationProbability<0.0?1.0/problem.SearchSpace.Dimension:parameters.MutationProbability;
    for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);while(offspring.Count<parameters.PopulationSize){
        ConstrainedCandidate first=ConstraintToolkit.Tournament(population,random,(left,right)=>ConstraintToolkit.DebCompare(left,right,problem.Sense));ConstrainedCandidate second=ConstraintToolkit.Tournament(population,random,(left,right)=>ConstraintToolkit.DebCompare(left,right,problem.Sense));
        double[] child=ConstraintToolkit.SbxChild(first.Position,second.Position,problem.SearchSpace,random,parameters.CrossoverProbability,parameters.DistributionIndex);ConstraintToolkit.PolynomialMutate(child,problem.SearchSpace,random,mutationProbability,parameters.DistributionIndex);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}
        population=ConstraintToolkit.Select(population.Concat(offspring),parameters.PopulationSize,(left,right)=>{int order=ConstraintToolkit.DebCompare(left,right,problem.Sense);return order!=0?order:ConstraintToolkit.DebCompare(left,right,problem.Sense);});}
    ConstrainedCandidate best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);
}
}
