using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.TessemaYenPenaltyGa;

public sealed class TessemaYenPenaltyGaOptimizer : IConstrainedOptimizer<TessemaYenPenaltyGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.TessemaYenPenaltyGa, Name = "Tessema-Yen Adaptive Penalty Genetic Algorithm", Acronym = "TY-APF",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { TessemaYenPenaltyGaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, TessemaYenPenaltyGaParameters parameters, OptimizationOptions? options=null, CancellationToken cancellationToken=default)
{ ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);double mutationProbability=parameters.MutationProbability<0.0?1.0/problem.SearchSpace.Dimension:parameters.MutationProbability;
  for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();AssignScores(population,problem,parameters,generation);List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);while(offspring.Count<parameters.PopulationSize){var first=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));var second=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));double[] child=ConstraintToolkit.SbxChild(first.Position,second.Position,problem.SearchSpace,random,parameters.CrossoverProbability,parameters.DistributionIndex);ConstraintToolkit.PolynomialMutate(child,problem.SearchSpace,random,mutationProbability,parameters.DistributionIndex);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}List<ConstrainedCandidate> union=new(population.Count+offspring.Count);union.AddRange(population);union.AddRange(offspring);AssignScores(union,problem,parameters,generation);population=ConstraintToolkit.Select(union,parameters.PopulationSize,static(left,right)=>left.Score.CompareTo(right.Score));}
  var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed); }
    private static void AssignScores(IReadOnlyList<ConstrainedCandidate> population,IContinuousConstrainedOptimizationProblem problem,TessemaYenPenaltyGaParameters parameters,int generation){double feasibleRatio=population.Count(x=>x.Constraints.IsFeasible)/(double)population.Count;double minF=population.Min(x=>ConstraintToolkit.ObjectiveKey(x.Objective,problem.Sense));double maxF=population.Max(x=>ConstraintToolkit.ObjectiveKey(x.Objective,problem.Sense));double maxV=Math.Max(population.Max(x=>x.Constraints.TotalViolation),1e-30);foreach(var x in population)x.Score=AdaptivePenaltyScore(x,problem,feasibleRatio,minF,maxF,maxV);}    private static double AdaptivePenaltyScore(ConstrainedCandidate x,IContinuousConstrainedOptimizationProblem problem,double feasibleRatio,double minF,double maxF,double maxV){double f=ConstraintToolkit.ObjectiveKey(x.Objective,problem.Sense);double nf=(f-minF)/Math.Max(maxF-minF,1e-30);double nv=x.Constraints.TotalViolation/maxV;if(x.Constraints.IsFeasible)return nf;if(feasibleRatio<=0.0)return nv;double d=Math.Sqrt(nf*nf+nv*nv);return d+(1.0-feasibleRatio)*nv+feasibleRatio*nf;}}
