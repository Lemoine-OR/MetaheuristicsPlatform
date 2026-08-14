# Fully Informed Particle Swarm (FIPS)

## Reference

Rui Mendes, James Kennedy, José Neves,
"The Fully Informed Particle Swarm: Simpler, Maybe Better",
IEEE Transactions on Evolutionary Computation,
8(3), 204-210, 2004.
DOI: 10.1109/TEVC.2004.826074

## Core idea

Canonical PSO uses:
- the particle's own personal best;
- one selected social guide such as neighborhood best.

FIPS instead receives contributions from all topology-defined informers.

For a particle with `m` informers, the platform distributes a total acceleration
coefficient `phi` equally across those informers:

```text
attraction =
    Σ_j (phi / m) * r_j * (pBest_j - x)
```

with independently sampled random multipliers for informer/dimension contributions.

The topology remains independent:
- FullyConnected + FIPS;
- Ring + FIPS;
- VonNeumann + FIPS;
- DCluster + FIPS;
- future dynamic graphs + FIPS.

## Relationship to SFIPSO

Zhang & Yi (2011), DOI 10.1016/j.ins.2011.02.026, combine:
- a modified self-organizing scale-free construction;
- active and inactive particle subpopulations;
- degree, fitness and spatial information;
- contextual-fitness-dependent cognitive/social allocation;
- a time-varying weighted fully-informed mechanism.

Therefore:

`BarabasiAlbertScaleFreeTopology + WeightedFullyInformedInfluencePolicy`

is a useful generic experiment, but it is **not** labeled SFIPSO.

An exact SFIPSO preset will be added only when all coupled mechanisms are implemented.