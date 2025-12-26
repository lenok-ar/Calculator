using NCalc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using Brushes = System.Windows.Media.Brushes;

namespace Calculator.page
{
    /// <summary>
    /// Логика взаимодействия для NewtonMethodWindow.xaml
    /// </summary>
    public partial class NewtonMethodWindow : Page
    {
        private CancellationTokenSource _cts;
        private bool _isCalculating = false;
        private Chart _chart;

        public NewtonMethodWindow()
        {
            InitializeComponent();
            InitializeChart();
        }

        private void InitializeChart()
        {
            // Инициализируем график
            _chart = new Chart();
            _chart.BackColor = System.Drawing.Color.White;
            _chart.BorderlineColor = System.Drawing.Color.LightGray;
            _chart.BorderlineDashStyle = ChartDashStyle.Solid;
            _chart.BorderlineWidth = 1;
            _chart.Padding = new System.Windows.Forms.Padding(10);

            // Создаем область графика
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "ChartArea";
            chartArea.BackColor = System.Drawing.Color.White;
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(220, 220, 220);
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(220, 220, 220);
            chartArea.AxisX.MinorGrid.Enabled = false;
            chartArea.AxisY.MinorGrid.Enabled = false;
            chartArea.AxisX.Title = "x";
            chartArea.AxisY.Title = "f(x)";
            chartArea.AxisX.TitleFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular);
            chartArea.AxisY.TitleFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular);

            _chart.ChartAreas.Add(chartArea);

