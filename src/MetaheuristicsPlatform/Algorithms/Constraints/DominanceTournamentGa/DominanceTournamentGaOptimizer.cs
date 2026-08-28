using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.DominanceTournamentGa;

public sealed class DominanceTournamentGaOptimizer : IConstrainedOptimizer<DominanceTournamentGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.DominanceTournamentGa, Name = "Dominance-Based Tournament Genetic Algorithm", Acronym = "DBT-GA",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { DominanceTournamentGaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, DominanceTournamentGaParameters parameters, OptimizationOptions? options=null, CancellationToken cancellationToken=default)
{
    ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();
    IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;
    List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);
    double mutationProbability=parameters.MutationProbability<0.0?1.0/problem.SearchSpace.Dimension:parameters.MutationProbability;
    for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);while(offspring.Count<parameters.PopulationSize){
        ConstrainedCandidate first=ConstraintToolkit.Tournament(population,random,(left,right)=>DominanceCompare(left,right,problem.Sense));ConstrainedCandidate second=ConstraintToolkit.Tournament(population,random,(left,right)=>DominanceCompare(left,right,problem.Sense));
        double[] child=ConstraintToolkit.SbxChild(first.Position,second.Position,problem.SearchSpace,random,parameters.CrossoverProbability,parameters.DistributionIndex);ConstraintToolkit.PolynomialMutate(child,problem.SearchSpace,random,mutationProbability,parameters.DistributionIndex);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}
        population=ConstraintToolkit.Select(population.Concat(offspring),parameters.PopulationSize,(left,right)=>{int order=DominanceCompare(left,right,problem.Sense);return order!=0?order:ConstraintToolkit.DebCompare(left,right,problem.Sense);});}
    ConstrainedCandidate best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);
}
private static int DominanceCompare(ConstrainedCandidate left, ConstrainedCandidate right, OptimizationSense sense)
{
    double lf=ConstraintToolkit.ObjectiveKey(left.Objective,sense), rf=ConstraintToolkit.ObjectiveKey(right.Objective,sense); double lv=left.Constraints.TotalViolation, rv=right.Constraints.TotalViolation;
    bool l=lf<=rf&&lv<=rv, r=rf<=lf&&rv<=lv; if(l&&!r)return -1;if(r&&!l)return 1;return 0;
}
}
