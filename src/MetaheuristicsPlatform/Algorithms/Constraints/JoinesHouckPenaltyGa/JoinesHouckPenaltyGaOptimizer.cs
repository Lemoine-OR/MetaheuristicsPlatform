using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.JoinesHouckPenaltyGa;

public sealed class JoinesHouckPenaltyGaOptimizer : IConstrainedOptimizer<JoinesHouckPenaltyGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.JoinesHouckPenaltyGa, Name = "Joines-Houck Nonstationary Penalty Genetic Algorithm", Acronym = "JH-NPGA",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { JoinesHouckPenaltyGaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, JoinesHouckPenaltyGaParameters parameters, OptimizationOptions? options=null, CancellationToken cancellationToken=default)
{ ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);double mutationProbability=parameters.MutationProbability<0.0?1.0/problem.SearchSpace.Dimension:parameters.MutationProbability;
  for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();AssignScores(population,problem,parameters,generation);List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);while(offspring.Count<parameters.PopulationSize){var first=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));var second=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));double[] child=ConstraintToolkit.SbxChild(first.Position,second.Position,problem.SearchSpace,random,parameters.CrossoverProbability,parameters.DistributionIndex);ConstraintToolkit.PolynomialMutate(child,problem.SearchSpace,random,mutationProbability,parameters.DistributionIndex);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}List<ConstrainedCandidate> union=new(population.Count+offspring.Count);union.AddRange(population);union.AddRange(offspring);AssignScores(union,problem,parameters,generation);population=ConstraintToolkit.Select(union,parameters.PopulationSize,static(left,right)=>left.Score.CompareTo(right.Score));}
  var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed); }
    private static void AssignScores(IReadOnlyList<ConstrainedCandidate> population,IContinuousConstrainedOptimizationProblem problem,JoinesHouckPenaltyGaParameters parameters,int generation){double scale=Math.Pow(parameters.PenaltyConstant*(generation+1),parameters.Alpha);foreach(var candidate in population)candidate.Score=NonstationaryPenalty(candidate,problem,scale,parameters.Beta);}    private static double NonstationaryPenalty(ConstrainedCandidate candidate,IContinuousConstrainedOptimizationProblem problem,double scale,double beta)=>ConstraintToolkit.ObjectiveKey(candidate.Objective,problem.Sense)+scale*ConstraintToolkit.ViolationPower(candidate,beta);}
