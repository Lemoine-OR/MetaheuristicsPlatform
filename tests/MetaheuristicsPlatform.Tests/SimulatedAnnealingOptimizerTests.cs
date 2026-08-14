using MetaheuristicsPlatform.Algorithms.SA;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class SimulatedAnnealingOptimizerTests
{
    [Fact]
    public void GenericValueTypeSaFindsZeroOnSimpleQuadratic()
    {
        var optimizer =
            CreateOptimizer();

        OptimizationResult<IntSolution> result =
            optimizer.Optimize(
                new QuadraticProblem(),
                new SimulatedAnnealingParameters
                {
                    InitialTemperature = 10.0,
                    MinimumTemperature = 1e-12,
                    TransitionsPerTemperatureLevel = 100,
                    StopAtMinimumTemperature = false
                },
                new ImmutableSolutionCloner<IntSolution>(),
                new MaxIterationsStoppingCriterion(20),
                new OptimizationOptions
                {
                    Seed = 20260814UL
                },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            0.0,
            result.BestFitness);

        Assert.Equal(
            0,
            result.BestSolution.Value);
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<IntSolution> first =
            Run(123456UL);

        OptimizationResult<IntSolution> second =
            Run(123456UL);

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);

        Assert.Equal(
            first.Statistics.Iterations,
            second.Statistics.Iterations);

        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
    }

    [Fact]
    public void DescriptorIsGenericSingleSolutionTrajectoryAlgorithm()
    {
        var descriptor =
            CreateOptimizer()
                .Descriptor;

        Assert.Equal(
            "SA",
            descriptor.Acronym);

        Assert.Equal(
            MetaheuristicsPlatform.Classification.MetaheuristicSolutionModel.SingleSolution,
            descriptor.SolutionModel);

        Assert.True(
            descriptor.Families.HasFlag(
                MetaheuristicsPlatform.Classification.MetaheuristicFamily.TrajectoryBased));

        Assert.True(
            descriptor.Mechanisms.HasFlag(
                MetaheuristicsPlatform.Classification.MetaheuristicMechanism.Neighborhood));

        Assert.Contains(
            descriptor.References,
            reference =>
                reference.Doi ==
                "10.1126/science.220.4598.671");
    }

    private static OptimizationResult<IntSolution>
        Run(ulong seed) =>
        CreateOptimizer()
            .Optimize(
                new QuadraticProblem(),
                new SimulatedAnnealingParameters
                {
                    InitialTemperature = 5.0,
                    MinimumTemperature = 1e-12,
                    TransitionsPerTemperatureLevel = 10,
                    StopAtMinimumTemperature = false
                },
                new ImmutableSolutionCloner<IntSolution>(),
                new MaxIterationsStoppingCriterion(30),
                new OptimizationOptions
                {
                    Seed = seed
                },
                cancellationToken:
                    TestContext.Current.CancellationToken);

    private static SimulatedAnnealingOptimizer<
        IntSolution,
        AddMove,
        int> CreateOptimizer() =>
        new(
            new DelegateSimulatedAnnealingInitialSolutionGenerator<IntSolution>(
                static (_, _) =>
                    new IntSolution(10)),
            new TowardZeroNeighborhood(),
            new AddMoveOperator(),
            new QuadraticDeltaEvaluator());

    private readonly record struct IntSolution(
        int Value);

    private readonly record struct AddMove(
        int Delta);

    private sealed class QuadraticProblem :
        IOptimizationProblem<IntSolution>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            IntSolution solution) =>
            (double)solution.Value *
            solution.Value;
    }

    private sealed class TowardZeroNeighborhood :
        IStochasticNeighborhood<
            IntSolution,
            AddMove>
    {
        public bool TrySampleMove(
            in IntSolution solution,
            IRandomSource random,
            out AddMove move)
        {
            ArgumentNullException.ThrowIfNull(random);

            move =
                solution.Value > 0
                    ? new AddMove(-1)
                    : new AddMove(+1);

            return true;
        }
    }

    private sealed class AddMoveOperator :
        IReversibleMoveOperator<
            IntSolution,
            AddMove,
            int>
    {
        public int CaptureUndo(
            in IntSolution solution,
            in AddMove move) =>
            solution.Value;

        public void Apply(
            ref IntSolution solution,
            in AddMove move)
        {
            solution =
                new IntSolution(
                    solution.Value +
                    move.Delta);
        }

        public void Undo(
            ref IntSolution solution,
            in AddMove move,
            in int undo)
        {
            solution =
                new IntSolution(
                    undo);
        }
    }

    private sealed class QuadraticDeltaEvaluator :
        IMoveObjectiveDeltaEvaluator<
            IntSolution,
            AddMove>
    {
        public bool TryEvaluateCandidateObjective(
            in IntSolution solution,
            double currentObjective,
            in AddMove move,
            out double candidateObjective)
        {
            int value =
                solution.Value +
                move.Delta;

            candidateObjective =
                (double)value *
                value;

            return true;
        }
    }
}