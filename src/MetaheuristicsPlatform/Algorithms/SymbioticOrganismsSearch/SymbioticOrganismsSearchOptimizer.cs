using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.SymbioticOrganismsSearch;

public sealed class SymbioticOrganismsSearchOptimizer : IMetaheuristic<double[], SymbioticOrganismsSearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id=MetaheuristicAlgorithmIds.SymbioticOrganismsSearch, Name="Symbiotic Organisms Search", Acronym="SOS",
        SolutionModel=MetaheuristicSolutionModel.Population, Families=MetaheuristicFamily.SwarmIntelligence,
        Mechanisms=MetaheuristicMechanism.Swarm, SearchSpaces=SearchSpaceKind.Continuous, IsStochastic=true,
        References=[SymbioticOrganismsSearchReferences.ChengPrayogo2014]
    };
    public SymbioticOrganismsSearchParameters CreateDefaultParameters()=>new();
    public OptimizationResult<double[]> Optimize(IOptimizationProblem<double[]> problem, SymbioticOrganismsSearchParameters parameters, ISolutionCloner<double[]> solutionCloner, IStoppingCriterion stoppingCriterion, OptimizationOptions? options=null, IOptimizationCallback<double[]>? callback=null, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);ArgumentNullException.ThrowIfNull(solutionCloner);ArgumentNullException.ThrowIfNull(stoppingCriterion);parameters.Validate();
        if(problem is not ISpanContinuousOptimizationProblem continuousProblem)throw new NotSupportedException("SOS requires ISpanContinuousOptimizationProblem.");
        IBoundedContinuousSearchSpace searchSpace=continuousProblem.SearchSpace; int d=searchSpace.Dimension;if(d<=0)throw new InvalidOperationException("SOS requires a positive dimension.");
        int n=parameters.PopulationSize; double[][] x=CreatePopulation(n,d); double[] f=new double[n]; double[] best=new double[d], oldI=new double[d], oldJ=new double[d], ci=new double[d], cj=new double[d], parasite=new double[d];
        var context=new OptimizationContext<double[]>(Descriptor,problem,solutionCloner,stoppingCriterion,options,callback,cancellationToken); var state=new SymbioticOrganismsSearchState(0,SymbioticOrganismsSearchPhase.Initialization,-1,null);context.Start(state);
        for(int i=0;i<n;i++){searchSpace.Sample(context.Random,x[i]);f[i]=context.Evaluate(x[i],state);RequireFinite(f[i]);var stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);}
        for(int iteration=1;iteration<=parameters.MaximumIterations;iteration++)
        {
            for(int i=0;i<n;i++)
            {
                cancellationToken.ThrowIfCancellationRequested(); int bi=BestIndex(f,problem.Sense);Array.Copy(x[bi],best,d);
                int j=OtherIndex(i,n,context.Random.NextInt32(n-1));Array.Copy(x[i],oldI,d);Array.Copy(x[j],oldJ,d);int bf1=1+context.Random.NextInt32(2),bf2=1+context.Random.NextInt32(2);
                for(int k=0;k<d;k++){double mv=0.5*(oldI[k]+oldJ[k]);ci[k]=oldI[k]+context.Random.NextDouble()*(best[k]-mv*bf1);cj[k]=oldJ[k]+context.Random.NextDouble()*(best[k]-mv*bf2);} searchSpace.Clamp(ci);searchSpace.Clamp(cj);
                state=new SymbioticOrganismsSearchState(iteration-1,SymbioticOrganismsSearchPhase.Mutualism,i,f[bi]); double fi=context.Evaluate(ci,state),fj=context.Evaluate(cj,state);RequireFinite(fi);RequireFinite(fj);if(problem.Sense.IsBetter(fi,f[i])){Array.Copy(ci,x[i],d);f[i]=fi;}if(problem.Sense.IsBetter(fj,f[j])){Array.Copy(cj,x[j],d);f[j]=fj;} var stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);
                bi=BestIndex(f,problem.Sense);Array.Copy(x[bi],best,d);j=OtherIndex(i,n,context.Random.NextInt32(n-1));for(int k=0;k<d;k++)ci[k]=x[i][k]+(2.0*context.Random.NextDouble()-1.0)*(best[k]-x[j][k]);searchSpace.Clamp(ci);state=new SymbioticOrganismsSearchState(iteration-1,SymbioticOrganismsSearchPhase.Commensalism,i,f[bi]);fi=context.Evaluate(ci,state);RequireFinite(fi);if(problem.Sense.IsBetter(fi,f[i])){Array.Copy(ci,x[i],d);f[i]=fi;}stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);
                j=OtherIndex(i,n,context.Random.NextInt32(n-1));Array.Copy(x[i],parasite,d);bool changed=false;ReadOnlySpan<double> lo=searchSpace.LowerBounds,hi=searchSpace.UpperBounds;for(int k=0;k<d;k++){if(context.Random.NextDouble()<0.5){parasite[k]=lo[k]+context.Random.NextDouble()*(hi[k]-lo[k]);changed=true;}}if(!changed){int k=context.Random.NextInt32(d);parasite[k]=lo[k]+context.Random.NextDouble()*(hi[k]-lo[k]);}state=new SymbioticOrganismsSearchState(iteration-1,SymbioticOrganismsSearchPhase.Parasitism,i,f[BestIndex(f,problem.Sense)]);double fp=context.Evaluate(parasite,state);RequireFinite(fp);if(problem.Sense.IsBetter(fp,f[j])){Array.Copy(parasite,x[j],d);f[j]=fp;}stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);
            }
            int bestIndex=BestIndex(f,problem.Sense);state=new SymbioticOrganismsSearchState(iteration,SymbioticOrganismsSearchPhase.CompletedIteration,-1,f[bestIndex]);context.CompleteIteration(state.BestFitness,state);var iterStop=context.EvaluateStopping(state);if(iterStop.ShouldStop)return context.Complete(iterStop,state);
        }
        return context.Complete(StoppingDecision.Stop("MaximumSOSIterations","The configured SOS iteration limit was reached."),state);
    }
    private static int OtherIndex(int i,int n,int draw)=>draw>=i?draw+1:draw;
    private static int BestIndex(ReadOnlySpan<double> f,OptimizationSense sense){int b=0;for(int i=1;i<f.Length;i++)if(sense.IsBetter(f[i],f[b]))b=i;return b;}
    private static double[][] CreatePopulation(int n,int d){var x=new double[n][];for(int i=0;i<n;i++)x[i]=new double[d];return x;}
    private static void RequireFinite(double v){if(!double.IsFinite(v))throw new InvalidOperationException("SOS requires finite objective values.");}
}