            // Добавляем график в WindowsFormsHost
            ChartHost.Child = _chart;
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCalculating)
            {
                MessageBox.Show("Выполняется расчет. Дождитесь окончания.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isCalculating = true;
                CalculateButton.Content = "Остановить расчет";
                CalculateButton.Click -= CalculateButton_Click;
                CalculateButton.Click += CancelCalculation_Click;

                ResultTextBox.Text = "⏳ Выполняется расчет...";
                ResultTextBox.Foreground = Brushes.Blue;

                _cts = new CancellationTokenSource();

                await Task.Run(() => CalculateMinimum(_cts.Token), _cts.Token);
            }
            catch (OperationCanceledException)
            {
                ResultTextBox.Text = "❌ Расчет отменен пользователем";
                ResultTextBox.Foreground = Brushes.Orange;
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = $"❌ Ошибка: {ex.Message}";
                ResultTextBox.Foreground = Brushes.Red;
            }
            finally
            {
                _isCalculating = false;
                CalculateButton.Content = "Найти минимум";
                CalculateButton.Click -= CancelCalculation_Click;
                CalculateButton.Click += CalculateButton_Click;
                _cts?.Dispose();
            }
        }

        private void CancelCalculation_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void CalculateMinimum(CancellationToken ct)
        {
            // Проверка ввода в UI потоке
            double x0 = 0, epsilon = 0, startInterval = 0, endInterval = 0;
            int maxIterations = 0;
            string functionText = "";

            Dispatcher.Invoke(() =>
            {
                // Проверяем все поля
                if (string.IsNullOrWhiteSpace(FunctionTextBox.Text))
                {
                    MessageBox.Show("Введите функцию!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw new ArgumentException("Функция не введена");
                }

                functionText = FunctionTextBox.Text.Trim();

                if (!TryParseDouble(InitialPointTextBox.Text, out x0))
                {
                    MessageBox.Show("Введите корректную начальную точку!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw new ArgumentException("Неверная начальная точка");
                }

                if (!TryParseDouble(PrecisionTextBox.Text, out epsilon) || epsilon <= 0)
                {
                    MessageBox.Show("Точность должна быть положительным числом!", "Ошибка ввода",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new ArgumentException("Некорректная точность");
                }

                if (!int.TryParse(MaxIterationsTextBox.Text, out maxIterations) || maxIterations <= 0)
                {
                    MessageBox.Show("Максимальное число итераций должно быть положительным целым числом!",
                        "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new ArgumentException("Некорректное число итераций");
                }

                if (!TryParseDouble(StartIntervalTextBox.Text, out startInterval))
                {
                    MessageBox.Show("Введите корректное начало интервала!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw new ArgumentException("Неверное начало интервала");
                }

                if (!TryParseDouble(EndIntervalTextBox.Text, out endInterval))
                {
                    MessageBox.Show("Введите корректный конец интервала!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw new ArgumentException("Неверный конец интервала");
                }

                if (startInterval >= endInterval)
                {
                    MessageBox.Show("Начало интервала должно быть меньше конца!", "Ошибка ввода",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new ArgumentException("Некорректный интервал");
                }
            });

            // После блока Dispatcher.Invoke() переменные уже имеют значения
            ct.ThrowIfCancellationRequested();

            try
            {
                // Проверяем функцию в начальной точке
                if (!IsFunctionDefined(x0, functionText))
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Функция не определена в точке x0 = {x0}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    throw new ArgumentException("Функция не определена в начальной точке");
                }

                // Метод Ньютона
                double x = x0;
                int iteration = 0;
                List<NewtonIteration> iterations = new List<NewtonIteration>();
                bool converged = false;

                while (iteration < maxIterations)
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        double f = CalculateFunction(x, functionText);
                        double f1 = FirstDerivative(x, functionText);
                        double f2 = SecondDerivative(x, functionText);

                        iterations.Add(new NewtonIteration(iteration, x, f, f1, f2));

                        // Критерий остановки по производной
                        if (Math.Abs(f1) < epsilon)
                        {
                            converged = true;
                            break;
                        }

                        if (Math.Abs(f2) < 1e-12)
                        {
                            throw new InvalidOperationException("Вторая производная слишком мала");
                        }

                        double xNew = x - f1 / f2;

                        // Проверка на NaN или бесконечность
                        if (double.IsNaN(xNew) || double.IsInfinity(xNew))
                        {
                            throw new InvalidOperationException("Метод расходится");
                        }

                        // Проверяем, определена ли функция в новой точке
                        if (!IsFunctionDefined(xNew, functionText))
                        {
                            throw new InvalidOperationException($"Функция не определена в точке x = {xNew:0.###}");
                        }

                        // Критерий остановки по изменению x
                        if (Math.Abs(xNew - x) < epsilon)
                        {
                            x = xNew;
                            converged = true;
                            break;
                        }

                        x = xNew;
                        iteration++;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Ошибка на итерации {iteration}: {ex.Message}");
                    }
                }

                double finalF = CalculateFunction(x, functionText);

                // Формируем отчет
                Dispatcher.Invoke(() =>
                {
                    string report = GenerateReport(x, finalF, epsilon, iteration, maxIterations, iterations, converged);
                    ResultTextBox.Text = report;
                    ResultTextBox.Foreground = converged ? Brushes.Green : Brushes.Orange;

                    // Отображаем на графике
                    PlotFunctionWithMinimum(startInterval, endInterval, functionText, x, finalF, iterations);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ResultTextBox.Text = $"❌ Ошибка при расчете: {ex.Message}\n\n" +
                                        $"Рекомендации:\n" +
                                        $"1. Проверьте корректность функции\n" +
                                        $"2. Попробуйте другую начальную точку\n" +
                                        $"3. Увеличьте количество итераций";
                    ResultTextBox.Foreground = Brushes.Red;
                    PlotFunction(startInterval, endInterval, functionText);
                });
            }
        }

        private string GenerateReport(double x, double fx, double epsilon, int iterationsUsed,
            int maxIterations, List<NewtonIteration> iterations, bool converged)
        {
            string report = $"РЕЗУЛЬТАТЫ МЕТОДА НЬЮТОНА:\n\n";

            report += converged ? "✅ Метод сошелся\n\n" : "⚠ Метод не сошелся (достигнут лимит итераций)\n\n";

            report += $"Найдена точка: x = {x:0.##########}\n";
            report += $"Значение функции: f(x) = {fx:0.##########}\n";
            report += $"Производная в точке: f'(x) = {iterations.LastOrDefault()?.Derivative:0.##########}\n\n";

            report += $"Параметры расчета:\n";
            report += $"Точность (ε): {epsilon}\n";
            report += $"Итераций использовано: {iterationsUsed} из {maxIterations}\n\n";

            if (iterations.Count > 0)
            {
                report += $"ПОСЛЕДНИЕ ИТЕРАЦИИ:\n";
                report += $"{"№",-4} {"x",-15} {"f(x)",-15} {"f'(x)",-15}\n";
                report += new string('-', 60) + "\n";

                int start = Math.Max(0, iterations.Count - 5);
                for (int i = start; i < iterations.Count; i++)
                {
                    var iter = iterations[i];
                    report += $"{iter.Iteration,-4} {iter.X,-15:0.##########} " +
                             $"{iter.F,-15:0.##########} {iter.Derivative,-15:0.##########}\n";
                }
            }

            return report;
        }

        private void PlotFunctionWithMinimum(double a, double b, string functionText,
            double minimumX, double minimumY, List<NewtonIteration> iterations)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Очищаем предыдущие графики
                    _chart.Series.Clear();

                    // График функции
                    Series functionSeries = new Series("Функция");
                    functionSeries.ChartType = SeriesChartType.Line;
                    functionSeries.Color = System.Drawing.Color.SteelBlue;
                    functionSeries.BorderWidth = 2;
                    functionSeries.MarkerStyle = MarkerStyle.None;

                    // Вычисляем точки для графика
                    int points = 200;
                    double step = (b - a) / points;
                    int validPoints = 0;

                    for (int i = 0; i <= points; i++)
                    {
                        double x = a + i * step;
                        try
                        {
                            double y = CalculateFunction(x, functionText);
                            if (!double.IsNaN(y) && !double.IsInfinity(y) &&
                                Math.Abs(y) < 1e6) // Ограничиваем слишком большие значения
                            {
                                functionSeries.Points.AddXY(x, y);
                                validPoints++;
                            }
                        }
                        catch
                        {
                            // Пропускаем точки разрыва
                        }
                    }

                    if (validPoints > 0)
                    {
                        _chart.Series.Add(functionSeries);
                    }

                    // Точка минимума
                    if (!double.IsNaN(minimumX) && !double.IsNaN(minimumY) &&
                        minimumX >= a && minimumX <= b)
                    {
                        Series minimumSeries = new Series("Минимум");
                        minimumSeries.ChartType = SeriesChartType.Point;
                        minimumSeries.Color = System.Drawing.Color.Red;
                        minimumSeries.MarkerStyle = MarkerStyle.Circle;
                        minimumSeries.MarkerSize = 10;
                        minimumSeries.MarkerBorderColor = System.Drawing.Color.DarkRed;
                        minimumSeries.MarkerBorderWidth = 2;
                        minimumSeries.Points.AddXY(minimumX, minimumY);

                        _chart.Series.Add(minimumSeries);

                        // Траектория итераций
                        if (iterations.Count > 1)
                        {
                            Series iterationSeries = new Series("Итерации");
                            iterationSeries.ChartType = SeriesChartType.Point;
                            iterationSeries.Color = System.Drawing.Color.Green;
                            iterationSeries.MarkerStyle = MarkerStyle.Triangle;
                            iterationSeries.MarkerSize = 6;
                            iterationSeries.MarkerBorderColor = System.Drawing.Color.DarkGreen;

                            foreach (var iter in iterations)
                            {
                                if (iter.X >= a && iter.X <= b)
                                {
                                    iterationSeries.Points.AddXY(iter.X, iter.F);
                                }
                            }

                            if (iterationSeries.Points.Count > 0)
                            {
                                _chart.Series.Add(iterationSeries);
                            }
                        }
                    }

                    // Настройки графика
                    _chart.ChartAreas[0].AxisX.Minimum = a;
                    _chart.ChartAreas[0].AxisX.Maximum = b;
                    _chart.ChartAreas[0].AxisX.Interval = Math.Round((b - a) / 10, 2);
                    _chart.Titles.Clear();
                    _chart.Titles.Add($"f(x) = {SimplifyFunctionText(functionText)}");
                    _chart.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                    _chart.Titles[0].ForeColor = System.Drawing.Color.FromArgb(44, 62, 80);

                    // Автомасштабирование по Y
                    _chart.ChartAreas[0].RecalculateAxesScale();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ResultTextBox.Text += $"\n\n⚠ Ошибка при построении графика: {ex.Message}";
                });
            }
        }

        private void PlotFunction(double a, double b, string functionText)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Очищаем предыдущие графики
                    _chart.Series.Clear();

                    // График функции
                    Series functionSeries = new Series("Функция");
                    functionSeries.ChartType = SeriesChartType.Line;
                    functionSeries.Color = System.Drawing.Color.SteelBlue;
                    functionSeries.BorderWidth = 2;

                    // Вычисляем точки для графика
                    int points = 200;
                    double step = (b - a) / points;
                    int validPoints = 0;

                    for (int i = 0; i <= points; i++)
                    {
                        double x = a + i * step;
                        try
                        {
                            double y = CalculateFunction(x, functionText);
                            if (!double.IsNaN(y) && !double.IsInfinity(y))
                            {
                                functionSeries.Points.AddXY(x, y);
                                validPoints++;
                            }
                        }
                        catch
                        {
                            // Пропускаем точки разрыва
                        }
                    }

                    if (validPoints > 0)
                    {
                        _chart.Series.Add(functionSeries);
                    }

                    // Настройки графика
                    _chart.ChartAreas[0].AxisX.Minimum = a;
                    _chart.ChartAreas[0].AxisX.Maximum = b;
                    _chart.ChartAreas[0].AxisX.Interval = Math.Round((b - a) / 10, 2);
                    _chart.Titles.Clear();
                    _chart.Titles.Add($"f(x) = {SimplifyFunctionText(functionText)}");
                    _chart.Titles[0].Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);

                    // Автомасштабирование
                    _chart.ChartAreas[0].RecalculateAxesScale();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при построении графика: {ex.Message}");
            }
        }

        private string SimplifyFunctionText(string functionText)
        {
            // Упрощаем отображение функции на графике
            return functionText.Length > 30 ?
                functionText.Substring(0, 27) + "..." :
                functionText;
        }

        private bool IsFunctionDefined(double x, string functionText)
        {
            try
            {
                double result = CalculateFunction(x, functionText);
                return !double.IsNaN(result) && !double.IsInfinity(result);
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseDouble(string text, out double result)
        {
            return double.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private double ParseDouble(string text)
        {
            return double.Parse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCalculating)
            {
                var result = MessageBox.Show("Выполняется расчет. Закрыть страницу?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _cts?.Cancel();
                    NavigationService?.GoBack();
                }
            }
            else
            {
                NavigationService?.GoBack();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Опционально: можно добавить валидацию при вводе
        }

        private void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double start = ParseDouble(StartIntervalTextBox.Text);
                double end = ParseDouble(EndIntervalTextBox.Text);

                // Простой автоподбор - берем середину интервала
                double x0 = (start + end) / 2;

                InitialPointTextBox.Text = x0.ToString("0.######", CultureInfo.InvariantCulture);

                MessageBox.Show($"Установлена начальная точка: {x0:0.######}", "Автоподбор",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Не удалось выполнить автоподбор. Проверьте интервал.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FunctionTextBox.Text = "x^2";
            InitialPointTextBox.Text = "1";
            PrecisionTextBox.Text = "0.001";
            MaxIterationsTextBox.Text = "100";
            StartIntervalTextBox.Text = "-2";
            EndIntervalTextBox.Text = "2";
            ResultTextBox.Text = "";

            InitializeChart();
        }

        private double CalculateFunction(double x, string functionText)
        {
            try
            {
                // Поддержка математических функций
                string processedText = functionText.ToLower();

                // Заменяем ^ на ** для NCalc
                processedText = System.Text.RegularExpressions.Regex.Replace(
                    processedText,
                    @"(\d+|x)\s*\^\s*(\d+|\(.*?\))",
                    match => $"pow({match.Groups[1].Value},{match.Groups[2].Value})"
                );

                NCalc.Expression expression = new NCalc.Expression(processedText);
                expression.Parameters["x"] = x;

                // Обработка математических функций
                expression.EvaluateFunction += (name, args) =>
                {
                    double arg = Convert.ToDouble(args.Parameters[0].Evaluate());

                    switch (name.ToLower())
                    {
                        case "sqrt":
                            if (arg < 0) throw new ArgumentException("Корень из отрицательного числа");
                            args.Result = Math.Sqrt(arg);
                            break;
                        case "sin":
                            args.Result = Math.Sin(arg);
                            break;
                        case "cos":
                            args.Result = Math.Cos(arg);
                            break;
                        case "tan":
                            args.Result = Math.Tan(arg);
                            break;
                        case "log":
                        case "ln":
                            if (arg <= 0) throw new ArgumentException("Логарифм неположительного числа");
                            args.Result = Math.Log(arg);
                            break;
                        case "exp":
                            args.Result = Math.Exp(arg);
                            break;
                        case "abs":
                            args.Result = Math.Abs(arg);
                            break;
                        case "pow":
                            double baseValue = Convert.ToDouble(args.Parameters[0].Evaluate());
                            double exponent = Convert.ToDouble(args.Parameters[1].Evaluate());
                            args.Result = Math.Pow(baseValue, exponent);
                            break;
                        default:
                            throw new ArgumentException($"Неизвестная функция: {name}");
                    }
                };

                expression.EvaluateParameter += (name, args) =>
                {
                    if (name.ToLower() == "pi")
                        args.Result = Math.PI;
                    else if (name.ToLower() == "e")
                        args.Result = Math.E;
                };

                object result = expression.Evaluate();

                if (result == null)
                    throw new ArgumentException("Функция вернула null");

                double doubleResult = Convert.ToDouble(result);

                if (double.IsInfinity(doubleResult))
                    throw new ArgumentException("Функция вернула бесконечность");

                if (double.IsNaN(doubleResult))
                    throw new ArgumentException("Функция вернула NaN");

                return doubleResult;
            }
            catch (DivideByZeroException)
            {
                throw new ArgumentException("Деление на ноль");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка вычисления: {ex.Message}");
            }
        }

        private double FirstDerivative(double x, string functionText, double h = 1e-5)
        {
            try
            {
                double f_plus = CalculateFunction(x + h, functionText);
                double f_minus = CalculateFunction(x - h, functionText);

                return (f_plus - f_minus) / (2 * h);
            }
            catch
            {
                // Если не удается вычислить центральную разность, пробуем одностороннюю
                try
                {
                    double f_plus = CalculateFunction(x + h, functionText);
                    double f_current = CalculateFunction(x, functionText);

                    return (f_plus - f_current) / h;
                }
                catch
                {
                    throw new ArgumentException("Не удается вычислить производную");
                }
            }
        }

        private double SecondDerivative(double x, string functionText, double h = 1e-5)
        {
            try
            {
                double f_plus = CalculateFunction(x + h, functionText);
                double f_current = CalculateFunction(x, functionText);
                double f_minus = CalculateFunction(x - h, functionText);

                return (f_plus - 2 * f_current + f_minus) / (h * h);
            }
            catch
            {
                throw new ArgumentException("Не удается вычислить вторую производную");
            }
        }
    }

    // Вспомогательные классы
    public class NewtonIteration
    {
        public int Iteration { get; set; }
        public double X { get; set; }
        public double F { get; set; }
        public double Derivative { get; set; }
        public double SecondDerivative { get; set; }

        public NewtonIteration(int iteration, double x, double f, double derivative, double secondDerivative)
        {
            Iteration = iteration;
            X = x;
            F = f;
            Derivative = derivative;
            SecondDerivative = secondDerivative;
        }
    }
}