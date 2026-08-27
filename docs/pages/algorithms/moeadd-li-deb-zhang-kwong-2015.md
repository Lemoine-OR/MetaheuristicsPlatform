@page moeadd_li_deb_zhang_kwong_2015 MOEA/DD

# MOEA/DD

## General description

MOEA/DD (`Moeadd`) is the public scientific identity associated with Li, Deb, Zhang & Kwong (2015),
An Evolutionary Many-Objective Optimization Algorithm Based on Dominance and Decomposition. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `moeadd-li-deb-zhang-kwong-2015`
- Class: `MoeaddOptimizer`
- Parameters: `MoeaddParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.114.0
- Primary DOI: `10.1109/TEVC.2014.2373386`

## Complexity

O(MN^2+NRM) dominance sorting and reference-region decomposition per generation. Memory usage is O(N(D+M)+RM).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of MOEA/DD is appropriate.

## Detailed operation

MOEA/DD unifies Pareto dominance and decomposition through reference subregions and decomposition values.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`MoeaddParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Moeadd;
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

MoeaddOptimizer algorithm =
    MetaheuristicFactory.Create<MoeaddOptimizer>(
        MetaheuristicAlgorithmIds.Moeadd);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new MoeaddParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`moeadd-li-deb-zhang-kwong-2015`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}r(x)&=\arg\min_k d_\perp(\bar f(x),\lambda^k),\\PBI(x\mid\lambda^k)&=d_1+\theta d_2,\qquad P_{t+1}=\operatorname{DominanceDecompositionSelect}(P_t\cup Q_t,N).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Li, Deb, Zhang & Kwong (2015), An Evolutionary Many-Objective Optimization Algorithm Based on Dominance and Decomposition, IEEE Transactions on Evolutionary Computation 19(5), 694-716. DOI: `10.1109/TEVC.2014.2373386`.
