using MetaheuristicsPlatform.Algorithms.Constraints.HomaifarPenaltyGa;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;
public sealed class HomaifarPenaltyGaOptimizerTests
{
 [Fact] public void Optimize_UsesNativeConstraintContract_AndFactoryCreatesCanonicalType()
 {
   var problem=CreateProblem();
   ConstrainedOptimizationResult result=new HomaifarPenaltyGaOptimizer().Optimize(problem,new HomaifarPenaltyGaParameters { MaximumGenerations = 4, ViolationLevelUpperBounds = new IReadOnlyList<double>[] { new[] { 0.1, 1.0, 10.0 } }, PenaltyCoefficients = new IReadOnlyList<double>[] { new[] { 10.0, 100.0, 1000.0 } } },new OptimizationOptions{Seed=11223344UL},cancellationToken: TestContext.Current.CancellationToken);
   Assert.True(result.Evaluations>0);Assert.True(double.IsFinite(result.Best.Objective));Assert.True(result.Best.Constraints.TotalViolation>=0.0);
   Assert.IsType<HomaifarPenaltyGaOptimizer>(MetaheuristicFactory.Create<HomaifarPenaltyGaOptimizer>(MetaheuristicAlgorithmIds.HomaifarPenaltyGa));
 }
 private static ContinuousConstrainedOptimizationProblem CreateProblem()=>new(BoundedContinuousSearchSpace.Uniform(2,0.0,1.0),OptimizationSense.Minimize,1,0,static x=>x[0]*x[0]+x[1]*x[1],static(ReadOnlySpan<double> x,Span<double> inequalities,Span<double> equalities)=>{inequalities[0]=1.0-x[0]-x[1];});
}
