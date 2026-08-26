using MetaheuristicsPlatform.Algorithms.MultiVerseOptimizer;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;
namespace MetaheuristicsPlatform.Tests;
public sealed class MVOScientificTests {
[Fact] public void DescriptorAndFactoryUseCanonicalScientificIdentity(){var optimizer=new MultiVerseOptimizer();Assert.Equal(MetaheuristicAlgorithmIds.MultiVerseOptimizer,optimizer.Descriptor.Id);Assert.Contains(optimizer.Descriptor.References,r=>r.Doi=="10.1007/s00521-015-1870-7");Assert.NotNull(MetaheuristicFactory.Create<MultiVerseOptimizer>(MetaheuristicAlgorithmIds.MultiVerseOptimizer));}
[Fact] public void SameSeedProducesSameResult(){var a=Run(12345UL);var b=Run(12345UL);Assert.Equal(a.BestFitness,b.BestFitness);Assert.Equal(a.BestSolution,b.BestSolution);Assert.Equal(a.Statistics.Evaluations,b.Statistics.Evaluations);}
[Fact] public void OneCompleteIterationHasValidatedEvaluationAccounting(){var result=new MultiVerseOptimizer().Optimize(CreateShiftedSphere(4),new MultiVerseOptimizerParameters { PopulationSize=6, MaximumIterations=1 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=77UL},cancellationToken:TestContext.Current.CancellationToken);Assert.Equal(12, result.Statistics.Evaluations);Assert.Equal(1,result.Statistics.Iterations);}
[Fact] public void ObjectiveDomainContractIsExplicit(){Assert.Throws<NotSupportedException>(()=>new MultiVerseOptimizer().Optimize(CreateLinearMaximizationProblem(4),new MultiVerseOptimizerParameters { PopulationSize=6, MaximumIterations=1 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(), cancellationToken: TestContext.Current.CancellationToken));}
[Fact] public void InvalidScientificControlsAreRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new MultiVerseOptimizerParameters { ExploitationAccuracy=0.0 }.Validate());}
private static OptimizationResult<double[]> Run(ulong seed)=>new MultiVerseOptimizer().Optimize(CreateShiftedSphere(5),new MultiVerseOptimizerParameters { PopulationSize=6, MaximumIterations=2 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=seed},cancellationToken:TestContext.Current.CancellationToken);
private static ContinuousOptimizationProblem CreateSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,Sphere);
private static ContinuousOptimizationProblem CreateShiftedSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,static x=>1.0+Sphere(x));
private static ContinuousOptimizationProblem CreateLinearMaximizationProblem(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Maximize,static x=>x[0]);
private static double Sphere(ReadOnlySpan<double> x){double sum=0.0;for(int i=0;i<x.Length;i++)sum+=x[i]*x[i];return sum;}
private sealed class NeverStoppingCriterion:IStoppingCriterion{public string Name=>"Never";public StoppingDecision Evaluate(in OptimizationState state,OptimizationSense sense)=>StoppingDecision.Continue(Name);}
}
