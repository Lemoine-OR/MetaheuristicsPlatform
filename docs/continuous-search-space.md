# Continuous search spaces

Continuous metaheuristics frequently need the same domain operations:
- dimension;
- lower and upper bounds;
- random sampling;
- bounds checking;
- clamping.

`IBoundedContinuousSearchSpace` defines these operations without introducing PSO-specific
concepts such as velocity, particles or topology.

`BoundedContinuousSearchSpace` stores defensive copies of the bounds and exposes them
as read-only spans.

This abstraction is intended to be reusable by:
- Particle Swarm Optimization;
- Differential Evolution;
- Evolution Strategies;
- continuous variants of Simulated Annealing;
- other vector-based continuous methods.

Boundary-handling policies beyond simple clamping remain outside this class because
their algorithmic consequences can differ between metaheuristics.