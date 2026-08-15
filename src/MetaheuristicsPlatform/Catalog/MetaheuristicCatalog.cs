namespace MetaheuristicsPlatform.Catalog;

public static class MetaheuristicCatalog
{
    private static readonly MetaheuristicCatalogEntry[] Entries =
    [
        new(
            "particle-swarm",
            "Particle Swarm Optimization",
            "ParticleSwarmOptimizer",
            "swarm-intelligence",
            "Swarm intelligence",
            "O(ND) per iteration for the canonical graphless fast path; topology/social policies may add overhead",
            "O(ND)",
            "Continuous bounded search spaces; generic platform infrastructure also supports alternative social/topology policies",
            false,
            "src/MetaheuristicsPlatform/Algorithms/PSO/ParticleSwarmOptimizer.cs",
            "Kennedy & Eberhart (1995), Particle Swarm Optimization, IEEE ICNN; Clerc & Kennedy (2002), The particle swarm — explosion, stability, and convergence in a multidimensional complex space, IEEE TEC 6(1), 58–73",
            "10.1109/4235.985692",
            "Flat particle-major buffers, deterministic target-owned RNG streams, graphless fully-connected canonical fast path, calibrated movement/evaluation parallelism."),
        new(
            "differential-evolution",
            "Differential Evolution",
            "DifferentialEvolutionOptimizer",
            "evolutionary-methods",
            "Evolutionary methods",
            "O(ND) per generation for classical mutation/crossover, plus objective-evaluation cost",
            "O(ND)",
            "Continuous bounded search spaces",
            false,
            "src/MetaheuristicsPlatform/Algorithms/DE/DifferentialEvolutionOptimizer.cs",
            "Storn & Price (1997), Differential Evolution — A Simple and Efficient Heuristic for Global Optimization over Continuous Spaces, Journal of Global Optimization 11(4), 341–359",
            "10.1023/A:1008202821328",
            "Flat parent/trial buffers; classical mutation and crossover strategies; deterministic per-target RNG; calibrated variation and independent evaluation parallelism."),
        new(
            "jde-brest-2006",
            "jDE — Self-Adaptive Differential Evolution",
            "SelfAdaptiveDifferentialEvolutionOptimizer",
            "evolutionary-methods",
            "Evolutionary methods",
            "O(ND) per generation plus objective-evaluation cost",
            "O(ND + N)",
            "Continuous bounded search spaces; canonical DE/rand/1/bin self-adaptation",
            false,
            "src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/SelfAdaptiveDifferentialEvolutionOptimizer.cs",
            "Brest et al. (2006), Self-Adapting Control Parameters in Differential Evolution: A Comparative Study on Numerical Benchmark Problems, IEEE TEC 10(6), 646–657",
            "10.1109/TEVC.2006.872133",
            "Per-individual inherited F_i/CR_i proposals; trial parameters are committed only after strict successful selection."),
        new(
            "jade-2009",
            "JADE",
            "JadeOptimizer",
            "evolutionary-methods",
            "Evolutionary methods",
            "O(ND + N log N) per generation plus objective-evaluation cost",
            "O(ND) population plus O(ND) optional archive",
            "Continuous bounded search spaces",
            false,
            "src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/JadeOptimizer.cs",
            "Zhang & Sanderson (2009), JADE: Adaptive Differential Evolution With Optional External Archive, IEEE TEC 13(5), 945–958",
            "10.1109/TEVC.2009.2014613",
            "current-to-pbest/1/bin, optional external archive, Cauchy F sampling, normal CR sampling and success-mean adaptation."),
        new(
            "shade-2013",
            "SHADE",
            "ShadeOptimizer",
            "evolutionary-methods",
            "Evolutionary methods",
            "O(ND + N log N) per generation plus objective-evaluation cost",
            "O(ND + H)",
            "Continuous bounded search spaces",
            false,
            "src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/ShadeOptimizer.cs",
            "Tanabe & Fukunaga (2013), Success-History Based Parameter Adaptation for Differential Evolution, IEEE CEC, 71–78",
            "10.1109/CEC.2013.6557555",
            "Historical memories M_F/M_CR, random memory slot per target, improvement-weighted success learning and external archive."),
        new(
            "lshade-2014",
            "L-SHADE",
            "LShadeOptimizer",
            "evolutionary-methods",
            "Evolutionary methods",
            "O(N_kD + N_k log N_k) at generation k plus objective-evaluation cost",
            "O(N_init D + H)",
            "Continuous bounded search spaces with an evaluation budget driving LPSR",
            false,
            "src/MetaheuristicsPlatform/Algorithms/DE/Adaptive/LShadeOptimizer.cs",
            "Tanabe & Fukunaga (2014), Improving the Search Performance of SHADE Using Linear Population Size Reduction, IEEE CEC, 1658–1665",
            "10.1109/CEC.2014.6900380",
            "SHADE 1.1 success-history semantics with linear population-size reduction, fixed physical capacity and shrinking active prefix."),
        new(
            "simulated-annealing-metropolis",
            "Simulated Annealing",
            "SimulatedAnnealingOptimizer<TSolution,TMove,TUndo>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(C_move + C_eval) per attempted transition; O(C_delta) when an exact differential evaluator is available",
            "O(|solution| + |move| + |undo|); no mandatory per-transition solution clone on the reversible path",
            "Any solution representation admitting a stochastic neighborhood and reversible move operator; exact delta evaluation is optional",
            true,
            "src/MetaheuristicsPlatform/Algorithms/SA/SimulatedAnnealingOptimizer.cs",
            "Metropolis et al. (1953), Journal of Chemical Physics 21(6), 1087–1092; Kirkpatrick, Gelatt & Vecchi (1983), Science 220(4598), 671–680",
            "10.1126/science.220.4598.671",
            "Generic reversible trajectory engine, Metropolis acceptance, pluggable cooling schedules, exact-delta fast path and common OptimizationContext lifecycle."),
        new(
            "tabu-search-glover",
            "Tabu Search",
            "TabuSearchOptimizer<TSolution,TMove,TUndo,TAttribute,TEnumerator>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(|N(x)| * C_delta + log M) per iteration with exact deltas; otherwise O(|N(x)| * (C_move + C_eval + C_undo) + log M)",
            "O(|solution| + M) for retained short-term tabu records",
            "Any finite enumerated neighborhood with reversible moves and domain-defined tabu attributes",
            true,
            "src/MetaheuristicsPlatform/Algorithms/TS/TabuSearchOptimizer.cs",
            "Glover (1989), Tabu Search-Part I, ORSA Journal on Computing 1(3), 190-206; Glover (1990), Tabu Search-Part II, ORSA Journal on Computing 2(1), 4-32",
            "10.1287/ijoc.1.3.190",
            "Allocation-free neighborhood scan, attribute-based expiration memory, best-so-far aspiration, configurable tenure, exact-delta fast path and reversible full-evaluation fallback."),
        new(
            "reactive-tabu-search-battiti-tecchiolli-1994",
            "Reactive Tabu Search",
            "ReactiveTabuSearchOptimizer<TSolution,TMove,TUndo,TAttribute,TEnumerator>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(|N(x)| * C_delta + log M_s) per normal iteration with exact deltas; escape steps reservoir-scan N(x) and evaluate one selected move",
            "O(|solution| + M_s + M_f + M_r) for short-term, frequency and repetition memories",
            "Finite enumerated neighborhoods with reversible moves, domain-defined tabu attributes and a stable configuration signature",
            true,
            "src/MetaheuristicsPlatform/Algorithms/TS/ReactiveTabuSearchOptimizer.cs",
            "Battiti & Tecchiolli (1994), The Reactive Tabu Search, ORSA Journal on Computing 6(2), 126-140; Glover (1989, 1990)",
            "10.1287/ijoc.6.2.126",
            "Explicit repetition detection, feedback tabu tenure, cycle-length moving average, reactive random-walk escape, optional frequency diversification, optional elite intensification, exact-delta fast path and common OptimizationContext lifecycle."),
        new(
            "local-search-best-improvement",
            "Local Search — Best Improvement",
            "BestImprovementLocalSearchOptimizer<TSolution,TMove,TUndo,TEnumerator>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(|N(x)| C_delta) per descent step with exact deltas; reversible full evaluation otherwise",
            "O(|solution|)",
            "Finite enumerated neighborhoods with reversible moves",
            true,
            "src/MetaheuristicsPlatform/Algorithms/Neighborhood/LocalSearchOptimizers.cs",
            "Talbi (2009), Metaheuristics: From Design to Implementation",
            "10.1002/9780470496916",
            "Steepest-descent best-improvement scan with allocation-free neighborhood cursor, exact-delta fast path and reversible fallback."),
        new(
            "local-search-first-improvement",
            "Local Search — First Improvement",
            "FirstImprovementLocalSearchOptimizer<TSolution,TMove,TUndo,TEnumerator>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(q C_delta) per accepted move, where q is the number of candidates scanned until first improvement",
            "O(|solution|)",
            "Finite ordered neighborhoods with reversible moves",
            true,
            "src/MetaheuristicsPlatform/Algorithms/Neighborhood/LocalSearchOptimizers.cs",
            "Talbi (2009), Metaheuristics: From Design to Implementation",
            "10.1002/9780470496916",
            "First-descent scan that stops immediately at the first strict improving move."),
        new(
            "multi-start-local-search",
            "Multi-Start Local Search",
            "MultiStartLocalSearchOptimizer<TSolution>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(S * (C_init + C_LS)) for S starts; C_LS depends on the composed local search",
            "O(|solution| + local-search workspace)",
            "Any representation with a start generator and compatible reusable local-search procedure",
            true,
            "src/MetaheuristicsPlatform/Algorithms/Neighborhood/RestartIteratedLocalSearchOptimizers.cs",
            "Marti (2003), Multi-Start Methods, Handbook of Metaheuristics, 355-368; Talbi (2009)",
            "10.1007/0-306-48056-5_12",
            "Sequential restart composition reusing the v0.23 local-search engine under one exact OptimizationContext lifecycle."),
        new(
            "iterated-local-search-lourenco-martin-stutzle",
            "Iterated Local Search - Lourenco-Martin-Stutzle",
            "IteratedLocalSearchOptimizer<TSolution>",
            "trajectory-based-methods",
            "Trajectory-based methods",
            "O(C_LS0 + sum_k(C_perturb,k + C_eval,k + C_LS,k))",
            "O(|solution| + local-search workspace)",
            "Any representation with a reusable local search, owned solution cloning and a domain-defined perturbation",
            true,
            "src/MetaheuristicsPlatform/Algorithms/Neighborhood/RestartIteratedLocalSearchOptimizers.cs",
            "Lourenco, Martin & Stutzle (2003), Iterated Local Search, Handbook of Metaheuristics, 320-353; Talbi (2009)",
            "10.1007/0-306-48056-5_11",
            "Canonical initial-local-search / perturb / local-search / accept framework with independent best-so-far ownership.")
    ];

    private static readonly IReadOnlyDictionary<string, MetaheuristicCatalogEntry>
        ById =
        Entries.ToDictionary(
            static entry => entry.Id,
            StringComparer.Ordinal);

    public static IReadOnlyList<MetaheuristicCatalogEntry> All =>
        Entries;

    public static bool TryGet(
        string id,
        out MetaheuristicCatalogEntry? entry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return ById.TryGetValue(
            id,
            out entry);
    }

    public static MetaheuristicCatalogEntry GetRequired(
        string id)
    {
        if (!TryGet(
                id,
                out MetaheuristicCatalogEntry? entry))
        {
            throw new KeyNotFoundException(
                $"Unknown metaheuristic algorithm id '{id}'.");
        }

        return entry!;
    }
}
