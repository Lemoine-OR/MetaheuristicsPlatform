using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.StochasticRankingEs;

public sealed class StochasticRankingEsOptimizer : IConstrainedOptimizer<StochasticRankingEsParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.StochasticRankingEs, Name = "Stochastic Ranking Evolution Strategy", Acronym = "SRES",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { StochasticRankingEsOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem,StochasticRankingEsParameters parameters,OptimizationOptions? options=null,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.Lambda,random,ref evaluations);double sigma=parameters.InitialSigma;for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();StochasticRank(population,problem.Sense,parameters.ProbabilityObjective,random);List<ConstrainedCandidate> parents=population.Take(parameters.Mu).ToList();List<ConstrainedCandidate> offspring=new(parameters.Lambda);while(offspring.Count<parameters.Lambda){var parent=parents[random.NextInt32(parents.Count)];double[] child=ConstraintToolkit.GaussianChild(parent.Position,problem.SearchSpace,random,sigma);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}population=offspring;sigma=Math.Max(parameters.MinimumSigma,sigma*parameters.SigmaDecay);}var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);}
    private static void StochasticRank(IList<ConstrainedCandidate> population,OptimizationSense sense,double probabilityObjective,IRandomSource random){for(int pass=0;pass<population.Count;pass++){bool swapped=false;for(int i=0;i<population.Count-1;i++){var left=population[i];var right=population[i+1];bool byObjective=(left.Constraints.IsFeasible&&right.Constraints.IsFeasible)||random.NextDouble()<probabilityObjective;int order=byObjective?ConstraintToolkit.ObjectiveKey(left.Objective,sense).CompareTo(ConstraintToolkit.ObjectiveKey(right.Objective,sense)):left.Constraints.TotalViolation.CompareTo(right.Constraints.TotalViolation);if(order>0){population[i]=right;population[i+1]=left;swapped=true;}}if(!swapped)break;}}
}
