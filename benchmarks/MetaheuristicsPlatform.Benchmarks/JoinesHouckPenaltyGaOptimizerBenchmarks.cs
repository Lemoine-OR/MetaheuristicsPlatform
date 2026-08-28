using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.Constraints.JoinesHouckPenaltyGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
namespace MetaheuristicsPlatform.Benchmarks;
[MemoryDiagnoser] public class JoinesHouckPenaltyGaOptimizerBenchmarks
{
 private readonly ContinuousConstrainedOptimizationProblem _problem=new(BoundedContinuousSearchSpace.Uniform(4,0.0,1.0),OptimizationSense.Minimize,1,0,static x=>x[0]*x[0]+x[1]*x[1]+x[2]*x[2]+x[3]*x[3],static(ReadOnlySpan<double> x,Span<double> inequalities,Span<double> equalities)=>{inequalities[0]=1.0-x[0]-x[1];});
 [Benchmark] public double Optimize()=>new JoinesHouckPenaltyGaOptimizer().Optimize(_problem,new JoinesHouckPenaltyGaParameters { MaximumGenerations = 2 },new OptimizationOptions{Seed=123456UL}).Best.Objective;
}
