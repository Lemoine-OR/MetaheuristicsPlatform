using MetaheuristicsPlatform.Algorithms.ImperialistCompetitiveAlgorithm;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;
namespace MetaheuristicsPlatform.Tests;
public sealed class ICAScientificTests {
[Fact] public void DescriptorAndFactoryUseCanonicalScientificIdentity(){var optimizer=new ImperialistCompetitiveAlgorithmOptimizer();Assert.Equal(MetaheuristicAlgorithmIds.ImperialistCompetitiveAlgorithm,optimizer.Descriptor.Id);Assert.Contains(optimizer.Descriptor.References,r=>r.Doi=="10.1109/CEC.2007.4425083");Assert.NotNull(MetaheuristicFactory.Create<ImperialistCompetitiveAlgorithmOptimizer>(MetaheuristicAlgorithmIds.ImperialistCompetitiveAlgorithm));}
[Fact] public void SameSeedProducesSameResult(){var a=Run(12345UL);var b=Run(12345UL);Assert.Equal(a.BestFitness,b.BestFitness);Assert.Equal(a.BestSolution,b.BestSolution);Assert.Equal(a.Statistics.Evaluations,b.Statistics.Evaluations);}
[Fact] public void OneCompleteIterationHasValidatedEvaluationAccounting(){var result=new ImperialistCompetitiveAlgorithmOptimizer().Optimize(CreateSphere(4),new ImperialistCompetitiveAlgorithmParameters { PopulationSize=6, InitialImperialistCount=2, MaximumIterations=1, RevolutionRate=0.0 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=77UL},cancellationToken:TestContext.Current.CancellationToken);Assert.Equal(10, result.Statistics.Evaluations);Assert.Equal(1,result.Statistics.Iterations);}
[Fact] public void ObjectiveDomainContractIsExplicit(){OptimizationResult<double[]> result = new ImperialistCompetitiveAlgorithmOptimizer().Optimize(CreateLinearMaximizationProblem(4), new ImperialistCompetitiveAlgorithmParameters { PopulationSize=6, InitialImperialistCount=2, MaximumIterations=1, RevolutionRate=0.0 }, new ArraySolutionCloner<double>(), new NeverStoppingCriterion(), new OptimizationOptions { Seed=91UL }, cancellationToken: TestContext.Current.CancellationToken); Assert.True(double.IsFinite(result.BestFitness));}
[Fact] public void InvalidScientificControlsAreRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new ImperialistCompetitiveAlgorithmParameters { InitialImperialistCount=1 }.Validate());}
private static OptimizationResult<double[]> Run(ulong seed)=>new ImperialistCompetitiveAlgorithmOptimizer().Optimize(CreateSphere(5),new ImperialistCompetitiveAlgorithmParameters { PopulationSize=6, InitialImperialistCount=2, MaximumIterations=2, RevolutionRate=0.0 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=seed},cancellationToken:TestContext.Current.CancellationToken);
private static ContinuousOptimizationProblem CreateSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,Sphere);
private static ContinuousOptimizationProblem CreateShiftedSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,static x=>1.0+Sphere(x));
private static ContinuousOptimizationProblem CreateLinearMaximizationProblem(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Maximize,static x=>x[0]);
private static double Sphere(ReadOnlySpan<double> x){double sum=0.0;for(int i=0;i<x.Length;i++)sum+=x[i]*x[i];return sum;}
private sealed class NeverStoppingCriterion:IStoppingCriterion{public string Name=>"Never";public StoppingDecision Evaluate(in OptimizationState state,OptimizationSense sense)=>StoppingDecision.Continue(Name);}
}
