@page nondominated_sorting_genetic_algorithm_srinivas_deb_1994 Nondominated Sorting Genetic Algorithm

# Nondominated Sorting Genetic Algorithm

## General description

Nondominated Sorting Genetic Algorithm (`Nsga`) is the public scientific identity associated with Srinivas & Deb (1994),
Multiobjective Optimization Using Nondominated Sorting in Genetic Algorithms. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `nondominated-sorting-genetic-algorithm-srinivas-deb-1994`
- Class: `NsgaOptimizer`
- Parameters: `NsgaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.108.0
- Primary DOI: `10.1162/evco.1994.2.3.221`

## Complexity

O(MN^2) nondominated sorting plus sharing and objective evaluations per generation. Memory usage is O(N(D+M)).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Nondominated Sorting Genetic Algorithm is appropriate.

## Detailed operation

Original non-elitist NSGA using nondominated ranks and objective-space fitness sharing within fronts.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`NsgaParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Nsga;
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

NsgaOptimizer algorithm =
    MetaheuristicFactory.Create<NsgaOptimizer>(
        MetaheuristicAlgorithmIds.Nsga);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new NsgaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`nondominated-sorting-genetic-algorithm-srinivas-deb-1994`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\operatorname{rank}(x)&=\operatorname{front}(x),\\m_i&=\sum_{j\in F_i}\max\!\left(0,1-\left(\frac{d_{ij}}{\sigma_{\mathrm{share}}}\right)^\alpha\right),\qquad \widetilde F_i=F_i/m_i.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Srinivas & Deb (1994), Multiobjective Optimization Using Nondominated Sorting in Genetic Algorithms, Evolutionary Computation 2(3), 221-248. DOI: `10.1162/evco.1994.2.3.221`.
