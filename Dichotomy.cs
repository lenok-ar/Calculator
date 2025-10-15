using Mathos.Parser;
using System;

class Dichotomy
{
    private readonly MathParser parser;
    private string F;
    private double a;
    private double b;
    private double E;
    private double funcOfA;
    private double funcOfB;
    private double funcOfC;
    private double intervalCenter;

    public Dichotomy(string F, double a, double b, double E)
    {
        this.F = F;
        this.a = a;
        this.b = b;
        this.E = E;
        parser = new MathParser();
        funcOfA = 0;
        funcOfB = 0;
        intervalCenter = 0;
    }

    public bool CheckFunc()
    {
        try
        {
            parser.LocalVariables["x"] = 0;
            parser.Parse(F);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool CheckInterval()
    {
        parser.LocalVariables["x"] = a;
        funcOfA = parser.Parse(F);

        parser.LocalVariables["x"] = b;
        funcOfB = parser.Parse(F);

        return funcOfA * funcOfB < 0;
    }

    public double IntervalCenter()
    {
        return intervalCenter = (a + b) / 2;
    }

    public double Solve()
    {
        if (!CheckFunc())
        {
            return 0;
        }

        if (!CheckInterval())
        {
            return 0;
        }

        int iteration = 0;

        while (Math.Abs(b - a) > E && iteration < 500)
        {
            intervalCenter = IntervalCenter();

            parser.LocalVariables["x"] = intervalCenter;
            funcOfC = parser.Parse(F);

            if (funcOfC < E)
            {
                return intervalCenter;
            }

            if (funcOfC * funcOfA < 0)
            {
                b = intervalCenter;
                funcOfB = funcOfC;
            }
            else
            {
                a = intervalCenter;
                funcOfA = funcOfC;
            }

            ++iteration;
        }

        return intervalCenter;
    }
}