@page pso_communication_topologies PSO Communication Topology Catalog

# PSO Communication Topology Catalog

## Purpose

MetaheuristicsPlatform separates the **PSO movement equation**, the **social influence policy** and the **communication topology**. A topology therefore defines *who can inform whom*; the social policy defines how those informers are converted into an attraction vector.

With the default `CanonicalBestInfluencePolicy`, particle `i` is attracted toward its own personal best and the best personal-best among the particles visible through its communication neighborhood. Changing topology changes the propagation of information without changing the common PSO state model.

The default `PsoParameters` uses `FullyConnectedTopology`. `ParticleSwarmOptimizer` has a graphless fast path for the specific combination `FullyConnectedTopology + CanonicalBestInfluencePolicy`.

## Dynamics and rebuild semantics

Topology descriptors use `PsoTopologyDynamics`:

- **Static** — deterministic graph built once and reused.
- **RandomStatic** — random graph sampled once per optimization run and reused.
- **DynamicRandom / FitnessDynamic / SpatialDynamic / AdaptiveDynamic / SelfOrganizing** — the runtime invalidates the graph after each completed iteration so it is rebuilt from current state before the next movement step.

`DClusterTopology` is currently the implemented **FitnessDynamic** topology and requires `CurrentFitness`.

## Implemented topologies

### Fully Connected — `fully-connected`

Class: `FullyConnectedTopology`

Default parameters: `IncludeSelf=true`.

Every pair of distinct particles is connected. With the default canonical best-neighborhood influence, each particle can therefore use the swarm-wide best personal-best as its social guide. This is the communication graph underlying classical **gbest PSO**.

The graph is static. For the canonical influence policy, the implementation avoids materializing this graph and uses the optimized graphless fully-connected path.

Scientific basis: Kennedy & Eberhart (1995), DOI `10.1109/ICNN.1995.488968`; Kennedy & Mendes (2002), DOI `10.1109/CEC.2002.1004493`; Mendes, Kennedy & Neves (2004), DOI `10.1109/TEVC.2004.826074`.

### Ring — `ring`

Class: `RingTopology`

Default parameters: `Radius=1`, `IncludeSelf=true`.

Particles are arranged on a cycle. Particle `i` is connected to `i-d` and `i+d` modulo `N` for every `d=1..Radius`. With radius 1, the communication neighborhood consists of the two adjacent particles plus self when `IncludeSelf=true`.

The graph is static. Local information must propagate around the ring, which slows global diffusion relative to fully connected PSO and can preserve diversity.

Scientific basis: Kennedy & Mendes (2002), DOI `10.1109/CEC.2002.1004493`; Mendes, Kennedy & Neves (2004), DOI `10.1109/TEVC.2004.826074`.

### Hub-and-Spoke — `hub-and-spoke`

Class: `HubAndSpokeTopology`

Default parameters: `HubIndex=0`, `IncludeSelf=true`.

One designated hub is connected to every other particle; peripheral particles are not directly connected to each other. Kennedy & Mendes (2002) call this graph a **star**. MetaheuristicsPlatform uses the structural name *Hub-and-Spoke* to avoid the literature ambiguity where “star” is sometimes used for gbest.

The graph is static.

Scientific basis: Kennedy & Mendes (2002), DOI `10.1109/CEC.2002.1004493`.

### Toroidal Von Neumann — `toroidal-von-neumann`

Class: `ToroidalVonNeumannTopology`

Default parameters: `Rows=null`, `Columns=null`, `IncludeSelf=true`.

Particles occupy a two-dimensional toroidal grid and connect **north, south, east and west**. Wrap-around edges make the grid toroidal.

If both dimensions are omitted, the implementation chooses a factorization of `N` near `sqrt(N)`. If one dimension is supplied, the swarm size must be divisible by it. If both are supplied, `Rows * Columns` must equal the swarm size.

The graph is static.

Scientific basis: Kennedy & Mendes (2002), DOI `10.1109/CEC.2002.1004493`; Mendes, Kennedy & Neves (2004), DOI `10.1109/TEVC.2004.826074`.

### Random Connected — `random-connected`

Class: `RandomConnectedTopology`

Default parameters: `ExtraEdgeProbability=0.15`, `IncludeSelf=true`.

The implementation first builds a randomized spanning tree, guaranteeing graph connectivity. It then samples additional undirected edges independently with probability `ExtraEdgeProbability`.

Its descriptor is **RandomStatic**: the graph is randomized once per optimization run, not rebuilt at every iteration.

This is a generic connected random-graph implementation inspired by random population structures evaluated by Kennedy & Mendes (2002), not a claim of reproducing one unique published adjacency matrix.

Scientific basis: Kennedy & Mendes (2002), DOI `10.1109/CEC.2002.1004493`.

### General Clustered — `clustered-general`

Class: `ClusteredTopology`

Default catalog parameters: `ClusterCount=4`, `GatewaysPerAdjacentPair=1`, `IncludeSelf=true`.

Particles are divided into contiguous near-balanced clusters. Every cluster is a clique. Adjacent clusters are linked by a configurable number of gateway edges and the cluster-level connections form a ring.

