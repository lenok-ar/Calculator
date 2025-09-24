using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Mathos.Parser;

class ChartBuilder
{
    private readonly MathParser parser;
    private string F;

    public ChartBuilder (string F)
    {
        this.F = F;
        parser = new MathParser ();
    }

    public void DrawChart(Chart chart, double a, double b, double step = 0.1)
    {
        chart.Series.Clear ();

        var series = new Series
        { 
            BorderWidth = 2,
            ChartType = SeriesChartType.Line,
            Name = "Функция",
        };

        for (double x = a; x <= b; x += step)
        {
            try 
            {
                parser.LocalVariables["x"] = x;
                double y = parser.Parse(F);
                series.Points.AddXY(x, y);
            }
            catch 
            {
                continue;
            }
        }

        chart.Series.Add(series);
    }

    public void PointRoot (Chart chart, double xRoot, string seriesName = "Корень")
    {
        var pointSeries = new Series
        {
            ChartType = SeriesChartType.Point,
            MarkerStyle = MarkerStyle.Square,
            MarkerSize = 10,
            Color = Color.Red,
            Name = seriesName,
        };

        parser.LocalVariables["x"] = xRoot;
        double y = parser.Parse(F);

        pointSeries.Points.AddXY(xRoot, y);
        chart.Series.Add(pointSeries);
    }
}
