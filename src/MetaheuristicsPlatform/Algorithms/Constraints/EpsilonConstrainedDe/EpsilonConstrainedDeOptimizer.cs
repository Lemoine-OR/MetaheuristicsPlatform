using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.EpsilonConstrainedDe;

public sealed class EpsilonConstrainedDeOptimizer : IConstrainedOptimizer<EpsilonConstrainedDeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.EpsilonConstrainedDe, Name = "Epsilon-Constrained Differential Evolution", Acronym = "epsilonDE",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { EpsilonConstrainedDeOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem,EpsilonConstrainedDeParameters parameters,OptimizationOptions? options=null,CancellationToken cancellationToken=default){ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> population=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);double epsilon0=population.Select(x=>x.Constraints.TotalViolation).OrderBy(v=>v).ElementAt(Math.Min(population.Count-1,(int)Math.Floor(parameters.InitialEpsilonQuantile*population.Count)));for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();double epsilon=EpsilonAt(epsilon0,generation,parameters.EpsilonControlGenerations,parameters.EpsilonExponent);List<ConstrainedCandidate> next=new(population.Count);for(int i=0;i<population.Count;i++){double[] trialPosition=ConstraintToolkit.DifferentialTrial(population,i,problem.SearchSpace,random,parameters.DifferentialWeight,parameters.CrossoverProbability);var trial=ConstraintToolkit.Evaluate(problem,trialPosition,ref evaluations);var target=population[i];next.Add(EpsilonCompare(trial,target,problem.Sense,epsilon)<=0?trial:target);}population=next;}var best=ConstraintToolkit.BestByDeb(population,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);}
    private static double EpsilonAt(double epsilon0,int generation,int controlGenerations,double exponent){if(generation>=controlGenerations)return 0.0;double ratio=1.0-generation/(double)controlGenerations;return epsilon0*Math.Pow(ratio,exponent);}
    private static int EpsilonCompare(ConstrainedCandidate left,ConstrainedCandidate right,OptimizationSense sense,double epsilon){bool l=left.Constraints.TotalViolation<=epsilon,r=right.Constraints.TotalViolation<=epsilon;if(l&&r)return ConstraintToolkit.ObjectiveKey(left.Objective,sense).CompareTo(ConstraintToolkit.ObjectiveKey(right.Objective,sense));if(l)return -1;if(r)return 1;int v=left.Constraints.TotalViolation.CompareTo(right.Constraints.TotalViolation);return v!=0?v:ConstraintToolkit.ObjectiveKey(left.Objective,sense).CompareTo(ConstraintToolkit.ObjectiveKey(right.Objective,sense));}
}
