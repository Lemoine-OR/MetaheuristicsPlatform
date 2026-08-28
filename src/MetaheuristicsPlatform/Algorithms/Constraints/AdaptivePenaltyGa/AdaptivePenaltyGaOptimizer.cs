using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.AdaptivePenaltyGa;

public sealed class AdaptivePenaltyGaOptimizer : IConstrainedOptimizer<AdaptivePenaltyGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.AdaptivePenaltyGa, Name = "Lemonge-Barbosa Adaptive Penalty Genetic Algorithm", Acronym = "APM-GA",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { AdaptivePenaltyGaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, AdaptivePenaltyGaParameters parameters, OptimizationOptions? options=null, CancellationToken cancellationToken=default)
{ ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);double mutationProbability=parameters.MutationProbability<0.0?1.0/problem.SearchSpace.Dimension:parameters.MutationProbability;
  for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();AssignScores(population,problem,parameters,generation);List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);while(offspring.Count<parameters.PopulationSize){var first=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));var second=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));double[] child=ConstraintToolkit.SbxChild(first.Position,second.Position,problem.SearchSpace,random,parameters.CrossoverProbability,parameters.DistributionIndex);ConstraintToolkit.PolynomialMutate(child,problem.SearchSpace,random,mutationProbability,parameters.DistributionIndex);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}List<ConstrainedCandidate> union=new(population.Count+offspring.Count);union.AddRange(population);union.AddRange(offspring);AssignScores(union,problem,parameters,generation);population=ConstraintToolkit.Select(union,parameters.PopulationSize,static(left,right)=>left.Score.CompareTo(right.Score));}
  var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed); }
    private static void AssignScores(IReadOnlyList<ConstrainedCandidate> population,IContinuousConstrainedOptimizationProblem problem,AdaptivePenaltyGaParameters parameters,int generation){int c=problem.InequalityCount+problem.EqualityCount;double[] avgV=new double[c];double avgF=0.0;foreach(var x in population){avgF+=ConstraintToolkit.ObjectiveKey(x.Objective,problem.Sense);double[] v=x.Constraints.ViolationVector();for(int j=0;j<c;j++)avgV[j]+=v[j];}avgF/=population.Count;for(int j=0;j<c;j++)avgV[j]/=population.Count;double[] k=AdaptiveCoefficients(avgF,avgV);foreach(var x in population)x.Score=PenaltyScore(x,problem,avgF,k);}    private static double[] AdaptiveCoefficients(double avgF,IReadOnlyList<double> avgV){double d=0.0;for(int j=0;j<avgV.Count;j++)d+=avgV[j]*avgV[j];double[] k=new double[avgV.Count];if(d<=1e-30)return k;for(int j=0;j<k.Length;j++)k[j]=Math.Abs(avgF)*avgV[j]/d;return k;}
    private static double PenaltyScore(ConstrainedCandidate x,IContinuousConstrainedOptimizationProblem problem,double avgF,IReadOnlyList<double> k){double f=ConstraintToolkit.ObjectiveKey(x.Objective,problem.Sense);if(x.Constraints.IsFeasible)return f;double score=Math.Max(f,avgF);double[] v=x.Constraints.ViolationVector();for(int j=0;j<v.Length;j++)score+=k[j]*v[j];return score;}}
