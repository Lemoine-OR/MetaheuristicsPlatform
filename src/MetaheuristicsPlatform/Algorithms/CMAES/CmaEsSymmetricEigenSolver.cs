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

        const int maximumStackDimension = 32;
        bool useStack = dimension <= maximumStackDimension;
        Span<double> a = useStack
            ? stackalloc double[matrix.Length]
            : matrix.ToArray();
        if (useStack)
            matrix.CopyTo(a);

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
                int pOffset = p * dimension;
                int pp = pOffset + p;

                for (int q = p + 1; q < dimension; q++)
                {
                    int qOffset = q * dimension;
                    int qq = qOffset + q;
                    int pq = pOffset + q;
                    double apq = a[pq];

                    offDiagonalMax =
                        Math.Max(
                            offDiagonalMax,
                            Math.Abs(apq));

                    double app = a[pp];
                    double aqq = a[qq];

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

                    a[pp] = newApp;
                    a[qq] = newAqq;

                    a[pq] = 0.0;
                    a[qOffset + p] = 0.0;

                    for (int r = 0, rowOffset = 0;
                         r < dimension;
                         r++, rowOffset += dimension)
                    {
                        if (r == p || r == q)
                        {
                            continue;
                        }

                        int rp = rowOffset + p;
                        int rq = rowOffset + q;
                        double arp = a[rp];
                        double arq = a[rq];

                        double rotatedP =
                            (c * arp) - (s * arq);

                        double rotatedQ =
                            (s * arp) + (c * arq);

                        a[rp] = rotatedP;
                        a[pOffset + r] = rotatedP;
                        a[rq] = rotatedQ;
                        a[qOffset + r] = rotatedQ;
                    }

                    for (int r = 0, rowOffset = 0;
                         r < dimension;
                         r++, rowOffset += dimension)
                    {
                        int rp = rowOffset + p;
                        int rq = rowOffset + q;
                        double vrp = eigenvectors[rp];
                        double vrq = eigenvectors[rq];

                        eigenvectors[rp] = (c * vrp) - (s * vrq);
                        eigenvectors[rq] = (s * vrp) + (c * vrq);
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
