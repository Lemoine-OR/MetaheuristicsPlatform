using MetaheuristicsPlatform.Algorithms.SymbioticOrganismsSearch;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;
namespace MetaheuristicsPlatform.Tests;
public sealed class SOSScientificTests {
[Fact] public void DescriptorAndFactoryUseCanonicalScientificIdentity(){var optimizer=new SymbioticOrganismsSearchOptimizer();Assert.Equal(MetaheuristicAlgorithmIds.SymbioticOrganismsSearch,optimizer.Descriptor.Id);Assert.Contains(optimizer.Descriptor.References,r=>r.Doi=="10.1016/j.compstruc.2014.03.007");Assert.NotNull(MetaheuristicFactory.Create<SymbioticOrganismsSearchOptimizer>(MetaheuristicAlgorithmIds.SymbioticOrganismsSearch));}
[Fact] public void SameSeedProducesSameResult(){var a=Run(12345UL);var b=Run(12345UL);Assert.Equal(a.BestFitness,b.BestFitness);Assert.Equal(a.BestSolution,b.BestSolution);Assert.Equal(a.Statistics.Evaluations,b.Statistics.Evaluations);}
[Fact] public void OneCompleteIterationHasValidatedEvaluationAccounting(){var result=new SymbioticOrganismsSearchOptimizer().Optimize(CreateSphere(4),new SymbioticOrganismsSearchParameters { PopulationSize=6, MaximumIterations=1 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=77UL},cancellationToken:TestContext.Current.CancellationToken);Assert.Equal(30, result.Statistics.Evaluations);Assert.Equal(1,result.Statistics.Iterations);}
[Fact] public void ObjectiveDomainContractIsExplicit(){OptimizationResult<double[]> result = new SymbioticOrganismsSearchOptimizer().Optimize(CreateLinearMaximizationProblem(4), new SymbioticOrganismsSearchParameters { PopulationSize=6, MaximumIterations=1 }, new ArraySolutionCloner<double>(), new NeverStoppingCriterion(), new OptimizationOptions { Seed=91UL }, cancellationToken: TestContext.Current.CancellationToken); Assert.True(double.IsFinite(result.BestFitness));}
[Fact] public void InvalidScientificControlsAreRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new SymbioticOrganismsSearchParameters { MaximumIterations=0 }.Validate());}
private static OptimizationResult<double[]> Run(ulong seed)=>new SymbioticOrganismsSearchOptimizer().Optimize(CreateSphere(5),new SymbioticOrganismsSearchParameters { PopulationSize=6, MaximumIterations=2 },new ArraySolutionCloner<double>(),new NeverStoppingCriterion(),new OptimizationOptions{Seed=seed},cancellationToken:TestContext.Current.CancellationToken);
private static ContinuousOptimizationProblem CreateSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,Sphere);
private static ContinuousOptimizationProblem CreateShiftedSphere(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Minimize,static x=>1.0+Sphere(x));
private static ContinuousOptimizationProblem CreateLinearMaximizationProblem(int dimension)=>new(BoundedContinuousSearchSpace.Uniform(dimension,-5.0,5.0),OptimizationSense.Maximize,static x=>x[0]);
private static double Sphere(ReadOnlySpan<double> x){double sum=0.0;for(int i=0;i<x.Length;i++)sum+=x[i]*x[i];return sum;}
private sealed class NeverStoppingCriterion:IStoppingCriterion{public string Name=>"Never";public StoppingDecision Evaluate(in OptimizationState state,OptimizationSense sense)=>StoppingDecision.Continue(Name);}
}
