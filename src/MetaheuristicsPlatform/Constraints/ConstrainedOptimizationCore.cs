using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Constraints;

public delegate double ContinuousConstrainedObjective(ReadOnlySpan<double> solution);
public delegate void ContinuousConstraintEvaluator(ReadOnlySpan<double> solution, Span<double> inequalities, Span<double> equalities);

public interface IContinuousConstrainedOptimizationProblem
{
    IBoundedContinuousSearchSpace SearchSpace { get; }
    OptimizationSense Sense { get; }
    int InequalityCount { get; }
    int EqualityCount { get; }
    double EqualityTolerance { get; }
    double EvaluateObjective(ReadOnlySpan<double> solution);
    void EvaluateConstraints(ReadOnlySpan<double> solution, Span<double> inequalities, Span<double> equalities);
}

public sealed class ContinuousConstrainedOptimizationProblem : IContinuousConstrainedOptimizationProblem
{
    private readonly ContinuousConstrainedObjective _objective;
    private readonly ContinuousConstraintEvaluator _constraints;

    public ContinuousConstrainedOptimizationProblem(
        IBoundedContinuousSearchSpace searchSpace,
        OptimizationSense sense,
        int inequalityCount,
        int equalityCount,
        ContinuousConstrainedObjective objective,
        ContinuousConstraintEvaluator constraints,
        double equalityTolerance = 1e-6)
    {
        ArgumentNullException.ThrowIfNull(searchSpace);
        ArgumentNullException.ThrowIfNull(objective);
        ArgumentNullException.ThrowIfNull(constraints);
        if (inequalityCount < 0) throw new ArgumentOutOfRangeException(nameof(inequalityCount));
        if (equalityCount < 0) throw new ArgumentOutOfRangeException(nameof(equalityCount));
        if (inequalityCount + equalityCount == 0) throw new ArgumentException("At least one constraint is required.");
        if (!double.IsFinite(equalityTolerance) || equalityTolerance < 0.0) throw new ArgumentOutOfRangeException(nameof(equalityTolerance));
        SearchSpace = searchSpace;
        Sense = sense;
        InequalityCount = inequalityCount;
        EqualityCount = equalityCount;
        EqualityTolerance = equalityTolerance;
        _objective = objective;
        _constraints = constraints;
    }

    public IBoundedContinuousSearchSpace SearchSpace { get; }
    public OptimizationSense Sense { get; }
    public int InequalityCount { get; }
    public int EqualityCount { get; }
    public double EqualityTolerance { get; }

    public double EvaluateObjective(ReadOnlySpan<double> solution)
    {
        RequireDimension(solution);
        double value = _objective(solution);
        if (!double.IsFinite(value)) throw new InvalidOperationException("Objective evaluation must be finite.");
        return value;
    }

    public void EvaluateConstraints(ReadOnlySpan<double> solution, Span<double> inequalities, Span<double> equalities)
    {
        RequireDimension(solution);
        if (inequalities.Length != InequalityCount || equalities.Length != EqualityCount)
            throw new ArgumentException("Constraint vector dimensions do not match the problem.");
        _constraints(solution, inequalities, equalities);
        for (int i = 0; i < inequalities.Length; i++) if (!double.IsFinite(inequalities[i])) throw new InvalidOperationException("Inequality evaluation must be finite.");
        for (int i = 0; i < equalities.Length; i++) if (!double.IsFinite(equalities[i])) throw new InvalidOperationException("Equality evaluation must be finite.");
    }

    private void RequireDimension(ReadOnlySpan<double> solution)
    {
        if (solution.Length != SearchSpace.Dimension) throw new ArgumentException("Solution dimension does not match the search space.", nameof(solution));
    }
}

public sealed class ConstraintEvaluation
{
    public ConstraintEvaluation(double[] inequalities, double[] equalities, double equalityTolerance)
    {
        ArgumentNullException.ThrowIfNull(inequalities);
        ArgumentNullException.ThrowIfNull(equalities);
        Inequalities = (double[])inequalities.Clone();
        Equalities = (double[])equalities.Clone();
        EqualityTolerance = equalityTolerance;
        double total = 0.0;
        for (int i = 0; i < inequalities.Length; i++) total += Math.Max(0.0, inequalities[i]);
        for (int i = 0; i < equalities.Length; i++) total += Math.Max(0.0, Math.Abs(equalities[i]) - equalityTolerance);
        TotalViolation = total;
        IsFeasible = total <= 0.0;
    }
    public double[] Inequalities { get; }
    public double[] Equalities { get; }
    public double EqualityTolerance { get; }
    public double TotalViolation { get; }
    public bool IsFeasible { get; }
    public double[] ViolationVector()
    {
        double[] values = new double[Inequalities.Length + Equalities.Length];
        for (int i = 0; i < Inequalities.Length; i++) values[i] = Math.Max(0.0, Inequalities[i]);
        for (int i = 0; i < Equalities.Length; i++) values[Inequalities.Length + i] = Math.Max(0.0, Math.Abs(Equalities[i]) - EqualityTolerance);
        return values;
    }
}

