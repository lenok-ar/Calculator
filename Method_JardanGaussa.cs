class Method_JardanGaussa
{
    private double[,] _martixA;
    private double[] _vectorB;

    public Method_JardanGaussa(double[,] martixA, double[] vectorB)
    {
        _martixA = martixA;
        _vectorB = vectorB;
    }

    public double[] Solve()
    {
        int n = _martixA.GetLength(0);

        double[,] matrix = new double[n, n + 1];

        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n; ++j)
            {
                matrix[i, j] = _martixA[i, j];
            }

            matrix[i, n] = -(_vectorB[i]);
        }

        for (int i = 0; i < n; ++i)
        {
            if (matrix[i, i] == 0)
            {
                for (int k = i + 1; k < n; ++k)
                {
                    if (matrix[k, i] != 0)
                    {
                        for (int j = 0; j <= n; ++j)
                        {
                            double temp = matrix[i, j];
                            matrix[i, j] = matrix[k, j];
                            matrix[k, j] = temp;
                        }

                        break;
                    }
                }
            }

            double pivot = matrix[i, i];

            for (int j = 0; j <= n; ++j)
                matrix[i, j] /= pivot;

            for (int k = 0; k < n; ++k)
            {
                if (k == i) continue;

                double factor = matrix[k, i];

                for (int j = 0; j <= n; ++j)
                    matrix[k, j] -= factor * matrix[i, j];
            }
        }

        double[] result = new double[n];

        for (int i = 0; i < n; ++i)
            result[i] = matrix[i, n];

        return result;
    }
}
