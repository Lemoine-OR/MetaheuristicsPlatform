using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;

public sealed class ContinuousOptimizationProblemTests
{
    [Fact]
    public void Evaluate_UsesSpanObjective()
    {
        var problem = new ContinuousOptimizationProblem(
            BoundedContinuousSearchSpace.Uniform(3, -10.0, 10.0),
            OptimizationSense.Minimize,
            static position =>
            {
                double sum = 0.0;
                for (int i = 0; i < position.Length; i++)
                {
                    sum += position[i] * position[i];
                }

                return sum;
            });

        double value = problem.Evaluate(new[] { 1.0, 2.0, 3.0 });

        Assert.Equal(14.0, value);
    }
}