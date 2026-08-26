using MetaheuristicsPlatform.Algorithms.BlackHole;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;
namespace MetaheuristicsPlatform.Tests;
public sealed class BHScientificTests {
[Fact] public void DescriptorAndFactoryUseCanonicalScientificIdentity(){var optimizer=new BlackHoleOptimizer();Assert.Equal(MetaheuristicAlgorithmIds.BlackHoleAlgorithm,optimizer.Descriptor.Id);Assert.Contains(optimizer.Descriptor.References,r=>r.Doi=="10.1016/j.ins.2012.08.023");Assert.NotNull(MetaheuristicFactory.Create<BlackHoleOptimizer>(MetaheuristicAlgorithmIds.BlackHoleAlgorithm));}
[Fact] public void SameSeedProducesSameResult(){var a=Run(12345UL);var b=Run(12345UL);Assert.Equal(a.BestFitness,b.BestFitness);Assert.Equal(a.BestSolution,b.BestSolution);Assert.Equal(a.Statistics.Evaluations,b.Statistics.Evaluations);}
[Fact] public void OneCompleteIterationHasValidatedEvaluationAccounting(){var result=new BlackHoleOptimizer().Optimize(CreateShiftedSphere(4),new BlackHoleParameters { PopulationSize=6, MaximumIterations=1 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=77UL},cancellationToken:TestContext.Current.CancellationToken);Assert.InRange(result.Statistics.Evaluations, 11, 16);Assert.Equal(1,result.Statistics.Iterations);}
[Fact] public void ObjectiveDomainContractIsExplicit(){Assert.Throws<NotSupportedException>(() => new BlackHoleOptimizer().Optimize(CreateLinearMaximizationProblem(4), new BlackHoleParameters { PopulationSize=6, MaximumIterations=1 }, new ArraySolutionCloner<double>(), new NeverStoppingCriterion(), cancellationToken: TestContext.Current.CancellationToken));}
[Fact] public void InvalidScientificControlsAreRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new BlackHoleParameters { PopulationSize=1 }.Validate());}
private static OptimizationResult<double[]> Run(ulong seed)=>new BlackHoleOptimizer().Optimize(CreateShiftedSphere(5),new BlackHoleParameters { PopulationSize=6, MaximumIterations=2 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=seed},cancellationToken:TestContext.Current.CancellationToken);
private static ContinuousOptimizationProblem CreateSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,Sphere);
private static ContinuousOptimizationProblem CreateShiftedSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,static x=>1.0+Sphere(x));
private static ContinuousOptimizationProblem CreateLinearMaximizationProblem(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Maximize,static x=>x[0]);
private static double Sphere(ReadOnlySpan<double> x){double sum=0.0;for(int i=0;i<x.Length;i++)sum+=x[i]*x[i];return sum;}
private sealed class NeverStoppingCriterion:IStoppingCriterion{public string Name=>"Never";public StoppingDecision Evaluate(in OptimizationState state,OptimizationSense sense)=>StoppingDecision.Continue(Name);}
}
