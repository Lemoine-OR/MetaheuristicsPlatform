using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.MultiVerseOptimizer;

public sealed class MultiVerseOptimizer : IMetaheuristic<double[], MultiVerseOptimizerParameters>
{
    public MetaheuristicDescriptor Descriptor { get; }=new(){Id=MetaheuristicAlgorithmIds.MultiVerseOptimizer,Name="Multi-Verse Optimizer",Acronym="MVO",SolutionModel=MetaheuristicSolutionModel.Population,Families=MetaheuristicFamily.Other,Mechanisms=MetaheuristicMechanism.Adaptive,SearchSpaces=SearchSpaceKind.Continuous,IsStochastic=true,References=[MultiVerseOptimizerReferences.MirjaliliMirjaliliHatamlou2016]};
    public MultiVerseOptimizerParameters CreateDefaultParameters()=>new();
    public OptimizationResult<double[]> Optimize(IOptimizationProblem<double[]> problem,MultiVerseOptimizerParameters parameters,ISolutionCloner<double[]> solutionCloner,IStoppingCriterion stoppingCriterion,OptimizationOptions? options=null,IOptimizationCallback<double[]>? callback=null,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);ArgumentNullException.ThrowIfNull(solutionCloner);ArgumentNullException.ThrowIfNull(stoppingCriterion);parameters.Validate();
        if(problem.Sense != OptimizationSense.Minimize)throw new NotSupportedException("Canonical MVO preserves the original minimization-oriented inflation-rate roulette and requires minimization.");
        if(problem is not ISpanContinuousOptimizationProblem continuousProblem)throw new NotSupportedException("MVO requires ISpanContinuousOptimizationProblem.");
        IBoundedContinuousSearchSpace searchSpace=continuousProblem.SearchSpace;int d=searchSpace.Dimension;if(d<=0)throw new InvalidOperationException("MVO requires a positive dimension.");
        int n=parameters.PopulationSize;double[][] u=CreatePopulation(n,d);double[] f=new double[n];var context=new OptimizationContext<double[]>(Descriptor,problem,solutionCloner,stoppingCriterion,options,callback,cancellationToken);var state=new MultiVerseOptimizerState(0,MultiVerseOptimizerPhase.Initialization,parameters.WormholeExistenceProbabilityMinimum,1.0,null);context.Start(state);
        for(int i=0;i<n;i++){searchSpace.Sample(context.Random,u[i]);f[i]=context.Evaluate(u[i],state);RequireNonNegative(f[i]);var stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);}
        double[] best=new double[d];
        for(int iteration=1;iteration<=parameters.MaximumIterations;iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();int[] order=Enumerable.Range(0,n).OrderBy(i=>f[i]).ToArray();Array.Copy(u[order[0]],best,d);double bestFitness=f[order[0]];
            double norm=Math.Sqrt(order.Sum(i=>f[i]*f[i]));double[] ni=new double[n];if(norm>0.0)for(int rank=0;rank<n;rank++)ni[rank]=f[order[rank]]/norm;
            double wep=parameters.WormholeExistenceProbabilityMinimum+(double)iteration/parameters.MaximumIterations*(parameters.WormholeExistenceProbabilityMaximum-parameters.WormholeExistenceProbabilityMinimum);double tdr=1.0-Math.Pow((double)iteration/parameters.MaximumIterations,1.0/parameters.ExploitationAccuracy);
            double[][] next=CreatePopulation(n,d);Array.Copy(best,next[0],d);ReadOnlySpan<double> lo=searchSpace.LowerBounds,hi=searchSpace.UpperBounds;
            for(int rank=1;rank<n;rank++)
            {
                int sourceIndex=order[rank];Array.Copy(u[sourceIndex],next[rank],d);
                for(int j=0;j<d;j++)
                {
                    if(context.Random.NextDouble()<ni[rank]){int donorRank=OriginalRoulette(ni,context.Random);next[rank][j]=u[order[donorRank]][j];}
                    if(context.Random.NextDouble()<wep){double step=tdr*((hi[j]-lo[j])*context.Random.NextDouble()+lo[j]);next[rank][j]=context.Random.NextDouble()<0.5?best[j]+step:best[j]-step;}
                }
                searchSpace.Clamp(next[rank]);
            }
            state=new MultiVerseOptimizerState(iteration-1,MultiVerseOptimizerPhase.WhiteHoleAndWormhole,wep,tdr,bestFitness);
            for(int rank=0;rank<n;rank++){u[rank]=next[rank];f[rank]=context.Evaluate(u[rank],state);RequireNonNegative(f[rank]);var stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);}
            int bi=BestIndex(f);state=new MultiVerseOptimizerState(iteration,MultiVerseOptimizerPhase.CompletedIteration,wep,tdr,f[bi]);context.CompleteIteration(state.BestInflationRate,state);var itStop=context.EvaluateStopping(state);if(itStop.ShouldStop)return context.Complete(itStop,state);
        }
        return context.Complete(StoppingDecision.Stop("MaximumMVOIterations","The configured MVO iteration limit was reached."),state);
    }
    private static int OriginalRoulette(double[] normalizedRates,IRandomSource random){double total=0;for(int i=0;i<normalizedRates.Length;i++)total-=normalizedRates[i];if(total==0.0)return 0;double p=random.NextDouble()*total,c=0;for(int i=0;i<normalizedRates.Length;i++){c-=normalizedRates[i];if(c>p)return i;}return 0;}
    private static int BestIndex(ReadOnlySpan<double> f){int b=0;for(int i=1;i<f.Length;i++)if(f[i]<f[b])b=i;return b;}
    private static double[][] CreatePopulation(int n,int d){var x=new double[n][];for(int i=0;i<n;i++)x[i]=new double[d];return x;}
    private static void RequireNonNegative(double v){if(!double.IsFinite(v)||v<0.0)throw new InvalidOperationException("Canonical MVO requires finite non-negative inflation rates.");}
}
