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
- Ying, K.-C.; Lin, S.-W.; Cheng, C.-Y.; He, C.-D. (2017), *Iterated reference greedy algorithm for solving distributed no-idle permutation flowshop scheduling problems*, Computers & Industrial Engineering 110, 413-423. DOI `10.1016/j.cie.2017.06.025`.
- Jing, X.-L.; Pan, Q.-K.; Gao, L.; Wang, Y.-L. (2020), *An effective Iterated Greedy algorithm for the distributed permutation flowshop scheduling with due windows*, Applied Soft Computing 96, 106629. DOI `10.1016/j.asoc.2020.106629`.

v0.37.0 implements the generic destruction/reconstruction core with optional local search and pluggable acceptance. Advanced two-stage, reference-based and adaptive-destruction variants are reserved for the reviewed v0.38.0 line.

### Advanced Iterated Greedy additions in v0.38.0

- Fernandez-Viagas, V.; Framinan, J. M. (2015), *A bounded-search iterated greedy algorithm for the distributed permutation flowshop scheduling problem*. DOI `10.1080/00207543.2014.948578`.
- Ding, J.-Y.; Song, S.; Gupta, J. N. D.; Zhang, R.; Chiong, R.; Wu, C. (2015), *An improved iterated greedy algorithm with a Tabu-based reconstruction strategy for the no-wait flowshop scheduling problem*. DOI `10.1016/j.asoc.2015.02.006`.
- Dubois-Lacoste, J.; Pagnozzi, F.; Stützle, T. (2017), *An iterated greedy algorithm with optimization of partial solutions for the makespan permutation flowshop problem*. DOI `10.1016/j.cor.2016.12.021`.
- Fernandez-Viagas, V.; Framinan, J. M. (2019), *A best-of-breed iterated greedy for the permutation flowshop scheduling problem with makespan objective*. DOI `10.1016/j.cor.2019.104767`.
- Li, Y.-Z.; Pan, Q.-K.; Li, J.-Q.; Gao, L.; Tasgetiren, M. F. (2021), *An Adaptive Iterated Greedy algorithm for distributed mixed no-idle permutation flowshop scheduling problems*. DOI `10.1016/j.swevo.2021.100874`.
- Zhang, S.; Qian, B.; Hu, R.; Li, K.; Yang, J.-B. (2026), *A two-stage iterated greedy algorithm for distributed blocking flowshop scheduling problem*. DOI `10.1016/j.eswa.2025.130422`.
### Scatter Search

- Martí, R.; Laguna, M.; Glover, F. (2006), *Principles of scatter search*, European Journal of Operational Research 169(2), 359-372. DOI `10.1016/j.ejor.2004.08.004`.
- Laguna, M.; Martí, R. (2003), *Scatter Search: Methodology and Implementations in C*. DOI `10.1007/978-1-4615-0337-8`.
- Glover, F.; Laguna, M.; Martí, R. (2004), *Scatter Search and Path Relinking: Foundations and Advanced Designs*. DOI `10.1007/978-3-540-39930-8_4`.
## Genetic Algorithm

- Eiben, A. E.; Smith, J. E. (2003), *Genetic Algorithms*, in *Introduction to Evolutionary Computing*, 37-69. DOI `10.1007/978-3-662-05094-1_3`.
- Whitley, D. (1994), *A genetic algorithm tutorial*, *Statistics and Computing* 4(2), 65-85. DOI `10.1007/BF00175354`.
- Blickle, T.; Thiele, L. (1996), *A Comparison of Selection Schemes used in Evolutionary Algorithms*, *Evolutionary Computation* 4(4), 361-394. DOI `10.1162/EVCO.1996.4.4.361`.

v0.41.0 implements a representation-independent fixed-size generational GA foundation. v0.42.0 adds the audited `ga.*` operator catalog without adding public algorithm IDs.

### Advanced Genetic Algorithm additions in v0.42.0

- Goldberg, D. E.; Deb, K. (1991), *A Comparative Analysis of Selection Schemes Used in Genetic Algorithms*. DOI `10.1016/B978-0-08-050684-5.50008-2`.
- Syswerda, G. (1989), *Uniform Crossover in Genetic Algorithms*, ICGA 1989, 2-9. DOI `10.5555/645512.657265`.
- Syswerda, G. (1991), *A Study of Reproduction in Generational and Steady-State Genetic Algorithms*. DOI `10.1016/B978-0-08-050684-5.50009-4`.
- Goldberg, D. E.; Lingle, R. (1985), *Alleles, Loci, and the Traveling Salesman Problem*. DOI `10.5555/645511.657095`.
- Davis, L. (1985), *Applying Adaptive Algorithms to Epistatic Domains*. DOI `10.5555/1625135.1625164`.
- Deb, K.; Agrawal, R. B. (1995), *Simulated Binary Crossover for Continuous Search Space*, Complex Systems 9(2), 115-148. No DOI asserted.
- Deb, K.; Pratap, A.; Agarwal, S.; Meyarivan, T. (2002), *A fast and elitist multiobjective genetic algorithm: NSGA-II*. DOI `10.1109/4235.996017`.
- Deb, K.; Deb, D. (2014), *Analysing mutation schemes for real-parameter genetic algorithms*. DOI `10.1504/IJAISC.2014.059280`.

## Memetic Algorithms

- Moscato, P. (1989). *On Evolution, Search, Optimization, Genetic Algorithms and Martial Arts: Towards Memetic Algorithms*. Caltech Concurrent Computation Program Report 826.
- Krasnogor, N.; Smith, J. E. (2005). *A Tutorial for Competent Memetic Algorithms: Model, Taxonomy, and Design Issues*. IEEE Transactions on Evolutionary Computation, 9(5), 474-488. DOI: `10.1109/TEVC.2005.850260`.