public sealed class ConstrainedPoint
{
    public ConstrainedPoint(double[] solution, double objective, ConstraintEvaluation constraints)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(constraints);
        Solution = (double[])solution.Clone(); Objective = objective; Constraints = constraints;
    }
    public double[] Solution { get; }
    public double Objective { get; }
    public ConstraintEvaluation Constraints { get; }
}

public sealed class ConstrainedOptimizationResult
{
    public ConstrainedOptimizationResult(ConstrainedPoint best, int evaluations, int iterations, ulong seed)
    {
        ArgumentNullException.ThrowIfNull(best); Best = best; Evaluations = evaluations; Iterations = iterations; Seed = seed;
    }
    public ConstrainedPoint Best { get; }
    public int Evaluations { get; }
    public int Iterations { get; }
    public ulong Seed { get; }
}

public interface IConstrainedOptimizer<in TParameters> where TParameters : IMetaheuristicParameters
{
    ConstrainedOptimizationResult Optimize(IContinuousConstrainedOptimizationProblem problem, TParameters parameters, OptimizationOptions? options = null, CancellationToken cancellationToken = default);
}

internal sealed class ConstrainedCandidate
{
    public ConstrainedCandidate(double[] position, double objective, ConstraintEvaluation constraints) { Position = position; Objective = objective; Constraints = constraints; }
    public double[] Position { get; }
    public double Objective { get; }
    public ConstraintEvaluation Constraints { get; }
    public double Score { get; set; }
}

