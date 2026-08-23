@page advanced_adaptive_large_neighborhood_search_components Advanced Adaptive Large Neighborhood Search Components

# Advanced Adaptive Large Neighborhood Search Components

v0.54 extends the public v0.53 ALNS implementation without creating a fabricated second
algorithm identity. The stable algorithm ID remains
`adaptive-large-neighborhood-search-ropke-pisinger-2006`; this page documents optional
selection and acceptance components that can be injected into that optimizer.

## Implemented advanced selection strategies

### Pair-coupled segmented roulette

The canonical v0.53 controller learns destroy and repair weights independently. The
pair-coupled strategy instead assigns a weight to each ordered pair \f$(d,r)\f$:

\f[
\begin{aligned}
P(d,r)
&=\frac{w_{dr}}{\sum_{i\in\mathcal D}\sum_{j\in\mathcal R}w_{ij}},\\
w_{dr}^{s+1}
&=
\begin{cases}
(1-r)w_{dr}^{s}+r\,\dfrac{\pi_{dr}}{\theta_{dr}},&\theta_{dr}>0,\\
w_{dr}^{s},&\theta_{dr}=0.
\end{cases}
\end{aligned}
\f]

This is useful when the quality of a destroy heuristic depends strongly on the repair heuristic
with which it is combined. Pair-level scoring is documented in application literature such as
Sarasola et al. (2020), DOI `10.1002/net.21905`.

### Alpha-UCB operator-pair selection

`AlphaUcbOperatorPairSelectionStrategy` treats each destroy/repair pair as a bandit action.
Unseen pairs are explored before the UCB score is used:

\f[
\begin{aligned}
Q_a(t)
&=\bar r_a(t-1)+
\sqrt{\frac{\alpha\ln(1+t)}{T_a(t-1)}},\\
a_t
&\in\operatorname*{argmax}_{a\in\mathcal A}Q_a(t).
\end{aligned}
\f]

The implementation follows the ALNS-specific bandit selection described by Hendel (2022),
DOI `10.1007/s12532-021-00209-7`. The default \f$\alpha=0.05\f$ is intentionally modest and
remains configurable.

## Implemented advanced acceptance composition

`TrajectoryAcceptanceLargeNeighborhoodAdapter` maps the existing generic trajectory
acceptance context directly to the LNS/ALNS acceptance context. This makes the platform's
existing acceptance rules reusable rather than reimplemented.

### Threshold Accepting

`AdvancedAdaptiveLargeNeighborhoodAcceptance.Threshold(tau)` composes the existing
`ThresholdAcceptancePolicy`:

\f[
\begin{aligned}
\Delta(y\mid x)
&=\operatorname{degradation}(x,y),\\
A_{TA}(x,y)
&=\mathbf 1[\Delta(y\mid x)\le\tau].
\end{aligned}
\f]

### Record-to-Record Travel

`AdvancedAdaptiveLargeNeighborhoodAcceptance.RecordToRecordTravel(delta)` composes the
existing best-record policy:

\f[
\begin{aligned}
\Delta_B(y)
&=\operatorname{degradation}(x^{best},y),\\
A_{RTR}(y)
&=\mathbf 1[\Delta_B(y)\le\delta].
\end{aligned}
\f]

Santini, Ropke & Hvattum (2018), DOI `10.1007/s10732-018-9377-x`, compare ALNS acceptance
criteria and report strong variants based on simulated annealing, threshold acceptance and
record-to-record travel. The canonical v0.53 default remains simulated annealing.

## API examples

Pair-coupled selection:

```csharp
var optimizer =
    new AdaptiveLargeNeighborhoodSearchOptimizer<MySolution,RemovedSet>(
        initial,
        destroyOperators,
        repairOperators,
        solutionComparer,
        acceptanceOverride: null,
        selectionStrategy:
            new PairCoupledSegmentedRouletteOperatorSelectionStrategy());
```

Alpha-UCB plus Record-to-Record Travel:

```csharp
var optimizer =
    new AdaptiveLargeNeighborhoodSearchOptimizer<MySolution,RemovedSet>(
        initial,
        destroyOperators,
        repairOperators,
        solutionComparer,
        AdvancedAdaptiveLargeNeighborhoodAcceptance.RecordToRecordTravel(5.0),
        new AlphaUcbOperatorPairSelectionStrategy(alpha: 0.05));
```

## Reviewed but deferred

### Contextual bandits

A contextual selector requires a stable generic state-to-context-vector contract. That
abstraction is deliberately not guessed in v0.54.

### Scheduled Great Deluge

Great Deluge carries evolving water-level state. A correct generic ALNS integration therefore
needs a run-local acceptance-session lifecycle rather than a shared mutable policy object.
v0.54 keeps this extension deferred instead of hiding unsafe state.

## Scientific references

- Sarasola et al. (2020), DOI `10.1002/net.21905`.
- Hendel (2022), DOI `10.1007/s12532-021-00209-7`.
- Santini, Ropke & Hvattum (2018), DOI `10.1007/s10732-018-9377-x`.
- Ropke & Pisinger (2006), DOI `10.1287/trsc.1050.0135`.
