using System;
using Mathos.Parser;

class Method_Golden_Section
{
    private readonly MathParser parser;
    private readonly string _fun;
    private double _rangeA;
    private double _rangeB;
    private readonly double _accuracy;
    private double _x1;
    private double _x2;
    private static readonly double GOLDEN_RATIO = (Math.Sqrt(5) - 1) / 2;

    public Method_Golden_Section(string fun, double rangeA, double rangeB, double accuracy)
    {
        parser = new MathParser();
        _fun = fun;
        _rangeA = rangeA;
        _rangeB = rangeB;
        _accuracy = accuracy;
    }

    private void SearchPoints()
    {
        _x1 = _rangeB - GOLDEN_RATIO * (_rangeB - _rangeA);
        _x2 = _rangeA + GOLDEN_RATIO * (_rangeB - _rangeA);
    }

    private double CalculateFunction(double point)
    {
        parser.LocalVariables["x"] = point;
        double result = parser.Parse(_fun);
        return result;
    }

    private void CheckFunction(double funX1, double funX2)
    {
        if (funX1 > funX2)
        {
            _rangeA = _x1;
        }
        else
        {
            _rangeB = _x2;
        }
    }

    public (double xMin, double funMin) Solve()
    {
        while (Math.Abs(_rangeB - _rangeA) > _accuracy)
        {
            SearchPoints();
            double funX1 = CalculateFunction(_x1);
            double funX2 = CalculateFunction(_x2);
            CheckFunction(funX1, funX2);
        }

        double xMin = (_rangeA + _rangeB) / 2;
        double funMin = CalculateFunction(xMin);

        return (xMin, funMin);
    }
}