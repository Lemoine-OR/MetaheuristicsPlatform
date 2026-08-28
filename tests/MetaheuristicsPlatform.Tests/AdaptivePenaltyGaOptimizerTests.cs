using MetaheuristicsPlatform.Algorithms.Constraints.AdaptivePenaltyGa;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;
public sealed class AdaptivePenaltyGaOptimizerTests
{
 [Fact] public void Optimize_UsesNativeConstraintContract_AndFactoryCreatesCanonicalType()
 {
   var problem=CreateProblem();
   ConstrainedOptimizationResult result=new AdaptivePenaltyGaOptimizer().Optimize(problem,new AdaptivePenaltyGaParameters { MaximumGenerations = 4 },new OptimizationOptions{Seed=11223344UL},cancellationToken: TestContext.Current.CancellationToken);
   Assert.True(result.Evaluations>0);Assert.True(double.IsFinite(result.Best.Objective));Assert.True(result.Best.Constraints.TotalViolation>=0.0);
   Assert.IsType<AdaptivePenaltyGaOptimizer>(MetaheuristicFactory.Create<AdaptivePenaltyGaOptimizer>(MetaheuristicAlgorithmIds.AdaptivePenaltyGa));
 }
 private static ContinuousConstrainedOptimizationProblem CreateProblem()=>new(BoundedContinuousSearchSpace.Uniform(2,0.0,1.0),OptimizationSense.Minimize,1,0,static x=>x[0]*x[0]+x[1]*x[1],static(ReadOnlySpan<double> x,Span<double> inequalities,Span<double> equalities)=>{inequalities[0]=1.0-x[0]-x[1];});
}
