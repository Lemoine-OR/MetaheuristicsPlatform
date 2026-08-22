@page advanced_ant_colony_optimization Advanced Ant Colony Optimization

# Advanced Ant Colony Optimization

Version 0.45.0 promotes two major Ant System descendants to public executable algorithms:
Ant Colony System (ACS) and MAX-MIN Ant System (MMAS).

## Implemented ACS mechanisms

- `aco.transition.acs-pseudo-random-proportional`
- `aco.update.acs-local`
- `aco.update.acs-best-so-far`

The ACS transition rule is

\f[
j=
\begin{cases}
\arg\max_{u\in\mathcal N(s)} \tau_u\eta_u^\beta,&q\le q_0,\\
J,&q>q_0,
\end{cases}
\qquad
\Pr(J=u)=
\frac{\tau_u\eta_u^\beta}
{\sum_{v\in\mathcal N(s)}\tau_v\eta_v^\beta}.
\f]

After selecting component \f$e\f$, ACS applies the local update

\f[
\tau_e\leftarrow(1-\xi)\tau_e+\xi\tau_0.
\f]

## Implemented MMAS mechanisms

- `aco.update.mmas-best-only`
- `aco.memory.mmas-bounds`
- `aco.reinforcement.mmas-iteration-best`
- `aco.reinforcement.mmas-best-so-far`
- `aco.restart.mmas-stagnation`

MMAS restricts the trail domain:

\f[
\tau_{\min}\le\tau_e\le\tau_{\max}.
\f]

Only the configured best source reinforces trails after evaporation.

## Reviewed / deferred

Elitist Ant System and Rank-Based Ant System remain explicitly reviewed/deferred.
They are not approximated by overloading ACS or MMAS semantics.

## Scientific references

- Dorigo & Gambardella (1997), *Ant Colony System: A Cooperative Learning Approach to the Traveling Salesman Problem*, IEEE Transactions on Evolutionary Computation 1(1), 53-66. DOI: `10.1109/4235.585892`.
- Stutzle & Hoos (2000), *MAX-MIN Ant System*, Future Generation Computer Systems 16(8), 889-914. DOI: `10.1016/S0167-739X(00)00043-1`.
