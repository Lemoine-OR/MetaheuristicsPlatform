@page teaching_learning_based_optimization_rao_savsani_vakharia_2011 Teaching-Learning-Based Optimization

# Teaching-Learning-Based Optimization

## General description

Teaching-Learning-Based Optimization (`TLBO`) is the scientific identity introduced by Rao, Savsani & Vakharia in 2011.
This page documents the canonical bounded-continuous platform implementation corresponding to that publication,
without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `teaching-learning-based-optimization-rao-savsani-vakharia-2011`
- Class: `TeachingLearningBasedOptimizationOptimizer`
- Parameters: `TeachingLearningBasedOptimizationParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.80.0
- Primary DOI: `10.1016/j.cad.2010.12.015`

## Complexity

O(ND) per iteration plus 2N objective evaluations. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free optimization using teacher-phase mean displacement and learner-to-learner interaction without algorithm-specific tuning parameters.

## Detailed operation

Canonical 2011 TLBO teacher phase with randomly selected teaching factor 1 or 2, followed by the published learner phase and greedy replacement after each candidate evaluation.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `TeachingLearningBasedOptimizationParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<TeachingLearningBasedOptimizationOptimizer>(
        MetaheuristicAlgorithmIds.TeachingLearningBasedOptimization);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new TeachingLearningBasedOptimizationParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`teaching-learning-based-optimization-rao-savsani-vakharia-2011`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}M^t&=\frac1N\sum_{i=1}^N x_i^t,\quad T_F\in\{1,2\},\\y_i^t&=x_i^t+r_i^t\bigl(T^t-T_FM^t\bigr),\\z_i^t&=\begin{cases}y_i^t+s_i^t(y_i^t-y_j^t),&f(y_i^t)\le f(y_j^t),\\y_i^t+s_i^t(y_j^t-y_i^t),&\text{otherwise},\end{cases}\quad j\ne i\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values and at least two learners. The teaching factor is generated internally as 1 or 2 with equal probability and is not an algorithm-specific input parameter.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; teacher and learner candidates are accepted only when they improve the incumbent under the configured objective sense.

### Scientific references

Rao, Savsani & Vakharia (2011), Teaching-learning-based optimization: A novel method for constrained mechanical design optimization problems, Computer-Aided Design 43(3), 303-315.
DOI: `10.1016/j.cad.2010.12.015`.
