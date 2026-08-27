@page vaea_xiang_zhou_li_chen_2017 Vector Angle-Based Evolutionary Algorithm

# Vector Angle-Based Evolutionary Algorithm

## General description

Vector Angle-Based Evolutionary Algorithm (`Vaea`) is the public scientific identity associated with Xiang, Zhou, Li & Chen (2017),
A Vector Angle-Based Evolutionary Algorithm for Unconstrained Many-Objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `vaea-xiang-zhou-li-chen-2017`
- Class: `VaeaOptimizer`
- Parameters: `VaeaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.117.0
- Primary DOI: `10.1109/TEVC.2016.2587808`

## Complexity

O(MN^2) nondominated sorting and vector-angle selection per generation. Memory usage is O(N(D+M)+N^2).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Vector Angle-Based Evolutionary Algorithm is appropriate.

## Detailed operation

VaEA uses normalized objective-vector angles for diversity and convergence-aware elimination without predefined reference vectors.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`VaeaParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Vaea;
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

VaeaOptimizer algorithm =
    MetaheuristicFactory.Create<VaeaOptimizer>(
        MetaheuristicAlgorithmIds.Vaea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new VaeaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`vaea-xiang-zhou-li-chen-2017`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\theta_{ij}&=\arccos\!\frac{\bar f_i^\top\bar f_j}{\|\bar f_i\|\,\|\bar f_j\|},\\x^\star&=\arg\max_{x\in F_\ell}\min_{y\in S}\theta(x,y),\qquad S\leftarrow S\cup\{x^\star\}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Xiang, Zhou, Li & Chen (2017), A Vector Angle-Based Evolutionary Algorithm for Unconstrained Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 21(1), 131-152. DOI: `10.1109/TEVC.2016.2587808`.
