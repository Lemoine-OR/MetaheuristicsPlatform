# Multi-metaheuristic architecture conformance

With PSO and DE implemented, the platform now has two population-based continuous
metaheuristics with fundamentally different search dynamics.

Shared infrastructure:
- `IMetaheuristic<TSolution,TParameters>`;
- `OptimizationContext<TSolution>`;
- `OptimizationSense`;
- common stopping criteria;
- common callbacks;
- deterministic root seed and random-source factory;
- bounded continuous search space;
- adaptive generic evaluation execution;
- common result/statistics model.

Algorithm-owned infrastructure:

PSO:
- velocity;
- personal best;
- topology;
- social influence;
- synchronous swarm dynamics.

DE:
- donor-index sampling;
- differential mutation;
- crossover;
- one-to-one generational selection.

This separation is intentional. The Core shares lifecycle and cross-cutting services,
not hot-loop algorithm mechanics.