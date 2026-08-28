using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Constraints.StochasticRankingEs;
public sealed class StochasticRankingEsParameters : IMetaheuristicParameters
{
 public int Mu { get; init; }=20; public int Lambda { get; init; }=100; public int MaximumGenerations { get; init; }=150; public double ProbabilityObjective { get; init; }=0.45; public double InitialSigma { get; init; }=0.2; public double SigmaDecay { get; init; }=0.99; public double MinimumSigma { get; init; }=1e-5;
 public void Validate(){if(Mu<2)throw new ArgumentOutOfRangeException(nameof(Mu));if(Lambda<Mu)throw new ArgumentOutOfRangeException(nameof(Lambda));if(MaximumGenerations<=0)throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));if(!double.IsFinite(ProbabilityObjective)||ProbabilityObjective<0.0||ProbabilityObjective>1.0)throw new ArgumentOutOfRangeException(nameof(ProbabilityObjective));if(!double.IsFinite(InitialSigma)||InitialSigma<=0.0)throw new ArgumentOutOfRangeException(nameof(InitialSigma));if(!double.IsFinite(SigmaDecay)||SigmaDecay<=0.0||SigmaDecay>1.0)throw new ArgumentOutOfRangeException(nameof(SigmaDecay));if(!double.IsFinite(MinimumSigma)||MinimumSigma<=0.0)throw new ArgumentOutOfRangeException(nameof(MinimumSigma));}
}
