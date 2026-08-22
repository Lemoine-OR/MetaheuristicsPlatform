@page advanced_genetic_algorithm_operators Advanced Genetic Algorithm Operators

# Advanced Genetic Algorithm Operators

Stable component namespace: `ga.*`

v0.42.0 extends the canonical `genetic-algorithm-generational` composition with a
scientifically audited operator catalog while preserving **one** public GA algorithm ID.
The operator IDs below are stable component IDs, not additional top-level algorithms.

## Scientific scope

The catalog deliberately separates representation-independent parent selection from
representation-specific crossover and mutation. One-point, two-point and uniform
crossover operate on equal-length arrays and do not pretend to preserve permutation
feasibility. PMX and OX1 explicitly validate permutation legality. SBX, Gaussian mutation
and polynomial mutation require finite explicit bounds.

Fitness-proportionate selection does not assume that the raw objective is already a
non-negative fitness. The component requires an explicit user-supplied weight transform;
invalid or all-zero weights are rejected.

## Executable selection components

- `ga.selection.tournament` — canonical v0.41 tournament selector, sampling with replacement.
- `ga.selection.truncation` — uniform sampling from the best configured population fraction.
- `ga.selection.linear-ranking` — linear rank probabilities with selective pressure in `[1,2]`.
- `ga.selection.exponential-ranking` — exponential decay by objective rank.
- `ga.selection.fitness-proportionate-explicit-weights` — roulette-wheel sampling over
  explicit finite non-negative weights.

For rank zero denoting the best individual, linear ranking uses

\f[
p_r=\frac{1}{N}\left(s-\frac{2(s-1)r}{N-1}\right),
\qquad 1\le s\le 2.
\f]

The exponential-rank implementation uses

\f[
p_r=\frac{e^{-\lambda r}}{\sum_{j=0}^{N-1}e^{-\lambda j}},
\qquad \lambda>0.
\f]

## Executable crossover components

### Sequence arrays

- `ga.crossover.one-point`
- `ga.crossover.two-point`
- `ga.crossover.uniform`

These operators require equal array lengths. They are intentionally not labelled
permutation-safe.

### Permutations

- `ga.crossover.pmx` — Goldberg-Lingle Partially Mapped Crossover.
- `ga.crossover.ox1` — Davis Order Crossover OX1.

Both operators require unique alleles and identical allele sets in the two parents.
They reject malformed permutation inputs instead of silently repairing them.

### Bounded real vectors

- `ga.crossover.sbx-bounded` — bounded Simulated Binary Crossover.

The original SBX was introduced by Deb and Agrawal (1995). The implementation uses the
explicitly bounded real-coded form widely associated with NSGA-II. The original 1995
paper is cited without a fabricated DOI; the bounded-use reference has DOI
`10.1109/4235.996017`.

## Executable mutation components

- `ga.mutation.bit-flip` — explicit per-bit probability.
- `ga.mutation.integer-random-reset` — bounded integer random reset, excluding the current
  value whenever the interval contains another value.
- `ga.mutation.swap` — one two-position swap per invocation.
- `ga.mutation.inversion` — one contiguous reversal per invocation.
- `ga.mutation.gaussian-bounded` — Gaussian perturbation followed by bound projection.
- `ga.mutation.polynomial-bounded` — bounded polynomial mutation.

The GA-level `MutationProbability` remains the probability of invoking the configured
mutation method on an offspring. Per-locus probabilities belong to the operator itself.

For bounded Gaussian mutation,

\f[
x_i'=\Pi_{[L_i,U_i]}\!\left(x_i+\sigma Z_i\right),
\qquad Z_i\sim\mathcal N(0,1).
\f]

For bounded polynomial mutation,

\f[
x_i'=\Pi_{[L_i,U_i]}\!\left(x_i+\delta_q(U_i-L_i)\right),
\f]

where \f$\delta_q\f$ is the standard polynomial perturbation determined by a uniform draw,
current normalized distance to each bound and distribution index \f$\eta_m\f$.

## Replacement boundary

`ga.replacement.generational-elitist` describes the executable v0.41/v0.42 lifecycle:

\f[
P_{t+1}=E_t\cup O_t,
\qquad |P_{t+1}|=N.
\f]

`ga.replacement.steady-state` is **reviewed/deferred**. It is not implemented as a cosmetic
replacement policy because true steady-state evolution changes the live population before
subsequent parent selections. The current optimizer selects all parents for a generation
from the same parental snapshot, so silently calling that loop steady-state would be
scientifically false.

## Complexity

Selection methods using ranking sort the population for each `SelectParent` call under the
current v0.41 selection interface, hence have \f$O(N\log N)\f$ call cost. Tournament and
explicit-weight selection are \f$O(k)\f$ and \f$O(N)\f$ respectively. Array crossover and
mutation components are \f$O(n)\f$ in representation length; swap is \f$O(1)\f$.
Permutation validation makes PMX/OX1 \f$O(n)\f$ expected time with hash-based membership.

A later selection-context API may cache rankings once per generation without changing
component IDs.

## Scientific references

- Blickle, T.; Thiele, L. (1996), *A Comparison of Selection Schemes used in Evolutionary
  Algorithms*, DOI `10.1162/EVCO.1996.4.4.361`.
- Goldberg, D. E.; Deb, K. (1991), *A Comparative Analysis of Selection Schemes Used in
  Genetic Algorithms*, DOI `10.1016/B978-0-08-050684-5.50008-2`.
- Syswerda, G. (1989), *Uniform Crossover in Genetic Algorithms*, ICGA 1989, 2-9.
  DOI `10.5555/645512.657265`.
- Syswerda, G. (1991), *A Study of Reproduction in Generational and Steady-State Genetic Algorithms*,
  *Foundations of Genetic Algorithms* 1, 94-101. DOI `10.1016/B978-0-08-050684-5.50009-4`.
- Goldberg, D. E.; Lingle, R. (1985), *Alleles, Loci, and the Traveling Salesman Problem*,
  DOI `10.5555/645511.657095`.
- Davis, L. (1985), *Applying Adaptive Algorithms to Epistatic Domains*,
  DOI `10.5555/1625135.1625164`.
- Deb, K.; Agrawal, R. B. (1995), *Simulated Binary Crossover for Continuous Search Space*,
  *Complex Systems* 9(2), 115-148. No DOI is asserted for the original publication.
- Deb, K.; Pratap, A.; Agarwal, S.; Meyarivan, T. (2002), *A fast and elitist multiobjective
  genetic algorithm: NSGA-II*, DOI `10.1109/4235.996017`.
- Deb, K.; Deb, D. (2014), *Analysing mutation schemes for real-parameter genetic
  algorithms*, DOI `10.1504/IJAISC.2014.059280`.
