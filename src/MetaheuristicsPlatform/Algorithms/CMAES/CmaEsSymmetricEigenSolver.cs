namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>
/// Small dependency-free Jacobi eigensolver for real symmetric matrices.
/// Eigenvectors are stored column-major inside a row-major square array:
/// element (row,column) is vectors[(row*n)+column].
/// </summary>
internal static class CmaEsSymmetricEigenSolver
{
    public static double Decompose(
        ReadOnlySpan<double> matrix,
        int dimension,
        double minimumEigenvalue,
        Span<double> eigenvectors,
        Span<double> axisScales)
    {
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dimension));
        }

        if (matrix.Length != dimension * dimension ||
            eigenvectors.Length != dimension * dimension ||
            axisScales.Length != dimension)
        {
            throw new ArgumentException(
                "CMA-ES eigensolver buffer dimensions are inconsistent.");
        }

        double[] a =
            matrix.ToArray();

        eigenvectors.Clear();

        for (int i = 0; i < dimension; i++)
        {
            eigenvectors[(i * dimension) + i] = 1.0;
        }

        int maximumSweeps =
            Math.Max(
                32,
                8 * dimension * dimension);

        for (int sweep = 0;
             sweep < maximumSweeps;
             sweep++)
        {
            double offDiagonalMax = 0.0;

            for (int p = 0; p < dimension - 1; p++)
            {
                for (int q = p + 1; q < dimension; q++)
                {
                    int pq = (p * dimension) + q;
                    double apq = a[pq];

                    offDiagonalMax =
                        Math.Max(
                            offDiagonalMax,
                            Math.Abs(apq));

                    double app =
                        a[(p * dimension) + p];

                    double aqq =
                        a[(q * dimension) + q];

                    double scale =
                        Math.Sqrt(
                            Math.Max(
                                minimumEigenvalue,
                                Math.Abs(app * aqq)));

                    if (Math.Abs(apq) <=
                        1e-14 * Math.Max(1.0, scale))
                    {
                        continue;
                    }

                    double tau =
                        (aqq - app) /
                        (2.0 * apq);

                    double t =
                        tau >= 0.0
                            ? 1.0 /
                              (tau +
                               Math.Sqrt(1.0 + (tau * tau)))
                            : -1.0 /
                              (-tau +
                               Math.Sqrt(1.0 + (tau * tau)));

                    double c =
                        1.0 /
                        Math.Sqrt(1.0 + (t * t));

                    double s =
                        t * c;

                    double newApp =
                        app - (t * apq);

                    double newAqq =
                        aqq + (t * apq);

                    a[(p * dimension) + p] =
                        newApp;

                    a[(q * dimension) + q] =
                        newAqq;

                    a[pq] = 0.0;
                    a[(q * dimension) + p] = 0.0;

                    for (int r = 0; r < dimension; r++)
                    {
                        if (r == p || r == q)
                        {
                            continue;
                        }

                        double arp =
                            a[(r * dimension) + p];

                        double arq =
                            a[(r * dimension) + q];

                        double rotatedP =
                            (c * arp) - (s * arq);

                        double rotatedQ =
                            (s * arp) + (c * arq);

                        a[(r * dimension) + p] =
                            rotatedP;

                        a[(p * dimension) + r] =
                            rotatedP;

                        a[(r * dimension) + q] =
                            rotatedQ;

                        a[(q * dimension) + r] =
                            rotatedQ;
                    }

                    for (int r = 0; r < dimension; r++)
                    {
                        double vrp =
                            eigenvectors[
                                (r * dimension) + p];

                        double vrq =
                            eigenvectors[
                                (r * dimension) + q];

                        eigenvectors[
                            (r * dimension) + p] =
                            (c * vrp) - (s * vrq);

                        eigenvectors[
                            (r * dimension) + q] =
                            (s * vrp) + (c * vrq);
                    }
                }
            }

            if (offDiagonalMax <= 1e-14)
            {
                break;
            }
        }

        double minEigenvalue =
            double.PositiveInfinity;

        double maxEigenvalue =
            0.0;

        for (int i = 0; i < dimension; i++)
        {
            double eigenvalue =
                a[(i * dimension) + i];

            if (!double.IsFinite(eigenvalue))
            {
                throw new InvalidOperationException(
                    "CMA-ES covariance eigendecomposition produced a non-finite eigenvalue.");
            }

            eigenvalue =
                Math.Max(
                    minimumEigenvalue,
                    eigenvalue);

            minEigenvalue =
                Math.Min(
                    minEigenvalue,
                    eigenvalue);

            maxEigenvalue =
                Math.Max(
                    maxEigenvalue,
                    eigenvalue);

            axisScales[i] =
                Math.Sqrt(eigenvalue);
        }

        return
            maxEigenvalue /
            minEigenvalue;
    }

    public static void ReconstructPositiveDefinite(
        ReadOnlySpan<double> eigenvectors,
        ReadOnlySpan<double> axisScales,
        Span<double> matrix)
    {
        int dimension =
            axisScales.Length;

        if (eigenvectors.Length != dimension * dimension ||
            matrix.Length != dimension * dimension)
        {
            throw new ArgumentException(
                "CMA-ES reconstruction buffer dimensions are inconsistent.");
        }

        matrix.Clear();

        for (int column = 0;
             column < dimension;
             column++)
        {
            double eigenvalue =
                axisScales[column] *
                axisScales[column];

            for (int row = 0;
                 row < dimension;
                 row++)
            {
                double vr =
                    eigenvectors[
                        (row * dimension) +
                        column];

                for (int other = 0;
                     other <= row;
                     other++)
                {
                    double value =
                        matrix[
                            (row * dimension) +
                            other] +
                        (eigenvalue *
                         vr *
                         eigenvectors[
                             (other * dimension) +
                             column]);

                    matrix[
                        (row * dimension) +
                        other] =
                        value;

                    matrix[
                        (other * dimension) +
                        row] =
                        value;
                }
            }
        }
    }

    public static void Transform(
        ReadOnlySpan<double> eigenvectors,
        ReadOnlySpan<double> axisScales,
        ReadOnlySpan<double> standardNormal,
        Span<double> destination)
    {
        int dimension =
            axisScales.Length;

        if (eigenvectors.Length != dimension * dimension ||
            standardNormal.Length != dimension ||
            destination.Length != dimension)
        {
            throw new ArgumentException(
                "CMA-ES transform buffer dimensions are inconsistent.");
        }

        destination.Clear();

        for (int column = 0;
             column < dimension;
             column++)
        {
            double scaled =
                axisScales[column] *
                standardNormal[column];

            for (int row = 0;
                 row < dimension;
                 row++)
            {
                destination[row] +=
                    eigenvectors[
                        (row * dimension) +
                        column] *
                    scaled;
            }
        }
    }

    public static void ApplyInverseSquareRoot(
        ReadOnlySpan<double> eigenvectors,
        ReadOnlySpan<double> axisScales,
        ReadOnlySpan<double> vector,
        Span<double> destination)
    {
        int dimension =
            axisScales.Length;

        if (eigenvectors.Length != dimension * dimension ||
            vector.Length != dimension ||
            destination.Length != dimension)
        {
            throw new ArgumentException(
                "CMA-ES inverse-square-root buffer dimensions are inconsistent.");
        }

        double[] projected =
            new double[dimension];

        for (int column = 0;
             column < dimension;
             column++)
        {
            double dot = 0.0;

            for (int row = 0;
                 row < dimension;
                 row++)
            {
                dot +=
                    eigenvectors[
                        (row * dimension) +
                        column] *
                    vector[row];
            }

            projected[column] =
                dot /
                axisScales[column];
        }

        destination.Clear();

        for (int column = 0;
             column < dimension;
             column++)
        {
            double coefficient =
                projected[column];

            for (int row = 0;
                 row < dimension;
                 row++)
            {
                destination[row] +=
                    eigenvectors[
                        (row * dimension) +
                        column] *
                    coefficient;
            }
        }
    }
}
