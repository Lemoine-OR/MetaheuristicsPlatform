using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class OptimizationContextExternalEvaluationTests
{
    [Fact]
    public void NonImprovingExternalEvaluation_DoesNotNeedCandidate()
    {
        var context =
            CreateContext();

        context.Start();

        context.RegisterExternalEvaluation(
            new[] { 1.0 },
            1.0);

        context.RegisterExternalEvaluation(
            2.0);

        Assert.Equal(
            2,
            context.State.Evaluations);

        Assert.Equal(
            1.0,
            context.State.BestFitness);
    }

    [Fact]
    public void ImprovingExternalEvaluation_WithoutCandidateIsRejected()
    {
        var context =
            CreateContext();

        context.Start();

        Assert.Throws<InvalidOperationException>(
            () =>
                context.RegisterExternalEvaluation(
                    1.0));
    }

    private static OptimizationContext<double[]>
        CreateContext()
    {
        var descriptor =
            new MetaheuristicDescriptor
            {
                Id = "test",
                Name = "Test",
                SolutionModel =
                    MetaheuristicSolutionModel.Population
            };

        return new OptimizationContext<double[]>(
            descriptor,
            new ArrayProblem(),
            new ArraySolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(1));
    }

    private sealed class ArrayProblem :
        IOptimizationProblem<double[]>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(double[] solution) =>
            solution[0];
    }
}