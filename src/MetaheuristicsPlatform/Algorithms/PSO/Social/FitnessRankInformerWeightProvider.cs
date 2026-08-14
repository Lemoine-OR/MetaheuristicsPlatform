using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Algorithms.PSO.Social;

/// <summary>
/// Generic rank-based experimental informer weighting.
/// Better personal-best fitness receives a larger integer weight.
/// </summary>
/// <remarks>
/// This is a platform utility and is not claimed to reproduce SFIPSO or another
/// named published PSO variant.
/// </remarks>
public sealed class FitnessRankInformerWeightProvider : IInformerWeightProvider
{
    public void GetWeights(
        int particle,
        PsoSocialContext context,
        ReadOnlySpan<int> informers,
        Span<double> weights)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (weights.Length != informers.Length)
        {
            throw new ArgumentException(
                "Weight buffer length must equal informer count.",
                nameof(weights));
        }

        if (informers.IsEmpty)
        {
            return;
        }

        Span<int> order =
            informers.Length <= 128
                ? stackalloc int[informers.Length]
                : new int[informers.Length];

        for (int i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        // Stable insertion sort avoids capturing ReadOnlySpan<int> in a lambda
        // and keeps the usual PSO neighborhood sizes allocation-free.
        for (int i = 1; i < order.Length; i++)
        {
            int key = order[i];
            int j = i - 1;

            while (j >= 0 &&
                   ComesBefore(
                       key,
                       order[j],
                       informers,
                       context))
            {
                order[j + 1] = order[j];
                j--;
            }

            order[j + 1] = key;
        }

        weights.Clear();

        for (int rank = 0; rank < order.Length; rank++)
        {
            weights[order[rank]] =
                order.Length - rank;
        }
    }

    private static bool ComesBefore(
        int leftOrderIndex,
        int rightOrderIndex,
        ReadOnlySpan<int> informers,
        PsoSocialContext context)
    {
        int leftInformer = informers[leftOrderIndex];
        int rightInformer = informers[rightOrderIndex];

        double leftFitness =
            context.GetPersonalBestFitness(leftInformer);

        double rightFitness =
            context.GetPersonalBestFitness(rightInformer);

        if (context.Sense.IsBetter(
            leftFitness,
            rightFitness))
        {
            return true;
        }

        if (context.Sense.IsBetter(
            rightFitness,
            leftFitness))
        {
            return false;
        }

        return leftInformer < rightInformer;
    }
}