using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Calculator
{
    public partial class LeastSquaresPage : Page
    {
        private ObservableCollection<DataPointLocal> _dataPoints;
        private PlotModel _plotModel;

        public class DataPointLocal
        {
            public int Index { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class PointData
        {
            public double X { get; set; }
            public double Y { get; set; }

            public PointData(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public LeastSquaresPage()
        {
            InitializeComponent();
            InitializeDataGrid();
            InitializePlotModel();
            WireUpEvents();
        }

        private void InitializeDataGrid()
        {
            _dataPoints = new ObservableCollection<DataPointLocal>
            {
                new DataPointLocal { Index = 1, X = 1.0, Y = 2.1 },
                new DataPointLocal { Index = 2, X = 2.0, Y = 3.2 },
                new DataPointLocal { Index = 3, X = 3.0, Y = 4.8 },
                new DataPointLocal { Index = 4, X = 4.0, Y = 6.1 },
                new DataPointLocal { Index = 5, X = 5.0, Y = 7.3 }
            };
        }

        private void InitializePlotModel()
        {
            _plotModel = new PlotModel
            {
                Title = "Метод наименьших квадратов",
                TitleFontSize = 14,
                TitleColor = OxyColors.DarkBlue,
                PlotMargins = new OxyThickness(50, 20, 20, 40),
                Background = OxyColors.White
            };

            _plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
                MinimumPadding = 0.1,
                MaximumPadding = 0.1
            });

            _plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
                MinimumPadding = 0.1,
                MaximumPadding = 0.1
            });

            PlotView.Model = _plotModel;
        }

        private void WireUpEvents()
        {
            LoadButton.Click += LoadButton_Click;
            ExportButton.Click += ExportButton_Click;
            GenerateButton.Click += GenerateButton_Click;
            ClearButton.Click += ClearButton_Click;
            CalculateButton.Click += CalculateButton_Click;
            BackButton.Click += BackButton_Click;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Menu());
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Выберите файл с данными",
                    DefaultExt = ".csv",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    LoadDataFromFile(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDataFromFile(string filePath)
        {
            try
            {
                _dataPoints.Clear();

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                int index = 1;
                int lineNumber = 0;

                foreach (string line in lines)
                {
                    lineNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.ToLower().Contains("x") && line.ToLower().Contains("y") && lineNumber == 1)
                        continue;

                    string[] parts = line.Split(new char[] { ',', ';', '\t', '|' },
                        StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                    {
                        if (double.TryParse(parts[0].Replace(',', '.'), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out double x) &&
                            double.TryParse(parts[1].Replace(',', '.'), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out double y))
                        {
                            _dataPoints.Add(new DataPointLocal
                            {
                                Index = index++,
                                X = Math.Round(x, 3),
                                Y = Math.Round(y, 3)
                            });
                        }
                    }
                }

                if (_dataPoints.Count == 0)
                {
                    MessageBox.Show("Не удалось загрузить данные из файла. Проверьте формат файла.\n" +
                        "Ожидаемый формат: X,Y в каждой строке",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Успешно загружено {_dataPoints.Count} точек из файла",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    PlotCurrentData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения файла: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dataPoints.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Экспорт данных",
                    DefaultExt = ".csv",
                    FileName = $"least_squares_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("X,Y");

                        foreach (var point in _dataPoints)
                        {
                            writer.WriteLine($"{point.X.ToString(CultureInfo.InvariantCulture)},{point.Y.ToString(CultureInfo.InvariantCulture)}");
                        }

                        if (!string.IsNullOrEmpty(LinearResultText.Text) && LinearResultText.Text != "Не рассчитано")
                        {
                            writer.WriteLine();
                            writer.WriteLine("# Результаты метода наименьших квадратов:");
                            writer.WriteLine($"# Линейная аппроксимация: {LinearResultText.Text}");
                            writer.WriteLine($"# R² = {LinearRSquaredText.Text}");

                            if (!string.IsNullOrEmpty(QuadraticResultText.Text) && QuadraticResultText.Text != "Не рассчитано")
                            {
                                writer.WriteLine($"# Квадратичная аппроксимация: {QuadraticResultText.Text}");
                                writer.WriteLine($"# R² = {QuadraticRSquaredText.Text}");
                            }
                        }
                    }

                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateGenerationParameters())
                    return;

                int pointsCount = int.Parse(PointsCountTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                double minX = double.Parse(MinXTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                double maxX = double.Parse(MaxXTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                double minYBase = double.Parse(MinYBaseTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                double maxYBase = double.Parse(MaxYBaseTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                double noiseLevel = double.Parse(NoiseLevelTextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);

                _dataPoints.Clear();

                Random rand = new Random();
                double xRange = maxX - minX;
                double yBaseRange = maxYBase - minYBase;

                for (int i = 0; i < pointsCount; i++)
                {
                    double x, yBase, noise, y;

                    if (RandomizeCheckBox.IsChecked == true)
                    {
                        x = minX + rand.NextDouble() * xRange;
                        yBase = minYBase + (yBaseRange * (x - minX) / Math.Max(1, xRange));
                    }
                    else
                    {
                        x = minX + (xRange * i) / Math.Max(1, pointsCount - 1);
                        yBase = minYBase + (yBaseRange * i) / Math.Max(1, pointsCount - 1);
                    }

                    noise = (rand.NextDouble() - 0.5) * 2 * noiseLevel;
                    y = yBase + noise;

                    _dataPoints.Add(new DataPointLocal
                    {
                        Index = i + 1,
                        X = Math.Round(x, 3),
                        Y = Math.Round(y, 3)
                    });
                }

                PlotCurrentData();
                ClearResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateGenerationParameters()
        {
            if (string.IsNullOrWhiteSpace(PointsCountTextBox.Text) ||
                string.IsNullOrWhiteSpace(MinXTextBox.Text) ||
                string.IsNullOrWhiteSpace(MaxXTextBox.Text) ||
                string.IsNullOrWhiteSpace(MinYBaseTextBox.Text) ||
                string.IsNullOrWhiteSpace(MaxYBaseTextBox.Text) ||
                string.IsNullOrWhiteSpace(NoiseLevelTextBox.Text))
            {
                MessageBox.Show("Заполните все параметры генерации",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(PointsCountTextBox.Text.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out int pointsCount) || pointsCount < 2)
            {
                MessageBox.Show("Количество точек должно быть целым числом не меньше 2",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!double.TryParse(MinXTextBox.Text.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out double minX) ||
                !double.TryParse(MaxXTextBox.Text.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out double maxX))
            {
                MessageBox.Show("Некорректные значения для X",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (minX >= maxX)
            {
                MessageBox.Show("Минимальное значение X должно быть меньше максимального",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!double.TryParse(NoiseLevelTextBox.Text.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out double noiseLevel) || noiseLevel < 0)
            {
                MessageBox.Show("Уровень шума должен быть неотрицательным числом",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _dataPoints.Clear();
            ClearResults();
            _plotModel.Series.Clear();
            _plotModel.InvalidatePlot(true);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var points = GetValidPoints();

                if (points.Count < 2)
                {
                    MessageBox.Show("Недостаточно точек для расчета (нужно минимум 2)",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var linearResult = CalculateLeastSquares(points, 1);
                DisplayLinearResults(linearResult);

                var quadraticResult = CalculateLeastSquares(points, 2);
                DisplayQuadraticResults(quadraticResult);

                PlotGraph(points, linearResult, quadraticResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<PointData> GetValidPoints()
        {
            var points = new List<PointData>();

            foreach (var dataPoint in _dataPoints)
            {
                if (!double.IsNaN(dataPoint.X) && !double.IsNaN(dataPoint.Y) &&
                    !double.IsInfinity(dataPoint.X) && !double.IsInfinity(dataPoint.Y))
                {
                    points.Add(new PointData(dataPoint.X, dataPoint.Y));
                }
            }

            return points;
        }

        private (double[] coefficients, double rSquared) CalculateLeastSquares(List<PointData> points, int degree)
        {
            int n = points.Count;
            int m = degree + 1;

            double[,] A = new double[m, m];
            double[] b = new double[m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += Math.Pow(points[k].X, i + j);
                    }
                    A[i, j] = sum;
                }

                double sumB = 0;
                for (int k = 0; k < n; k++)
                {
                    sumB += points[k].Y * Math.Pow(points[k].X, i);
                }
                b[i] = sumB;
            }

            double[] coefficients = SolveLinearSystem(A, b);
            double rSquared = CalculateRSquared(points, coefficients);

            return (coefficients, rSquared);
        }

        private double[] SolveLinearSystem(double[,] A, double[] b)
        {
            int n = b.Length;
            double[] x = new double[n];

            double[,] aCopy = (double[,])A.Clone();
            double[] bCopy = (double[])b.Clone();

            for (int i = 0; i < n; i++)
            {
                double maxEl = Math.Abs(aCopy[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(aCopy[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(aCopy[k, i]);
                        maxRow = k;
                    }
                }

                if (maxRow != i)
                {
                    for (int k = i; k < n; k++)
                    {
                        double temp = aCopy[maxRow, k];
                        aCopy[maxRow, k] = aCopy[i, k];
                        aCopy[i, k] = temp;
                    }
                    double tempB = bCopy[maxRow];
                    bCopy[maxRow] = bCopy[i];
                    bCopy[i] = tempB;
                }

                for (int k = i + 1; k < n; k++)
                {
                    double factor = aCopy[k, i] / aCopy[i, i];
                    for (int j = i; j < n; j++)
                    {
                        aCopy[k, j] -= factor * aCopy[i, j];
                    }
                    bCopy[k] -= factor * bCopy[i];
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = bCopy[i];
                for (int k = i + 1; k < n; k++)
                {
                    x[i] -= aCopy[i, k] * x[k];
                }
                x[i] /= aCopy[i, i];
            }

            return x;
        }

        private double CalculateRSquared(List<PointData> points, double[] coefficients)
        {
            double yMean = points.Average(p => p.Y);
            double ssTotal = 0;
            double ssResidual = 0;

            foreach (var point in points)
            {
                double yPredicted = 0;
                for (int i = 0; i < coefficients.Length; i++)
                {
                    yPredicted += coefficients[i] * Math.Pow(point.X, i);
                }

                ssTotal += Math.Pow(point.Y - yMean, 2);
                ssResidual += Math.Pow(point.Y - yPredicted, 2);
            }

            if (Math.Abs(ssTotal) < 1e-10)
                return 1.0;

            return Math.Max(0, Math.Min(1, 1 - (ssResidual / ssTotal)));
        }

        private void DisplayLinearResults((double[] coefficients, double rSquared) result)
        {
            if (result.coefficients == null || result.coefficients.Length < 2)
                return;

            string equation = $"y = {result.coefficients[0]:0.######}";
            if (result.coefficients[1] >= 0)
                equation += $" + {result.coefficients[1]:0.######}x";
            else
                equation += $" - {Math.Abs(result.coefficients[1]):0.######}x";

            LinearResultText.Text = equation;
            LinearRSquaredText.Text = $"{result.rSquared:0.######}";

            // Цвет R² в зависимости от качества аппроксимации
            if (result.rSquared > 0.8)
                LinearRSquaredText.Foreground = System.Windows.Media.Brushes.Green;
            else if (result.rSquared > 0.5)
                LinearRSquaredText.Foreground = System.Windows.Media.Brushes.Orange;
            else
                LinearRSquaredText.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void DisplayQuadraticResults((double[] coefficients, double rSquared) result)
        {
            if (result.coefficients == null || result.coefficients.Length < 3)
                return;

            string equation = $"y = {result.coefficients[0]:0.######}";

            if (result.coefficients[1] >= 0)
                equation += $" + {result.coefficients[1]:0.######}x";
            else
                equation += $" - {Math.Abs(result.coefficients[1]):0.######}x";

            if (result.coefficients[2] >= 0)
                equation += $" + {result.coefficients[2]:0.######}x²";
            else
                equation += $" - {Math.Abs(result.coefficients[2]):0.######}x²";

            QuadraticResultText.Text = equation;
            QuadraticRSquaredText.Text = $"{result.rSquared:0.######}";

            // Цвет R² в зависимости от качества аппроксимации
            if (result.rSquared > 0.8)
                QuadraticRSquaredText.Foreground = System.Windows.Media.Brushes.Green;
            else if (result.rSquared > 0.5)
                QuadraticRSquaredText.Foreground = System.Windows.Media.Brushes.Orange;
            else
                QuadraticRSquaredText.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void PlotCurrentData()
        {
            if (_dataPoints.Count == 0)
                return;

            _plotModel.Series.Clear();

            var scatterSeries = new ScatterSeries
            {
                Title = "Исходные точки",
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.DarkBlue,
                MarkerStroke = OxyColors.White,
                MarkerStrokeThickness = 1
            };

            foreach (var point in _dataPoints)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            _plotModel.Series.Add(scatterSeries);
            _plotModel.InvalidatePlot(true);
        }

        private void PlotGraph(List<PointData> points,
                              (double[] coefficients, double rSquared) linearResult,
                              (double[] coefficients, double rSquared) quadraticResult)
        {
            _plotModel.Series.Clear();

            if (points.Count == 0)
                return;

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            if (ShowPointsCheckBox.IsChecked == true)
            {
                var scatterSeries = new ScatterSeries
                {
                    Title = "Исходные точки",
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 6,
                    MarkerFill = OxyColors.DarkBlue,
                    MarkerStroke = OxyColors.White,
                    MarkerStrokeThickness = 1
                };

                foreach (var point in points)
                {
                    scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
                }

                _plotModel.Series.Add(scatterSeries);
            }

            if (ShowLinearCheckBox.IsChecked == true && linearResult.coefficients != null && linearResult.coefficients.Length >= 2)
            {
                var lineSeries = new LineSeries
                {
                    Title = $"Линейная (R²={linearResult.rSquared:0.###})",
                    Color = OxyColors.Red,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid
                };

                int steps = 100;
                for (int i = 0; i <= steps; i++)
                {
                    double x = minX + (maxX - minX) * i / steps;
                    double y = linearResult.coefficients[0] + linearResult.coefficients[1] * x;
                    lineSeries.Points.Add(new DataPoint(x, y));
                }

                _plotModel.Series.Add(lineSeries);
            }

            if (ShowQuadraticCheckBox.IsChecked == true && quadraticResult.coefficients != null && quadraticResult.coefficients.Length >= 3)
            {
                var quadSeries = new LineSeries
                {
                    Title = $"Квадратичная (R²={quadraticResult.rSquared:0.###})",
                    Color = OxyColors.Green,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid
                };

                int steps = 100;
                for (int i = 0; i <= steps; i++)
                {
                    double x = minX + (maxX - minX) * i / steps;
                    double y = quadraticResult.coefficients[0] +
                              quadraticResult.coefficients[1] * x +
                              quadraticResult.coefficients[2] * x * x;
                    quadSeries.Points.Add(new DataPoint(x, y));
                }

                _plotModel.Series.Add(quadSeries);
            }

            double xPadding = Math.Max(0.1 * (maxX - minX), 0.5);
            double yPadding = Math.Max(0.1 * (maxY - minY), 0.5);

            ((LinearAxis)_plotModel.Axes[0]).Minimum = minX - xPadding;
            ((LinearAxis)_plotModel.Axes[0]).Maximum = maxX + xPadding;
            ((LinearAxis)_plotModel.Axes[1]).Minimum = minY - yPadding;
            ((LinearAxis)_plotModel.Axes[1]).Maximum = maxY + yPadding;

            _plotModel.InvalidatePlot(true);
        }

        private void ClearResults()
        {
            LinearResultText.Text = "Не рассчитано";
            LinearRSquaredText.Text = "-";
            QuadraticResultText.Text = "Не рассчитано";
            QuadraticRSquaredText.Text = "-";

            LinearRSquaredText.Foreground = System.Windows.Media.Brushes.Black;
            QuadraticRSquaredText.Foreground = System.Windows.Media.Brushes.Black;
        }
    }
}