using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.EquilibriumOptimizer;

public sealed class EquilibriumOptimizer : IMetaheuristic<double[], EquilibriumOptimizerParameters>
{
    public MetaheuristicDescriptor Descriptor { get; }=new(){Id=MetaheuristicAlgorithmIds.EquilibriumOptimizer,Name="Equilibrium Optimizer",Acronym="EO",SolutionModel=MetaheuristicSolutionModel.Population,Families=MetaheuristicFamily.Other,Mechanisms=MetaheuristicMechanism.Adaptive,SearchSpaces=SearchSpaceKind.Continuous,IsStochastic=true,References=[EquilibriumOptimizerReferences.FaramarziHeidarinejadStephensMirjalili2020]};
    public EquilibriumOptimizerParameters CreateDefaultParameters()=>new();
    public OptimizationResult<double[]> Optimize(IOptimizationProblem<double[]> problem,EquilibriumOptimizerParameters parameters,ISolutionCloner<double[]> solutionCloner,IStoppingCriterion stoppingCriterion,OptimizationOptions? options=null,IOptimizationCallback<double[]>? callback=null,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(problem);ArgumentNullException.ThrowIfNull(parameters);ArgumentNullException.ThrowIfNull(solutionCloner);ArgumentNullException.ThrowIfNull(stoppingCriterion);parameters.Validate();if(problem is not ISpanContinuousOptimizationProblem continuousProblem)throw new NotSupportedException("EO requires ISpanContinuousOptimizationProblem.");IBoundedContinuousSearchSpace searchSpace=continuousProblem.SearchSpace;int d=searchSpace.Dimension;if(d<=0)throw new InvalidOperationException("EO requires a positive dimension.");int n=parameters.PopulationSize;double[][] x=CreatePopulation(n,d);double[] f=new double[n];var context=new OptimizationContext<double[]>(Descriptor,problem,solutionCloner,stoppingCriterion,options,callback,cancellationToken);var state=new EquilibriumOptimizerState(0,EquilibriumOptimizerPhase.Initialization,null);context.Start(state);
        for(int i=0;i<n;i++){searchSpace.Sample(context.Random,x[i]);f[i]=context.Evaluate(x[i],state);RequireFinite(f[i]);var stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);}
        double[][] eq=CreatePopulation(4,d);double[] eqF=Enumerable.Repeat(problem.Sense==OptimizationSense.Minimize?double.PositiveInfinity:double.NegativeInfinity,4).ToArray();UpdatePool(x,f,eq,eqF,problem.Sense);double[] avg=new double[d],candidate=new double[d],old=new double[d];
        for(int iteration=1;iteration<=parameters.MaximumIterations;iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();UpdatePool(x,f,eq,eqF,problem.Sense);for(int k=0;k<d;k++)avg[k]=0.25*(eq[0][k]+eq[1][k]+eq[2][k]+eq[3][k]);double tau=iteration-1.0;double time=Math.Pow(1.0-tau/parameters.MaximumIterations,parameters.ExploitationConstant*tau/parameters.MaximumIterations);state=new EquilibriumOptimizerState(iteration-1,EquilibriumOptimizerPhase.Search,Best(eqF,problem.Sense));
            for(int i=0;i<n;i++)
            {
                Array.Copy(x[i],old,d);double oldF=f[i];int pool=context.Random.NextInt32(5);double[] ceq=pool<4?eq[pool]:avg;double r1=context.Random.NextDouble(),r2=context.Random.NextDouble();double gcp=r2>=parameters.GenerationProbability?0.5*r1:0.0;
                for(int k=0;k<d;k++){double lambda=context.Random.NextDouble();if(lambda<=0.0)lambda=double.Epsilon;double r=context.Random.NextDouble();double F=parameters.ExplorationConstant*Math.Sign(r-0.5)*(Math.Exp(-lambda*time)-1.0);double g0=gcp*(ceq[k]-lambda*x[i][k]);double g=g0*F;candidate[k]=ceq[k]+(x[i][k]-ceq[k])*F+(g/lambda)*(1.0-F);}searchSpace.Clamp(candidate);double cf=context.Evaluate(candidate,state);RequireFinite(cf);if(problem.Sense.IsBetter(cf,oldF)){Array.Copy(candidate,x[i],d);f[i]=cf;}else{Array.Copy(old,x[i],d);f[i]=oldF;}var stop=context.EvaluateStopping(state);if(stop.ShouldStop)return context.Complete(stop,state);
            }
            UpdatePool(x,f,eq,eqF,problem.Sense);state=new EquilibriumOptimizerState(iteration,EquilibriumOptimizerPhase.CompletedIteration,Best(eqF,problem.Sense));context.CompleteIteration(state.BestFitness,state);var itStop=context.EvaluateStopping(state);if(itStop.ShouldStop)return context.Complete(itStop,state);
        }
        return context.Complete(StoppingDecision.Stop("MaximumEOIterations","The configured EO iteration limit was reached."),state);
    }
    private static void UpdatePool(double[][] x,double[] f,double[][] eq,double[] eqF,OptimizationSense sense)
    {
        for(int i=0;i<x.Length;i++)
        {
            int slot=-1;
            if(sense.IsBetter(f[i],eqF[0])) slot=0;
            else if(IsWorse(f[i],eqF[0],sense)&&sense.IsBetter(f[i],eqF[1])) slot=1;
            else if(IsWorse(f[i],eqF[0],sense)&&IsWorse(f[i],eqF[1],sense)&&sense.IsBetter(f[i],eqF[2])) slot=2;
            else if(IsWorse(f[i],eqF[0],sense)&&IsWorse(f[i],eqF[1],sense)&&IsWorse(f[i],eqF[2],sense)&&sense.IsBetter(f[i],eqF[3])) slot=3;
            if(slot<0) continue;
            for(int q=3;q>slot;q--){eqF[q]=eqF[q-1];Array.Copy(eq[q-1],eq[q],eq[q].Length);}
            eqF[slot]=f[i];Array.Copy(x[i],eq[slot],eq[slot].Length);
        }
    }
    private static bool IsWorse(double value,double reference,OptimizationSense sense)=>sense.IsBetter(reference,value);
    private static double Best(ReadOnlySpan<double> f,OptimizationSense sense){double b=f[0];for(int i=1;i<f.Length;i++)if(sense.IsBetter(f[i],b))b=f[i];return b;}
    private static double[][] CreatePopulation(int n,int d){var x=new double[n][];for(int i=0;i<n;i++)x[i]=new double[d];return x;}
    private static void RequireFinite(double v){if(!double.IsFinite(v))throw new InvalidOperationException("EO requires finite objective values.");}
}
