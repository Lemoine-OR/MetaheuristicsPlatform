# Advanced Variable Neighborhood Search variants

MetaheuristicsPlatform v0.27.0 separates the main VNS variants according to the scientific mechanism they actually change.

| Variant | Status | Distinguishing mechanism |
|---|---|---|
| RVNS | Executable | Omits local improvement; shaking + neighborhood change only |
| GVNS | Executable | Uses VND as the improvement phase |
| SVNS | Executable | Relaxes incumbent recentering with a distance-skewed acceptance rule |
| VNDS | Reviewed / deferred | Changes the size of decomposed subproblems and solves reduced spaces with VNS |

VNDS is intentionally not exposed as a public executable algorithm in v0.27.0. A faithful generic implementation requires a first-class decomposition/subproblem contract so that the search can operate in reduced solution spaces; mapping VNDS onto an ordinary perturbation would misrepresent the method.

Scientific references:

- Hansen & Mladenovic (2001), DOI `10.1016/S0377-2217(00)00100-4`.
- Hansen, Mladenovic, Todosijevic & Hanafi (2017), DOI `10.1007/s13675-016-0075-x`.
