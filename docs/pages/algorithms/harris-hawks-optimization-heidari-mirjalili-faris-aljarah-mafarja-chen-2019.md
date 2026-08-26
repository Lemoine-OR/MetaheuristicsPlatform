@page harris_hawks_optimization_heidari_mirjalili_faris_aljarah_mafarja_chen_2019 Harris Hawks Optimization

# Harris Hawks Optimization

## General description

Harris Hawks Optimization (`HHO`) is the scientific identity introduced by Heidari, Mirjalili, Faris, Aljarah, Mafarja & Chen in 2019.
This page documents the canonical bounded-continuous platform implementation corresponding to that
publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019`
- Class: `HarrisHawksOptimizer`
- Parameters: `HarrisHawksOptimizerParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.77.0
- Primary DOI: `10.1016/j.future.2019.02.028`

## Complexity

O(ND) population motion plus up to 2N rapid-dive candidate evaluations per generation. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization using surprise-pounce exploration, besiege modes and Levy rapid dives.

## Detailed operation

Canonical HHO exploration plus four exploitation modes governed by escaping energy and prey escape probability, including beta=1.5 Levy rapid dives.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `HarrisHawksOptimizerParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<HarrisHawksOptimizer>(
        MetaheuristicAlgorithmIds.HarrisHawksOptimization);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new HarrisHawksOptimizerParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}E_t&=2E_0\left(1-\frac{t}{T}\right),\quad E_0\sim\mathcal U[-1,1],\quad J=2(1-r_5),\\X^{t+1}&=X_{\rm rabbit}-E_t\left|X_{\rm rabbit}-X^t\right|\quad(r\ge\tfrac12,|E_t|<\tfrac12),\\X^{t+1}&=(X_{\rm rabbit}-X^t)-E_t\left|JX_{\rm rabbit}-X^t\right|\quad(r\ge\tfrac12,|E_t|\ge\tfrac12),\\Y&=X_{\rm rabbit}-E_t\left|JX_{\rm rabbit}-Z\right|,\quad Z\in\{X^t,X_{\rm mean}^t\},\\X_2&=Y+R\odot L_{1.5}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; at least two hawks; the rabbit is the best-so-far solution and Levy rapid dives use the beta=1.5 Mantegna realization.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; escaping energy E transitions the canonical mechanism from exploration to the four exploitation strategies.

### Scientific references

Heidari, Mirjalili, Faris, Aljarah, Mafarja & Chen (2019), Harris hawks optimization: Algorithm and applications, Future Generation Computer Systems 97, 849-872.
DOI: `10.1016/j.future.2019.02.028`.
