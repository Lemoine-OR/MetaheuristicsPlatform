namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>Inheritance semantics after individual local learning.</summary>
public enum MemeticLearningMode
{
    Lamarckian = 0,
    Baldwinian = 1
}

/// <summary>Information exposed to the learning policy after local search.</summary>
public readonly record struct MemeticLearningContext(
    double GenotypeObjective,
    double PhenotypeObjective,
    bool Improved);

/// <summary>
/// Determines whether the locally improved phenotype is inherited and which objective
/// value participates in evolutionary selection.
/// </summary>
public readonly record struct MemeticLearningDecision(
    bool InheritImprovedPhenotype,
    double SelectionObjective);

public interface IMemeticLearningPolicy
{
    string Id { get; }

    MemeticLearningMode Mode { get; }

    MemeticLearningDecision Decide(
        in MemeticLearningContext context);
}

/// <summary>
/// Lamarckian learning: the locally improved phenotype replaces the inherited genotype.
/// </summary>
public sealed class LamarckianMemeticLearningPolicy :
    IMemeticLearningPolicy
{
    public string Id =>
        MemeticAlgorithmComponentIds.LamarckianLearning;

    public MemeticLearningMode Mode =>
        MemeticLearningMode.Lamarckian;

    public MemeticLearningDecision Decide(
        in MemeticLearningContext context) =>
        new(
            InheritImprovedPhenotype: true,
            SelectionObjective: context.PhenotypeObjective);
}

/// <summary>
/// Baldwinian learning: the genotype is retained while selection observes the learned
/// phenotype objective.
/// </summary>
public sealed class BaldwinianMemeticLearningPolicy :
    IMemeticLearningPolicy
{
    public string Id =>
        MemeticAlgorithmComponentIds.BaldwinianLearning;

    public MemeticLearningMode Mode =>
        MemeticLearningMode.Baldwinian;

    public MemeticLearningDecision Decide(
        in MemeticLearningContext context) =>
        new(
            InheritImprovedPhenotype: false,
            SelectionObjective: context.PhenotypeObjective);
}
