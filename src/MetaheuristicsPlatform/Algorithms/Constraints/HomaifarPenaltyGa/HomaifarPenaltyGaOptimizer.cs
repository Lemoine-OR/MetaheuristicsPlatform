using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.HomaifarPenaltyGa;

public sealed class HomaifarPenaltyGaOptimizer : IConstrainedOptimizer<HomaifarPenaltyGaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.HomaifarPenaltyGa, Name = "Homaifar-Qi-Lai Penalty Genetic Algorithm", Acronym = "HQL-GA",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { HomaifarPenaltyGaOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, HomaifarPenaltyGaParameters parameters, OptimizationOptions? options=null, CancellationToken cancellationToken=default)
{ ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);double mutationProbability=parameters.MutationProbability<0.0?1.0/problem.SearchSpace.Dimension:parameters.MutationProbability;
  for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();AssignScores(population,problem,parameters,generation);List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);while(offspring.Count<parameters.PopulationSize){var first=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));var second=ConstraintToolkit.Tournament(population,random,static(left,right)=>left.Score.CompareTo(right.Score));double[] child=ConstraintToolkit.SbxChild(first.Position,second.Position,problem.SearchSpace,random,parameters.CrossoverProbability,parameters.DistributionIndex);ConstraintToolkit.PolynomialMutate(child,problem.SearchSpace,random,mutationProbability,parameters.DistributionIndex);offspring.Add(ConstraintToolkit.Evaluate(problem,child,ref evaluations));}List<ConstrainedCandidate> union=new(population.Count+offspring.Count);union.AddRange(population);union.AddRange(offspring);AssignScores(union,problem,parameters,generation);population=ConstraintToolkit.Select(union,parameters.PopulationSize,static(left,right)=>left.Score.CompareTo(right.Score));}
  var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed); }
    private static void AssignScores(IReadOnlyList<ConstrainedCandidate> population,IContinuousConstrainedOptimizationProblem problem,HomaifarPenaltyGaParameters parameters,int generation){foreach(var candidate in population)candidate.Score=HomaifarPenalty(candidate,problem,parameters);}    private static double HomaifarPenalty(ConstrainedCandidate candidate,IContinuousConstrainedOptimizationProblem problem,HomaifarPenaltyGaParameters parameters){double[] violations=candidate.Constraints.ViolationVector();if(parameters.ViolationLevelUpperBounds.Count!=violations.Length)throw new ArgumentException("Homaifar level vectors must match the constrained-component count.");double penalty=0.0;for(int j=0;j<violations.Length;j++){double violation=violations[j];if(violation<=0.0)continue;int level=FindViolationLevel(violation,parameters.ViolationLevelUpperBounds[j]);penalty+=parameters.PenaltyCoefficients[j][level]*violation*violation;}return ConstraintToolkit.ObjectiveKey(candidate.Objective,problem.Sense)+penalty;}
    private static int FindViolationLevel(double violation,IReadOnlyList<double> upperBounds){for(int i=0;i<upperBounds.Count;i++)if(violation<=upperBounds[i])return i;return upperBounds.Count-1;}}
