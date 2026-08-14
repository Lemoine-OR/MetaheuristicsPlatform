# PSO communication topologies — state of the art and implementation roadmap

## Architectural principle

The literature sometimes uses the same name for different communication structures.
The platform therefore uses structural class names and records historical aliases in
metadata.

Examples:
- `FullyConnectedTopology` corresponds to the communication graph behind classical
  gbest / "all";
- `HubAndSpokeTopology` corresponds to the "star" structure explicitly described by
  Kennedy & Mendes (2002);
- `ToroidalVonNeumannTopology` corresponds to the 2-D toroidal square/Von-Neumann
  structure.

## Implemented in v0.4.0

| Platform topology | Dynamics | Exact published variant? | Main source |
|---|---:|---:|---|
| FullyConnectedTopology | Static | Yes, graph structure | Kennedy & Eberhart 1995; Kennedy & Mendes 2002 |
| RingTopology | Static | Yes, graph structure | Kennedy & Mendes 2002 |
| HubAndSpokeTopology | Static | Yes, graph structure | Kennedy & Mendes 2002 |
| ToroidalVonNeumannTopology | Static | Yes, graph structure | Kennedy & Mendes 2002; Mendes et al. 2004 |
| RandomConnectedTopology | Random static | No, generic connected random graph | Kennedy & Mendes 2002 |
| ClusteredTopology | Static | No, generalized cluster/gateway topology | Mendes et al. 2004 |
| WattsStrogatzSmallWorldTopology | Random static | No, generic static small-world graph | Gong & Zhang 2013 |
| BarabasiAlbertScaleFreeTopology | Random static | No, generic static scale-free graph | Zhang & Yi 2011 |
| CustomGraphTopology | Static | User-defined | — |
| DClusterTopology | Fitness dynamic | **Yes** | **El Dor et al. 2015** |

## DCluster

Reference:

A. El Dor, D. Lemoine, M. Clerc, P. Siarry, L. Deroussi, M. Gourgand,
"Dynamic cluster in particle swarm optimization algorithm",
Natural Computing 14(4), 655-672, 2015.
DOI: 10.1007/s11047-014-9465-2

The exact regular implementation:
1. ranks particles from worst current fitness to best current fitness;
2. splits the ranking into equal cliques of size `p`;
3. uses `p + 1` clusters, hence `N = p(p+1)`;
4. treats the first/worst cluster as central;
5. links each central particle to the worst particle of one distinct outer cluster;
6. rebuilds the graph dynamically from current fitness.

Optimization sense is respected generically:
- minimization: high objective values rank as worse;
- maximization: low objective values rank as worse.

A future generalized dynamic clustered method will have a different class name and will
not be presented as exact DCluster.

## Important planned exact variants

These are deliberately not approximated in v0.4.0. They will be added when the PSO
runtime exposes exactly the state needed by the published method.

### Progressive / spatial neighborhoods

P. N. Suganthan,
"Particle swarm optimiser with neighbourhood operator",
CEC 1999.
DOI: 10.1109/CEC.1999.785514

### Dynamic hierarchy

S. Janson, M. Middendorf,
"A hierarchical particle swarm optimizer",
CEC 2003.
DOI: 10.1109/CEC.2003.1299745

### Fully Informed Particle Swarm (FIPS)

R. Mendes, J. Kennedy, J. Neves,
"The Fully Informed Particle Swarm: Simpler, Maybe Better",
IEEE Transactions on Evolutionary Computation 8(3), 204-210, 2004.
DOI: 10.1109/TEVC.2004.826074

FIPS is a **social-influence policy**, not a topology. It will be implemented separately
and will work with several graph topologies.

### Scale-Free Fully Informed PSO (SFIPSO)

C. Zhang, Z. Yi,
"Scale-free fully informed particle swarm optimization algorithm",
Information Sciences 181(20), 4550-4568, 2011.
DOI: 10.1016/j.ins.2011.02.026

The exact method is more than a Barabasi-Albert graph: it couples modified scale-free
network construction with fitness/spatial information and fully-informed influence.
Therefore the generic `BarabasiAlbertScaleFreeTopology` is not labeled SFIPSO.

### Adaptive small-world PSO

Y.-J. Gong, J. Zhang,
"Small-world particle swarm optimization with topology adaptation",
GECCO 2013.
DOI: 10.1145/2463372.2463381

The exact adaptive variant changes topology parameters according to swarm state and
stagnation. It will be added as a dynamic policy rather than conflated with the static
small-world graph.

## Graph metrics

The foundation computes:
- node count;
- edge count;
- self loops;
- connected components;
- min/max/average structural degree;
- degree variance;
- density;
- diameter;
- average shortest path length among reachable pairs;
- average local clustering coefficient.

These metrics are diagnostic and are not recomputed automatically inside every PSO
iteration.