namespace MetaheuristicsPlatform.Evaluation.Delegates;

public delegate TSolution SolutionDecoderDelegate<in TCandidate, out TSolution>(
    TCandidate candidate,
    CancellationToken cancellationToken);

/// <summary>
/// Compatibility delegate for mutable reference-type solutions.
/// </summary>
/// <remarks>
/// For value-type solutions use <see cref="RefSolutionMutationDelegate{TSolution}"/>
/// so changes propagate to the pipeline.
/// </remarks>
public delegate bool SolutionMutationDelegate<in TSolution>(
    TSolution solution,
    CancellationToken cancellationToken);

/// <summary>
/// Mutation delegate with correct replacement semantics for both classes and structs.
/// </summary>
public delegate bool RefSolutionMutationDelegate<TSolution>(
    ref TSolution solution,
    CancellationToken cancellationToken);

public delegate double SolutionEvaluatorDelegate<in TSolution>(
    TSolution solution,
    CancellationToken cancellationToken);

public delegate void LamarckianFeedbackDelegate<TCandidate, in TSolution>(
    TSolution solution,
    ref TCandidate candidate,
    CancellationToken cancellationToken);