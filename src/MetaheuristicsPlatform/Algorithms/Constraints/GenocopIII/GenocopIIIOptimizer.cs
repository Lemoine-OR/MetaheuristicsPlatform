using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constraints.GenocopIII;

public sealed class GenocopIIIOptimizer : IConstrainedOptimizer<GenocopIIIParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.GenocopIII, Name = "GENOCOP III", Acronym = "GENOCOP-III",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Evolutionary,
        Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = new[] { GenocopIIIOptimizerReferences.Primary }
    };
public ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem,GenocopIIIParameters parameters,OptimizationOptions? options=null,CancellationToken cancellationToken=default){ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);parameters.Validate();IRandomSource random=ConstraintToolkit.CreateRandom(options,out ulong seed);int evaluations=0;List<ConstrainedCandidate> searchPopulation=ConstraintToolkit.Initialize(problem,parameters.PopulationSize,random,ref evaluations);List<ConstrainedCandidate> referencePopulation=searchPopulation.Where(x=>x.Constraints.IsFeasible).ToList();EnsureReferencePopulation(problem,parameters,random,referencePopulation,ref evaluations);for(int generation=0;generation<parameters.MaximumGenerations;generation++){cancellationToken.ThrowIfCancellationRequested();List<ConstrainedCandidate> offspring=new(parameters.PopulationSize);foreach(var candidate in searchPopulation){var reference=referencePopulation[random.NextInt32(referencePopulation.Count)];double[] repaired=RepairTowardReference(problem,candidate.Position,reference.Position,parameters.BisectionSteps);offspring.Add(ConstraintToolkit.Evaluate(problem,repaired,ref evaluations));}searchPopulation=ConstraintToolkit.Select(searchPopulation.Concat(offspring),parameters.PopulationSize,(l,r)=>ConstraintToolkit.DebCompare(l,r,problem.Sense));foreach(var candidate in searchPopulation)if(candidate.Constraints.IsFeasible)referencePopulation.Add(candidate);referencePopulation=ConstraintToolkit.Select(referencePopulation,parameters.ReferencePopulationSize,(l,r)=>ConstraintToolkit.DebCompare(l,r,problem.Sense));}var best=ConstraintToolkit.BestByDeb(referencePopulation,problem.Sense);return new ConstrainedOptimizationResult(ConstraintToolkit.ToPoint(best),evaluations,parameters.MaximumGenerations,seed);}
    private static void EnsureReferencePopulation(IContinuousConstrainedOptimizationProblem problem,GenocopIIIParameters parameters,IRandomSource random,List<ConstrainedCandidate> referencePopulation,ref int evaluations){int attempts=0;while(referencePopulation.Count<parameters.ReferencePopulationSize&&attempts<parameters.MaximumReferenceAttempts){double[] position=new double[problem.SearchSpace.Dimension];problem.SearchSpace.Sample(random,position);var c=ConstraintToolkit.Evaluate(problem,position,ref evaluations);if(c.Constraints.IsFeasible)referencePopulation.Add(c);attempts++;}if(referencePopulation.Count==0)throw new InvalidOperationException("GENOCOP III requires at least one feasible reference point.");}
    private static double[] RepairTowardReference(IContinuousConstrainedOptimizationProblem problem,ReadOnlySpan<double> search,ReadOnlySpan<double> reference,int steps){double low=0.0,high=1.0;double[] best=reference.ToArray();for(int step=0;step<steps;step++){double alpha=0.5*(low+high);double[] trial=new double[search.Length];for(int i=0;i<trial.Length;i++)trial[i]=(1.0-alpha)*reference[i]+alpha*search[i];if(IsFeasible(problem,trial)){best=trial;low=alpha;}else high=alpha;}return best;}
    private static bool IsFeasible(IContinuousConstrainedOptimizationProblem problem,double[] position){double[] g=new double[problem.InequalityCount];double[] h=new double[problem.EqualityCount];problem.EvaluateConstraints(position,g,h);return new ConstraintEvaluation(g,h,problem.EqualityTolerance).IsFeasible;}
}
