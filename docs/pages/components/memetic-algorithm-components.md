@page memetic_algorithm_components Memetic Algorithm Local Improvement and Learning Policies

# Memetic Algorithm Local Improvement and Learning Policies

## Scope

Version 0.43.0 separates **when / on whom local search is applied** from **how the
learned phenotype is inherited**. The policies operate above the shared generational GA
engine and below the public `MemeticAlgorithmOptimizer<TSolution>` composition.

## Executable local-improvement policies

### Every offspring

`ma.local-search.every-offspring`

\f[
C_t=O_t.
\f]

Every newly generated non-elite offspring receives local improvement.

### Periodic

`ma.local-search.periodic`

\f[
C_t=
\begin{cases}
O_t,&t\bmod k=0,\\
\varnothing,&\text{otherwise}.
\end{cases}
\f]

### Probabilistic

`ma.local-search.probabilistic`

\f[
\Pr(x\in C_t)=p_{\mathrm{LS}}.
\f]

The implementation avoids an RNG draw at the exact boundary probabilities zero and one.

### Top fraction

`ma.local-search.top-fraction`

For objective rank \f$r_t(x)\f$ among newly generated offspring,

\f[
x\in C_t
\Longleftrightarrow
r_t(x)
<
\max\!\left(1,\left\lceil q|O_t|\right\rceil\right).
\f]

Ranking is objective-sense symmetric.

### Stagnation-adaptive

`ma.local-search.adaptive-stagnation`

\f[
p_{\mathrm{LS}}(t)
=
p_{\min}
+
\left(p_{\max}-p_{\min}\right)
\min\!\left(1,\frac{s_t}{W}\right).
\f]

This first adaptive controller deliberately uses an observable generic signal
(stagnation of the global best) rather than embedding domain-specific diversity metrics.

## Executable learning policies

### Lamarckian

`ma.learning.lamarckian`

\f[
\begin{aligned}
x^{LS}&=\mathcal L(x),\\
g_{t+1}&\leftarrow x^{LS},\\
F&=f(x^{LS}).
\end{aligned}
\f]

### Baldwinian

`ma.learning.baldwinian`

\f[
\begin{aligned}
x^{LS}&=\mathcal L(x),\\
g_{t+1}&\leftarrow x,\\
F&=f(x^{LS}).
\end{aligned}
\f]

The implementation improves a cloned phenotype, so Baldwinian learning cannot mutate the
inherited genotype through aliasing.

## Reviewed / deferred extensions

### Self-adaptive meme choice

Adaptive choice among several local-search memes is scientifically important but requires
a dedicated credit-assignment and meme-population contract. It is reviewed rather than
simulated by a hard-coded selector in v0.43.

### Additional population engines

The local-improvement and learning contracts are representation independent. Direct DE or
other population-engine wiring is deferred until those runtimes expose an equivalent shared
generation extension point; v0.43 does not duplicate their lifecycle merely to claim support.

## Accounting and ownership

Local-search evaluations share the outer `OptimizationContext`:

\f[
N_{\mathrm{eval}}
=
N_{\mathrm{evolution}}
+
N_{\mathrm{LS}}.
\f]

The memetic state exposes local-search invocations, successful improvements, accepted local
moves, cumulative finite objective gain, stagnation length and the last application
probability.

## Scientific references

- Moscato, P. (1989), Caltech Concurrent Computation Program Report 826.
- Krasnogor, N.; Smith, J. (2005), IEEE Transactions on Evolutionary Computation 9(5),
  474-488, DOI `10.1109/TEVC.2005.850260`.
