namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Stable scientific component identifiers for CMA-ES.</summary>
public static class CmaEsComponentIds
{
    public const string MultivariateNormalSampling =
        "cma.sampling.multivariate-normal";

    public const string LogarithmicRecombination =
        "cma.recombination.logarithmic-positive";

    public const string EvolutionPathCumulation =
        "cma.path.cumulation";

    public const string CumulativeStepSizeAdaptation =
        "cma.step-size.csa";

    public const string RankOneCovarianceUpdate =
        "cma.covariance.rank-one";

    public const string RankMuCovarianceUpdate =
        "cma.covariance.rank-mu";

    public const string ActiveCovarianceAdaptation =
        "cma.covariance.active";

    public const string SeparableCmaEs =
        "cma.variant.separable";

    public const string IpopRestart =
        "cma.restart.ipop";

    public const string BipopRestart =
        "cma.restart.bipop";
}
