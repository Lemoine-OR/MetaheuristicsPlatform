using MetaheuristicsPlatform.Algorithms.SA;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Tests;

public sealed class SimulatedAnnealingScientificCoolingScheduleTests
{
    [Fact]
    public void LinearScheduleUsesFiniteAdditiveDecrement()
    {
        var schedule =
            new LinearCoolingSchedule(
                decrement: 2.5);

        SimulatedAnnealingCoolingContext context =
            CreateContext(
                completedLevels: 1,
                currentTemperature: 10.0);

        Assert.Equal(
            7.5,
            schedule.GetNextTemperature(
                in context),
            precision: 12);

        context =
            CreateContext(
                completedLevels: 2,
                currentTemperature: 1.0);

        Assert.Equal(
            0.0,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void HajekScheduleUsesNormalizedLogarithmicLaw()
    {
        var schedule =
            new HajekLogarithmicCoolingSchedule();

        SimulatedAnnealingCoolingContext context =
            CreateContext(
                completedLevels: 1,
                currentTemperature: 10.0);

        double expected =
            10.0 *
            Math.Log(2.0) /
            Math.Log(3.0);

        Assert.Equal(
            expected,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void SzuHartleyScheduleIsInverseLinear()
    {
        var schedule =
            new SzuHartleyFastCauchyCoolingSchedule();

        SimulatedAnnealingCoolingContext context =
            CreateContext(
                completedLevels: 1,
                currentTemperature: 10.0);

        Assert.Equal(
            5.0,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void IngberScheduleUsesDimensionDependentExponent()
    {
        var schedule =
            new IngberVeryFastCoolingSchedule(
                dimension: 2,
                c: 0.5);

        SimulatedAnnealingCoolingContext context =
            CreateContext(
                completedLevels: 4,
                currentTemperature: 10.0);

        Assert.Equal(
            10.0 *
            Math.Exp(-1.0),
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void TsallisQTwoRecoversFastCauchyCooling()
    {
        var generalized =
            new TsallisStarioloGeneralizedCoolingSchedule(
                visitingQ: 2.0);

        var fast =
            new SzuHartleyFastCauchyCoolingSchedule();

        SimulatedAnnealingCoolingContext context =
            CreateContext(
                completedLevels: 5,
                currentTemperature: 3.0);

        Assert.Equal(
            fast.GetNextTemperature(
                in context),
            generalized.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void TsallisQOneRecoversNormalizedLogarithmicCooling()
    {
        var generalized =
            new TsallisStarioloGeneralizedCoolingSchedule(
                visitingQ: 1.0);

        var logarithmic =
            new HajekLogarithmicCoolingSchedule();

        SimulatedAnnealingCoolingContext context =
            CreateContext(
                completedLevels: 7,
                currentTemperature: 2.0);

        Assert.Equal(
            logarithmic.GetNextTemperature(
                in context),
            generalized.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void AartsVanLaarhovenUsesLevelStandardDeviation()
    {
        var schedule =
            new AartsVanLaarhovenStatisticalCoolingSchedule(
                delta: 0.1);

        SimulatedAnnealingCoolingContext context =
            CreateStatisticalContext(
                currentTemperature: 10.0,
                variance: 4.0);

        double expected =
            10.0 /
            (1.0 +
             10.0 *
             Math.Log(1.1) /
             6.0);

        Assert.Equal(
            expected,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void HuangUsesPublishedExponentialStatisticalDecrement()
    {
        var schedule =
            new HuangStatisticalCoolingSchedule(
                lambda: 0.6);

        SimulatedAnnealingCoolingContext context =
            CreateStatisticalContext(
                currentTemperature: 10.0,
                variance: 4.0);

        Assert.Equal(
            10.0 *
            Math.Exp(-3.0),
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void TrikiUsesVarianceDrivenDecrement()
    {
        var schedule =
            new TrikiAdaptiveCoolingSchedule(
                targetMeanObjectiveDecrease: 0.02);

        SimulatedAnnealingCoolingContext context =
            CreateStatisticalContext(
                currentTemperature: 10.0,
                variance: 4.0);

        Assert.Equal(
            9.5,
            schedule.GetNextTemperature(
                in context),
            precision: 12);
    }

    [Fact]
    public void StatisticalSchedulesFreezeWhenVarianceIsZero()
    {
        SimulatedAnnealingCoolingContext context =
            CreateStatisticalContext(
                currentTemperature: 10.0,
                variance: 0.0);

        Assert.Equal(
            0.0,
            new AartsVanLaarhovenStatisticalCoolingSchedule()
                .GetNextTemperature(
                    in context));

        Assert.Equal(
            0.0,
            new HuangStatisticalCoolingSchedule()
                .GetNextTemperature(
                    in context));

        Assert.Equal(
            0.0,
            new TrikiAdaptiveCoolingSchedule(0.01)
                .GetNextTemperature(
                    in context));
    }

    [Fact]
    public void RuntimeCatalogContainsAllTenBuiltInCoolingLaws()
    {
        IReadOnlyList<
            SimulatedAnnealingCoolingScheduleDescriptor> catalog =
            SimulatedAnnealingCoolingScheduleCatalog.All;

        Assert.Equal(
            10,
            catalog.Count);

        Assert.Equal(
            10,
            catalog.Select(
                    entry => entry.Id)
                .Distinct(
                    StringComparer.Ordinal)
                .Count());

        Assert.Equal(
            10,
            catalog.Select(
                    entry => entry.Kind)
                .Distinct()
                .Count());
    }

    [Fact]
    public void NewBuiltInSchedulesExposeCanonicalCatalogIds()
    {
        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.Linear,
            new LinearCoolingSchedule(0.1).Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.Hajek1988,
            new HajekLogarithmicCoolingSchedule().Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.SzuHartley1987,
            new SzuHartleyFastCauchyCoolingSchedule().Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.Ingber1989,
            new IngberVeryFastCoolingSchedule(1, 1.0).Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.TsallisStariolo1996,
            new TsallisStarioloGeneralizedCoolingSchedule(2.0).Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.AartsVanLaarhoven1985,
            new AartsVanLaarhovenStatisticalCoolingSchedule().Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.HuangEtAl1986,
            new HuangStatisticalCoolingSchedule().Id);

        Assert.Equal(
            SimulatedAnnealingCoolingScheduleIds.TrikiEtAl2005,
            new TrikiAdaptiveCoolingSchedule(0.01).Id);
    }

    [Fact]
    public void HistoricalScheduleInstanceIdsRemainBackwardCompatible()
    {
        Assert.Equal(
            "geometric",
            new GeometricCoolingSchedule().Id);

        Assert.Equal(
            "lundy-mees-1986",
            new LundyMeesCoolingSchedule(0.01).Id);
    }

    [Fact]
    public void ExistingCoolingEnumNumericValuesRemainStable()
    {
        Assert.Equal(
            0,
            (int)SimulatedAnnealingCoolingScheduleKind.Geometric);

        Assert.Equal(
            1,
            (int)SimulatedAnnealingCoolingScheduleKind.LundyMees);
    }

    [Fact]
    public void StatisticalCustomScheduleReceivesCompletedLevelStatistics()
    {
        var schedule =
            new CapturingStatisticalSchedule();

        var optimizer =
            new SimulatedAnnealingOptimizer<
                IntSolution,
                AddMove,
                int>(
                new DelegateSimulatedAnnealingInitialSolutionGenerator<IntSolution>(
                    static (_, _) =>
                        new IntSolution(4)),
                new TowardZeroNeighborhood(),
                new AddMoveOperator(),
                new QuadraticDeltaEvaluator());

        _ =
            optimizer.Optimize(
                new QuadraticProblem(),
                new SimulatedAnnealingParameters
                {
                    InitialTemperature = 10.0,
                    MinimumTemperature = 1e-9,
                    TransitionsPerTemperatureLevel = 2,
                    StopAtMinimumTemperature = false,
                    CustomCoolingSchedule = schedule
                },
                new ImmutableSolutionCloner<IntSolution>(),
                new MaxIterationsStoppingCriterion(2),
                new OptimizationOptions
                {
                    Seed = 20260815UL
                },
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            1,
            schedule.CallCount);

        Assert.Equal(
            2,
            schedule.LastContext.LevelAttemptedTransitions);

        Assert.Equal(
            2,
            schedule.LastContext.LevelAcceptedTransitions);

        Assert.Equal(
            2,
            schedule.LastContext.LevelObjectiveSamples);

        Assert.True(
            double.IsFinite(
                schedule.LastContext.LevelObjectiveVariance));
    }

    [Fact]
    public void StatisticalBuiltInRequiresAtLeastTwoTransitionsPerLevel()
    {
        var parameters =
            new SimulatedAnnealingParameters
            {
                InitialTemperature = 10.0,
                MinimumTemperature = 1e-9,
                TransitionsPerTemperatureLevel = 1,
                CoolingSchedule =
                    SimulatedAnnealingCoolingScheduleKind.AartsVanLaarhovenStatistical
            };

        Assert.Throws<ArgumentOutOfRangeException>(
            parameters.Validate);
    }

    [Fact]
    public void RuntimeCatalogResolvesStableIdAndKind()
    {
        SimulatedAnnealingCoolingScheduleDescriptor byId =
            SimulatedAnnealingCoolingScheduleCatalog.Get(
                SimulatedAnnealingCoolingScheduleIds.Hajek1988);

        SimulatedAnnealingCoolingScheduleDescriptor byKind =
            SimulatedAnnealingCoolingScheduleCatalog.Get(
                SimulatedAnnealingCoolingScheduleKind.HajekLogarithmic);

        Assert.Same(
            byId,
            byKind);
    }

    [Fact]
    public void InvalidScientificParametersAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new LinearCoolingSchedule(
                    decrement: 0.0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new IngberVeryFastCoolingSchedule(
                    dimension: 0,
                    c: 1.0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new TsallisStarioloGeneralizedCoolingSchedule(
                    visitingQ: 3.0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HuangStatisticalCoolingSchedule(
                    lambda: 0.0));
    }

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
            ArgumentNullException.ThrowIfNull(
                random);

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

    private sealed class CapturingStatisticalSchedule :
        ISimulatedAnnealingStatisticalCoolingSchedule
    {
        public string Id =>
            "test.statistical.capture";

        public int CallCount { get; private set; }

        public SimulatedAnnealingCoolingContext LastContext { get; private set; }

        public double GetNextTemperature(
            in SimulatedAnnealingCoolingContext context)
        {
            CallCount++;
            LastContext = context;

            return
                context.CurrentTemperature *
                0.5;
        }
    }

    private static SimulatedAnnealingCoolingContext
        CreateContext(
            long completedLevels,
            double currentTemperature) =>
        new(
            CompletedTemperatureLevels:
                completedLevels,
            AttemptedTransitions:
                100,
            AcceptedTransitions:
                50,
            InitialTemperature:
                10.0,
            CurrentTemperature:
                currentTemperature);

    private static SimulatedAnnealingCoolingContext
        CreateStatisticalContext(
            double currentTemperature,
            double variance) =>
        new SimulatedAnnealingCoolingContext(
            CompletedTemperatureLevels:
                1,
            AttemptedTransitions:
                100,
            AcceptedTransitions:
                50,
            InitialTemperature:
                10.0,
            CurrentTemperature:
                currentTemperature)
        {
            LevelAttemptedTransitions =
                100,
            LevelAcceptedTransitions =
                50,
            LevelObjectiveSamples =
                100,
            LevelObjectiveMean =
                42.0,
            LevelObjectiveVariance =
                variance
        };
}
