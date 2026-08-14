# PSO dynamics

## Inertia weight

Shi and Eberhart introduced the inertia weight in:

Y. Shi, R. C. Eberhart,
"A modified particle swarm optimizer",
IEEE International Conference on Evolutionary Computation, 1998.
DOI: 10.1109/ICEC.1998.699146

The platform supports:
- constant inertia;
- linear inertia schedules.

## Clerc-Kennedy constriction

M. Clerc, J. Kennedy,
"The particle swarm - explosion, stability, and convergence in a multidimensional complex space",
IEEE Transactions on Evolutionary Computation 6(1), 58-73, 2002.
DOI: 10.1109/4235.985692

The implemented factor is:

```text
chi =
    2*kappa /
    |2 - phi - sqrt(phi^2 - 4*phi)|
```

with `phi > 4` and `0 < kappa <= 1`.

For `phi = 4.10`, `kappa = 1`, chi is approximately `0.7298437881`.

## Velocity limits

Velocity limiting is component-wise and expressed as a fraction of each variable's
search-space range.

This is kept separate from the constriction equation because published and experimental
PSO variants use different limiting policies.

Eberhart and Shi compared inertia and constriction approaches in:

R. C. Eberhart, Y. Shi,
"Comparing inertia weights and constriction factors in particle swarm optimization",
CEC 2000.
DOI: 10.1109/CEC.2000.870279