namespace MetaheuristicsPlatform.Neighborhoods;

/// <summary>
/// Optional applicability predicate for neighborhoods that can cheaply reject a move
/// before application or evaluation.
/// </summary>
public interface IMoveApplicability<TSolution, TMove>
{
    bool IsApplicable(
        in TSolution solution,
        in TMove move);
}