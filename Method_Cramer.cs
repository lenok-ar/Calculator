using System.Windows;

class Method_Cramer
{
    private double[,] _matrixA;
    private double[] _vectorb;
    private double[] _vectorB;

    public Method_Cramer(double[,] matrixA, double[] vectorb)
    {
        _matrixA = matrixA;
        _vectorb = vectorb;
    }

    public double Determinant(double[,] matrixA)
    {
        int n = matrixA.GetLength(0);

        if (n == 1) return matrixA[0, 0];
        if (n == 2) return matrixA[0, 0] * matrixA[1, 1] - matrixA[0, 1] * matrixA[1, 0];

        double det = 0;

        for (int p = 0; p < n; ++p)
        {
            double[,] subMatrix = new double[n - 1, n - 1];

            for (int i = 1; i < n; ++i)
            {
                int colIndex = 0;

                for (int j = 0; j < n; ++j)
                {
                    if (j == p) continue;
                    subMatrix[i - 1, colIndex] = matrixA[i, j];
                    colIndex++;
                }
            }

            det += matrixA[0, p] * Determinant(subMatrix) * ((p % 2 == 0) ? 1 : -1);
        }

        return det;
    }

    public double[] Solve()
    {
        int n = _matrixA.GetLength(0);
        double detA = Determinant(_matrixA);
        _vectorB = new double[_vectorb.Length];

        if (detA == 0)
        {
            MessageBox.Show("Система не имеет единственного решения!");
        }

        for (int i = 0; i < _vectorb.Length; ++i)
            _vectorB[i] = -(_vectorb[i]);

        double[] x = new double[n];

        for (int i = 0; i < n; ++i)
        {
            double[,] Ai = (double[,])_matrixA.Clone();

            for (int j = 0; j < n; ++j)
            {
                Ai[j, i] = _vectorB[j];
            }

            x[i] = Determinant(Ai) / detA;
        }

        return x;
    }
}
