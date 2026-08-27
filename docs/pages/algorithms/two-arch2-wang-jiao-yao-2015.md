@page two_arch2_wang_jiao_yao_2015 Two_Arch2

# Two_Arch2

## General description

Two_Arch2 (`TwoArch2`) is the public scientific identity associated with Wang, Jiao & Yao (2015),
Two_Arch2: An Improved Two-Archive Algorithm for Many-Objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `two-arch2-wang-jiao-yao-2015`
- Class: `TwoArch2Optimizer`
- Parameters: `TwoArch2Parameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.113.0
- Primary DOI: `10.1109/TEVC.2014.2350987`

## Complexity

O(MN^2+ANM) dominance and two-archive update work per generation. Memory usage is O((N+A_c+A_d)(D+M)).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Two_Arch2 is appropriate.

## Detailed operation

Two_Arch2 separates convergence and diversity responsibilities into two cooperating archives with distinct selection principles.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`TwoArch2Parameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.TwoArch2;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem =
    new ContinuousMultiobjectiveOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(4, 0.0, 1.0),
        new[]
        {
            OptimizationSense.Minimize,
            OptimizationSense.Minimize
        },
        static (
            ReadOnlySpan<double> x,
            Span<double> f) =>
        {
            f[0] = x[0];
            f[1] =
                1.0 -
                Math.Sqrt(x[0]) +
                x[1] +
                x[2] +
                x[3];
        });

TwoArch2Optimizer algorithm =
    MetaheuristicFactory.Create<TwoArch2Optimizer>(
        MetaheuristicAlgorithmIds.TwoArch2);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new TwoArch2Parameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`two-arch2-wang-jiao-yao-2015`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}A_c^{t+1}&=\operatorname{ConvergenceArchive}(A_c^t\cup A_d^t\cup Q_t),\\A_d^{t+1}&=\operatorname{DiversityArchive}_{L_p}(A_c^{t+1},A_d^t\cup Q_t).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Wang, Jiao & Yao (2015), Two_Arch2: An Improved Two-Archive Algorithm for Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 19(4), 524-541. DOI: `10.1109/TEVC.2014.2350987`.
