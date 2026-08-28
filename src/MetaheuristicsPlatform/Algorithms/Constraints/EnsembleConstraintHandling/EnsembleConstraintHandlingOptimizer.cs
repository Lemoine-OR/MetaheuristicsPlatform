using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.EnsembleConstraintHandling;

public sealed class EnsembleConstraintHandlingOptimizer : IConstrainedOptimizer<EnsembleConstraintHandlingParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.EnsembleConstraintHandling, Name = "Ensemble of Constraint Handling Techniques", Acronym = "ECHT",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { EnsembleConstraintHandlingOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem,EnsembleConstraintHandlingParameters parameters,OptimizationOptions? options=null,CancellationToken cancellationToken=default){ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<List<ConstrainedCandidate>> subs=new();for(int k=0;k<parameters.PolicyCount;k++)subs.Add(ConstraintToolkit.Initialize(problem,parameters.SubpopulationSize,random,ref evaluations));for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();for(int policy=0;policy<subs.Count;policy++){var pop=subs[policy];List<ConstrainedCandidate> next=new(pop.Count);for(int i=0;i<pop.Count;i++){double[] trialPosition=ConstraintToolkit.DifferentialTrial(pop,i,problem.SearchSpace,random,parameters.DifferentialWeight,parameters.CrossoverProbability);var trial=ConstraintToolkit.Evaluate(problem,trialPosition,ref evaluations);var target=pop[i];next.Add(CompareByPolicy(trial,target,problem,policy,generation,parameters)<=0?trial:target);}subs[policy]=next;}if((generation+1)%parameters.ExchangePeriod==0)ExchangeElites(subs,problem.Sense);}var all=subs.SelectMany(x=>x).ToList();var best=ConstraintToolkit.BestByDeb(all,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);}
    private static int CompareByPolicy(ConstrainedCandidate left,ConstrainedCandidate right,IContinuousConstrainedOptimizationProblem problem,int policyIndex,int generation,EnsembleConstraintHandlingParameters parameters){int policy=policyIndex%4;if(policy==0)return ConstraintToolkit.DebCompare(left,right,problem.Sense);if(policy==1){double l=ConstraintToolkit.ObjectiveKey(left.Objective,problem.Sense)+parameters.StaticPenalty*left.Constraints.TotalViolation;double r=ConstraintToolkit.ObjectiveKey(right.Objective,problem.Sense)+parameters.StaticPenalty*right.Constraints.TotalViolation;return l.CompareTo(r);}if(policy==2){double epsilon=Math.Max(0.0,parameters.InitialEpsilon*(1.0-generation/(double)parameters.MaximumGenerations));bool l=left.Constraints.TotalViolation<=epsilon,r=right.Constraints.TotalViolation<=epsilon;if(l&&!r)return -1;if(!l&&r)return 1;if(l&&r)return ConstraintToolkit.ObjectiveKey(left.Objective,problem.Sense).CompareTo(ConstraintToolkit.ObjectiveKey(right.Objective,problem.Sense));}return left.Constraints.TotalViolation.CompareTo(right.Constraints.TotalViolation);}
    private static void ExchangeElites(IList<List<ConstrainedCandidate>> subs,OptimizationSense sense){var elites=subs.Select(p=>ConstraintToolkit.BestByDeb(p,sense)).ToList();for(int source=0;source<subs.Count;source++){int target=(source+1)%subs.Count;var p=subs[target];int worst=0;for(int i=1;i<p.Count;i++)if(ConstraintToolkit.DebCompare(p[worst],p[i],sense)<0)worst=i;p[worst]=elites[source];}}
}
