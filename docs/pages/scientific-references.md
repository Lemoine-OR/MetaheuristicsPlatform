@page scientific_references Scientific references

# Scientific references

Every literature-derived implementation must cite the publication that defines the
implemented mechanism and include a DOI when one exists. Algorithm pages are the
authoritative location for method-specific provenance.

## Threshold Accepting

- Dueck, G.; Scheuer, T. (1990). *Threshold accepting: A general purpose optimization
  algorithm appearing superior to simulated annealing*. Journal of Computational Physics
  90(1), 161-175. DOI: `10.1016/0021-9991(90)90201-B`.
- Winker, P.; Fang, K.-T. (1997). *Application of Threshold-Accepting to the Evaluation
  of the Discrepancy of a Set of Points*. SIAM Journal on Numerical Analysis 34(5),
  2028-2042. DOI: `10.1137/S0036142995286076`.
- Hu, T. C.; Kahng, A. B.; Tsao, C.-W. A. (1995). *Old Bachelor Acceptance: A New Class
  of Non-Monotone Threshold Accepting Methods*. ORSA Journal on Computing 7(4), 417-425.
  DOI: `10.1287/ijoc.7.4.417`.
## Great Deluge and Record-to-Record Travel

- Dueck, G. (1993). *New Optimization Heuristics: The Great Deluge Algorithm and the Record-to-Record Travel*.
  DOI: `10.1006/jcph.1993.1010`.
- Burke, E.; Bykov, Y.; Newall, J.; Petrovic, S. (2003). *A Time-Predefined Approach to Course Timetabling*.
  DOI: `10.2298/YJOR0302139B`.
- Burke, E. K.; Bykov, Y. (2016). *An Adaptive Flex-Deluge Approach to University Exam Timetabling*.
  DOI: `10.1287/ijoc.2015.0680`.

## Late Acceptance and Demon references

- Burke, E. K.; Bykov, Y. (2017), *The late acceptance Hill-Climbing heuristic*,
  European Journal of Operational Research 258(1), 70-78.
  DOI `10.1016/j.ejor.2016.07.012`.
- Zimmermann, T.; Salamon, P. (1992), *The demon algorithm*,
  International Journal of Computer Mathematics 42(1-2), 21-31.
  DOI `10.1080/00207169208804047`.

## Demon-based acceptance

- Creutz, M. (1983), *Microcanonical Monte Carlo Simulation*, Physical Review Letters 50(19), 1411-1414. DOI `10.1103/PhysRevLett.50.1411`.
- Talbi, E.-G. (2009), *Single-Solution Based Metaheuristics*, in *Metaheuristics: From Design to Implementation*, Chapter 2. DOI `10.1002/9780470496916.ch2`.
- Wood, I. A.; Downs, T. (1998), *Demon algorithms and their application to optimization problems*, IEEE WCCI / IJCNN, 1661-1666.

The v0.36.0 implementation is the one-point conserved credit/energy controller. The Zimmermann-Salamon (1992) ensemble Demon Algorithm and later ILS credit-reset Demon-like criteria remain scientifically distinct.

## Iterated Greedy

- Ruiz, R.; Stützle, T. (2007), *A simple and effective iterated greedy algorithm for the permutation flowshop scheduling problem*, European Journal of Operational Research 177(3), 2033-2049. DOI `10.1016/j.ejor.2005.12.009`.
- Stützle, T.; Ruiz, R. (2025), *Iterated Greedy*, in *Handbook of Heuristics*, 745-777. DOI `10.1007/978-3-032-00385-0_10`.
- Ruiz, R.; Pan, Q.-K.; Naderi, B. (2019), *Iterated Greedy methods for the distributed permutation flowshop scheduling problem*, Omega 83, 213-222. DOI `10.1016/j.omega.2018.03.004`.
- *Iterated reference greedy algorithm for solving distributed no-idle permutation flowshop scheduling problems* (2017), Computers & Industrial Engineering 110, 413-423. DOI `10.1016/j.cie.2017.06.025`.
- *An effective Iterated Greedy algorithm for the distributed permutation flowshop scheduling with due windows* (2020), Applied Soft Computing 96, 106629. DOI `10.1016/j.asoc.2020.106629`.

v0.37.0 implements the generic destruction/reconstruction core with optional local search and pluggable acceptance. Advanced two-stage, reference-based and adaptive-destruction variants are reserved for the reviewed v0.38.0 line.
