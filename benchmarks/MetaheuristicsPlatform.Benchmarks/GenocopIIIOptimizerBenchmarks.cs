using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Constraints.GenocopIII;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
namespace MetaheuristicsPlatform.Benchmarks;
[MemoryDiagnoser] public class GenocopIIIOptimizerBenchmarks
{
 private readonly ContinuousConstrainedOptimizationProblem _problem=new(BoundedContinuousSearchSpace.Uniform(4,0.0,1.0),OptimizationSense.Minimize,1,0,static x=>x[0]*x[0]+x[1]*x[1]+x[2]*x[2]+x[3]*x[3],static(ReadOnlySpan<double> x,Span<double> inequalities,Span<double> equalities)=>{inequalities[0]=1.0-x[0]-x[1];});
 [Benchmark] public double Optimize()=>new GenocopIIIOptimizer().Optimize(_problem,new GenocopIIIParameters { MaximumGenerations = 2, PopulationSize = 20, ReferencePopulationSize = 5 },new OptimizationOptions{Seed=123456UL}).Best.Objective;
}