This creates dense local information exchange inside clusters and sparse inter-cluster communication.

The implementation is a reusable generalized clique-and-gateway structure and is **not** presented as the exact FourClusters adjacency matrix from Mendes et al. (2004). Exact DCluster is implemented separately.

Scientific basis: Mendes, Kennedy & Neves (2004), DOI `10.1109/TEVC.2004.826074`.

### Small World (Watts-Strogatz style) — `small-world-watts-strogatz`

Class: `WattsStrogatzSmallWorldTopology`

Default parameters: `NeighborhoodSize=4`, `RewiringProbability=0.1`, `IncludeSelf=true`.

The topology starts from a regular ring lattice. `NeighborhoodSize` must be positive, even, and smaller than the swarm size. Each eligible lattice edge is then rewired with probability `RewiringProbability` to a target not already connected to the source particle.

Its descriptor is **RandomStatic**: the randomized graph is sampled once per run.

This implementation is intentionally a static reusable Watts-Strogatz-style graph. It is **not** the complete adaptive SWPSO algorithm of Gong & Zhang (2013), whose topology parameters evolve during optimization.

Scientific basis: Gong & Zhang (2013), DOI `10.1145/2463372.2463381`.

### Scale Free (Barabasi-Albert style) — `scale-free-barabasi-albert`

Class: `BarabasiAlbertScaleFreeTopology`

Default parameters: `InitialCliqueSize=3`, `EdgesPerNewNode=2`, `IncludeSelf=true`.

A clique is created first. New particles are added sequentially and connect to distinct existing particles by preferential attachment. The selection weight is `max(1, degree)`, so higher-degree particles are more likely to receive new links.

Its descriptor is **RandomStatic**: the graph is sampled once per run.

This is a generic static scale-free communication graph. It is intentionally distinct from Zhang & Yi's complete SFIPSO method, which combines scale-free organization with additional fitness/spatial and fully-informed mechanisms.

Scientific basis: Zhang & Yi (2011), DOI `10.1016/j.ins.2011.02.026`.

### DCluster / Dynamic Cluster — `dcluster-exact`

Class: `DClusterTopology`

Parameter: `ClusterSize=p`, `IncludeSelf=true`.

This is the exact regular **DCluster** construction of El Dor et al. The swarm size must satisfy

\f[
N=p(p+1).
\f]

At each rebuild:

1. particles are ranked from **worst current fitness to best current fitness**;
2. the ranking is partitioned into `p+1` contiguous groups of `p` particles;
3. every group becomes a clique;
4. the first/worst group is the central cluster;
5. the `j`-th particle of the central cluster is linked to the worst particle of outer cluster `j`.

The descriptor is **FitnessDynamic** and requires `CurrentFitness`. `ParticleSwarmOptimizer` invalidates the communication graph after every completed iteration and rebuilds it before the next movement step from the new current-fitness ranking.

DCluster is not in `PsoTopologyCatalog.CreateDefaults()` because it requires an explicit `ClusterSize` and a compatible swarm size.

Scientific basis: El Dor, Lemoine, Clerc, Siarry, Deroussi & Gourgand (2015), *Dynamic cluster in particle swarm optimization algorithm*, DOI `10.1007/s11047-014-9465-2`.

### Custom Graph — `custom-graph`

Class: `CustomGraphTopology`

A caller supplies an immutable `NeighborhoodGraph`. Its node count must equal the swarm size. The graph is then reused unchanged.

This is an extension point rather than a published topology: it lets a research application inject any preconstructed communication structure while reusing the same PSO social policies and movement engine.

## Default built-in catalog

`PsoTopologyCatalog.CreateDefaults()` currently exposes eight ready-to-create topology instances:

1. Fully Connected
2. Ring
3. Hub-and-Spoke
4. Toroidal Von Neumann
5. Random Connected
6. General Clustered (`clusterCount=4`)
7. Watts-Strogatz-style Small World
8. Barabasi-Albert-style Scale Free

`DClusterTopology` and `CustomGraphTopology` are implemented but require caller-supplied problem-specific construction parameters and are therefore not part of this zero-argument/default factory list.

## Scientific references

- Kennedy & Eberhart (1995), *Particle Swarm Optimization*, DOI `10.1109/ICNN.1995.488968`.
- Kennedy & Mendes (2002), *Population Structure and Particle Swarm Performance*, DOI `10.1109/CEC.2002.1004493`.
- Mendes, Kennedy & Neves (2004), *The Fully Informed Particle Swarm: Simpler, Maybe Better*, DOI `10.1109/TEVC.2004.826074`.
- Zhang & Yi (2011), *Scale-free fully informed particle swarm optimization algorithm*, DOI `10.1016/j.ins.2011.02.026`.
- Gong & Zhang (2013), *Small-world particle swarm optimization with topology adaptation*, DOI `10.1145/2463372.2463381`.
- El Dor et al. (2015), *Dynamic cluster in particle swarm optimization algorithm*, DOI `10.1007/s11047-014-9465-2`.
