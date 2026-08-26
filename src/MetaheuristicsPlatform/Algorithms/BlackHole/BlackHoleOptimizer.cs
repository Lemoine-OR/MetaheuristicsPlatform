using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.BlackHole;

public sealed class BlackHoleOptimizer : IMetaheuristic<double[], BlackHoleParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.BlackHoleAlgorithm, Name = "Black Hole Algorithm", Acronym = "BH",
        SolutionModel = MetaheuristicSolutionModel.Population, Families = MetaheuristicFamily.Other,
        Mechanisms = MetaheuristicMechanism.Adaptive, SearchSpaces = SearchSpaceKind.Continuous, IsStochastic = true,
        References = [BlackHoleReferences.Hatamlou2013]
    };
    public BlackHoleParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(IOptimizationProblem<double[]> problem, BlackHoleParameters parameters, ISolutionCloner<double[]> solutionCloner, IStoppingCriterion stoppingCriterion, OptimizationOptions? options=null, IOptimizationCallback<double[]>? callback=null, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(problem); ArgumentNullException.ThrowIfNull(parameters); ArgumentNullException.ThrowIfNull(solutionCloner); ArgumentNullException.ThrowIfNull(stoppingCriterion); parameters.Validate();
        if (problem.Sense != OptimizationSense.Minimize) throw new NotSupportedException("Canonical Black Hole Algorithm requires minimization because the published event-horizon radius uses raw objective values.");
        if (problem is not ISpanContinuousOptimizationProblem continuousProblem) throw new NotSupportedException("Black Hole Algorithm requires ISpanContinuousOptimizationProblem.");
        IBoundedContinuousSearchSpace searchSpace=continuousProblem.SearchSpace; int dimension=searchSpace.Dimension; if(dimension<=0) throw new InvalidOperationException("Black Hole Algorithm requires a positive dimension.");
        int n=parameters.PopulationSize; double[][] stars=CreatePopulation(n,dimension); double[] f=new double[n];
        var context=new OptimizationContext<double[]>(Descriptor,problem,solutionCloner,stoppingCriterion,options,callback,cancellationToken);
        var state=new BlackHoleState(0,BlackHolePhase.Initialization,n,null,null); context.Start(state);
        for(int i=0;i<n;i++){ searchSpace.Sample(context.Random,stars[i]); f[i]=context.Evaluate(stars[i],state); RequirePositive(f[i]); var stop=context.EvaluateStopping(state); if(stop.ShouldStop) return context.Complete(stop,state); }
        int bh=BestIndex(f);
        for(int iteration=1;iteration<=parameters.MaximumIterations;iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested(); state=new BlackHoleState(iteration-1,BlackHolePhase.Attraction,n,f[bh],null);
            for(int i=0;i<n;i++)
            {
                if(i==bh) continue; double r=context.Random.NextDouble();
                for(int d=0;d<dimension;d++) stars[i][d]=stars[i][d]+r*(stars[bh][d]-stars[i][d]);
                searchSpace.Clamp(stars[i]); f[i]=context.Evaluate(stars[i],state); RequirePositive(f[i]);
                if(f[i]<f[bh]) Swap(stars,f,i,bh);
                var stop=context.EvaluateStopping(state); if(stop.ShouldStop) return context.Complete(stop,state);
            }
            double sum=0.0; for(int i=0;i<n;i++) sum+=f[i]; if(!(sum>0.0)||!double.IsFinite(sum)) throw new InvalidOperationException("Black Hole event-horizon denominator must be finite and positive.");
            double radius=Math.Abs(f[bh]/sum); state=new BlackHoleState(iteration-1,BlackHolePhase.EventHorizon,n,f[bh],radius);
            for(int i=0;i<n;i++)
            {
                if(i==bh) continue; if(Distance(stars[i],stars[bh])>=radius) continue;
                searchSpace.Sample(context.Random,stars[i]); f[i]=context.Evaluate(stars[i],state); RequirePositive(f[i]); if(f[i]<f[bh]) Swap(stars,f,i,bh);
                var stop=context.EvaluateStopping(state); if(stop.ShouldStop) return context.Complete(stop,state);
            }
            state=new BlackHoleState(iteration,BlackHolePhase.CompletedIteration,n,f[bh],radius); context.CompleteIteration(state.BlackHoleFitness,state); var itStop=context.EvaluateStopping(state); if(itStop.ShouldStop) return context.Complete(itStop,state);
        }
        return context.Complete(StoppingDecision.Stop("MaximumBlackHoleIterations","The configured Black Hole iteration limit was reached."),state);
    }
    private static double Distance(double[] a,double[] b){double s=0;for(int d=0;d<a.Length;d++){double q=a[d]-b[d];s+=q*q;}return Math.Sqrt(s);}
    private static int BestIndex(ReadOnlySpan<double> f){int b=0;for(int i=1;i<f.Length;i++)if(f[i]<f[b])b=i;return b;}
    private static void Swap(double[][] x,double[] f,int a,int b){(x[a],x[b])=(x[b],x[a]);(f[a],f[b])=(f[b],f[a]);}
    private static double[][] CreatePopulation(int n,int d){var x=new double[n][];for(int i=0;i<n;i++)x[i]=new double[d];return x;}
    private static void RequirePositive(double value){if(!double.IsFinite(value)||value<=0.0)throw new InvalidOperationException("Canonical Black Hole Algorithm requires strictly positive finite objective values.");}
}