internal static class ConstraintToolkit
{
    public static IRandomSource CreateRandom(OptimizationOptions? options, out ulong seed)
    {
        options ??= new OptimizationOptions(); options.Validate(); seed = options.Seed; return options.RandomSourceFactory.Create(seed);
    }
    public static double ObjectiveKey(double objective, OptimizationSense sense) => sense == OptimizationSense.Minimize ? objective : -objective;
    public static ConstrainedCandidate Evaluate(IContinuousConstrainedOptimizationProblem problem, double[] position, ref int evaluations)
    {
        double objective = problem.EvaluateObjective(position);
        double[] inequalities = new double[problem.InequalityCount];
        double[] equalities = new double[problem.EqualityCount];
        problem.EvaluateConstraints(position, inequalities, equalities);
        evaluations++;
        return new ConstrainedCandidate(position, objective, new ConstraintEvaluation(inequalities, equalities, problem.EqualityTolerance));
    }
    public static List<ConstrainedCandidate> Initialize(IContinuousConstrainedOptimizationProblem problem, int size, IRandomSource random, ref int evaluations)
    {
        List<ConstrainedCandidate> population = new(size);
        for (int i = 0; i < size; i++) { double[] position = new double[problem.SearchSpace.Dimension]; problem.SearchSpace.Sample(random, position); population.Add(Evaluate(problem, position, ref evaluations)); }
        return population;
    }
    public static int DebCompare(ConstrainedCandidate left, ConstrainedCandidate right, OptimizationSense sense)
    {
        if (left.Constraints.IsFeasible && !right.Constraints.IsFeasible) return -1;
        if (!left.Constraints.IsFeasible && right.Constraints.IsFeasible) return 1;
        if (left.Constraints.IsFeasible) return ObjectiveKey(left.Objective, sense).CompareTo(ObjectiveKey(right.Objective, sense));
        return left.Constraints.TotalViolation.CompareTo(right.Constraints.TotalViolation);
    }
    public static ConstrainedCandidate BestByDeb(IReadOnlyList<ConstrainedCandidate> population, OptimizationSense sense)
    {
        ConstrainedCandidate best = population[0]; for (int i = 1; i < population.Count; i++) if (DebCompare(population[i], best, sense) < 0) best = population[i]; return best;
    }
    public static ConstrainedCandidate Tournament(IReadOnlyList<ConstrainedCandidate> population, IRandomSource random, Func<ConstrainedCandidate,ConstrainedCandidate,int> compare)
    {
        ConstrainedCandidate first = population[random.NextInt32(population.Count)]; ConstrainedCandidate second = population[random.NextInt32(population.Count)]; int order = compare(first, second); return order < 0 ? first : order > 0 ? second : (random.NextDouble() < 0.5 ? first : second);
    }
    public static List<ConstrainedCandidate> Select(IEnumerable<ConstrainedCandidate> candidates, int count, Comparison<ConstrainedCandidate> comparison)
    {
        List<ConstrainedCandidate> ordered = candidates.ToList(); ordered.Sort(comparison); if (ordered.Count > count) ordered.RemoveRange(count, ordered.Count-count); return ordered;
    }
    public static double[] SbxChild(ReadOnlySpan<double> first, ReadOnlySpan<double> second, IBoundedContinuousSearchSpace space, IRandomSource random, double crossoverProbability, double distributionIndex)
    {
        double[] child = first.ToArray(); if (random.NextDouble() > crossoverProbability) return child; ReadOnlySpan<double> lower = space.LowerBounds; ReadOnlySpan<double> upper = space.UpperBounds;
        for (int i=0;i<child.Length;i++) { if (random.NextDouble()>0.5) continue; double u=random.NextDouble(); double beta=u<=0.5?Math.Pow(2.0*u,1.0/(distributionIndex+1.0)):Math.Pow(1.0/(2.0*(1.0-u)),1.0/(distributionIndex+1.0)); child[i]=0.5*((1.0+beta)*first[i]+(1.0-beta)*second[i]); child[i]=Math.Clamp(child[i],lower[i],upper[i]); }
        return child;
    }
    public static void PolynomialMutate(Span<double> position, IBoundedContinuousSearchSpace space, IRandomSource random, double probability, double distributionIndex)
    {
        ReadOnlySpan<double> lower=space.LowerBounds; ReadOnlySpan<double> upper=space.UpperBounds;
        for(int i=0;i<position.Length;i++){ if(random.NextDouble()>probability) continue; double width=upper[i]-lower[i]; if(width<=0.0) continue; double u=random.NextDouble(); double delta=u<0.5?Math.Pow(2.0*u,1.0/(distributionIndex+1.0))-1.0:1.0-Math.Pow(2.0*(1.0-u),1.0/(distributionIndex+1.0)); position[i]=Math.Clamp(position[i]+delta*width,lower[i],upper[i]); }
    }
    public static double NextGaussian(IRandomSource random) { double u1=Math.Max(random.NextDouble(),1e-12); double u2=random.NextDouble(); return Math.Sqrt(-2.0*Math.Log(u1))*Math.Cos(2.0*Math.PI*u2); }
    public static double[] GaussianChild(ReadOnlySpan<double> parent, IBoundedContinuousSearchSpace space, IRandomSource random, double sigma)
    {
        double[] child=parent.ToArray(); ReadOnlySpan<double> lower=space.LowerBounds; ReadOnlySpan<double> upper=space.UpperBounds; for(int i=0;i<child.Length;i++){double width=upper[i]-lower[i]; child[i]=Math.Clamp(child[i]+sigma*width*NextGaussian(random),lower[i],upper[i]);} return child;
    }
    public static double[] DifferentialTrial(IReadOnlyList<ConstrainedCandidate> population, int targetIndex, IBoundedContinuousSearchSpace space, IRandomSource random, double differentialWeight, double crossoverProbability)
    {
        int n=population.Count; int a,b,c; do{a=random.NextInt32(n);}while(a==targetIndex); do{b=random.NextInt32(n);}while(b==targetIndex||b==a); do{c=random.NextInt32(n);}while(c==targetIndex||c==a||c==b); double[] target=population[targetIndex].Position; double[] trial=(double[])target.Clone(); int forced=random.NextInt32(trial.Length); ReadOnlySpan<double> lower=space.LowerBounds; ReadOnlySpan<double> upper=space.UpperBounds;
        for(int i=0;i<trial.Length;i++){if(i!=forced&&random.NextDouble()>crossoverProbability)continue; trial[i]=population[a].Position[i]+differentialWeight*(population[b].Position[i]-population[c].Position[i]); trial[i]=Math.Clamp(trial[i],lower[i],upper[i]);} return trial;
    }
    public static ConstrainedPoint ToPoint(ConstrainedCandidate candidate)=>new(candidate.Position,candidate.Objective,candidate.Constraints);
    public static double ViolationPower(ConstrainedCandidate candidate,double power){double total=0.0;foreach(double v in candidate.Constraints.ViolationVector())total+=Math.Pow(v,power);return total;}
}
