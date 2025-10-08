using System;

class Method_Gaussa
{
    private double[,] _matrixA;
    private double[] _vectorb;
    private double[] _vectorB;

    public Method_Gaussa(double[,] matrixA, double[] vectorB)
    {
        _matrixA = matrixA;
        _vectorb = vectorB;
        ConvertEquation();
    }

    private double[] ConvertEquation()
    {
        _vectorB = new double[_vectorb.Length];

        for (int i = 0; i < _vectorb.Length; ++i)
        {
            _vectorB[i] = -(_vectorb[i]);
        }

        return _vectorB;
    }

    public double[] SolveGauss()
    {
        int n = _vectorB.Length;

        for (int pivot = 0; pivot < n; ++pivot)
        {
            int maxRow = pivot;
            double maxVal = Math.Abs(_matrixA[pivot, pivot]);

            for (int i = pivot + 1; i < n; ++i)
            {
                if (Math.Abs(_matrixA[i, pivot]) > maxVal)
                {
                    maxVal = Math.Abs(_matrixA[i, pivot]);
                    maxRow = i;
                }
            }

            if (maxRow != pivot)
            {
                for (int j = 0; j < n; ++j)
                {
                    double temp = _matrixA[pivot, j];
                    _matrixA[pivot, j] = _matrixA[maxRow, j];
                    _matrixA[maxRow, j] = temp;
                }

                double tempB = _vectorB[pivot];
                _vectorB[pivot] = _vectorB[maxRow];
                _vectorB[maxRow] = tempB;
            }

            for (int i = pivot + 1; i < n; ++i)
            {
                double factor = _matrixA[i, pivot] / _matrixA[pivot, pivot];

                for (int j = pivot; j < n; ++j)
                {
                    _matrixA[i, j] -= factor * _matrixA[pivot, j];
                }

                _vectorB[i] -= factor * _vectorB[pivot];
            }
        }

        double[] x = new double[n];

        for (int i = n - 1; i >= 0; --i)
        {
            double sum = 0;

            for (int j = i + 1; j < n; ++j)
            {
                sum += _matrixA[i, j] * x[j];
            }

            x[i] = (_vectorB[i] - sum) / _matrixA[i, i];
        }

        return x;
    }
}
