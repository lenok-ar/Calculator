// page/NewtonMethodWindow.xaml.cs
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

// ДОБАВЬТЕ ЭТУ СТРОЧКУ:
using Calculator;

namespace Calculator.page
{
    public partial class NewtonMethodWindow : Page
    {
        private NewtonMethod newtonMethod;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;

        public NewtonMethodWindow()
        {
            try
            {
                InitializeComponent();

                // Отладочная информация в консоль
                Debug.WriteLine("=== NewtonMethodWindow конструктор ===");
                Debug.WriteLine($"CalculateButton: {CalculateButton != null}");
                Debug.WriteLine($"FunctionTextBox: {FunctionTextBox != null}");
                Debug.WriteLine($"InitialPointTextBox: {InitialPointTextBox != null}");
                Debug.WriteLine($"PrecisionTextBox: {PrecisionTextBox != null}");

                InitializeControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeControls()
        {
            try
            {
                InitializeChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации графика: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeChart()
        {
            chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart.Size = new System.Drawing.Size(600, 400);

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "x";
            chartArea.AxisY.Title = "f(x)";
            chartArea.AxisX.LabelStyle.Format = "F2";
            chartArea.AxisY.LabelStyle.Format = "F2";
            chart.ChartAreas.Add(chartArea);

            Series functionSeries = new Series("f(x)");
            functionSeries.ChartType = SeriesChartType.Line;
            functionSeries.Color = System.Drawing.Color.Blue;
            functionSeries.BorderWidth = 2;
            chart.Series.Add(functionSeries);

            Series minimumSeries = new Series("Минимум");
            minimumSeries.ChartType = SeriesChartType.Point;
            minimumSeries.Color = System.Drawing.Color.Red;
            minimumSeries.MarkerSize = 10;
            minimumSeries.MarkerStyle = MarkerStyle.Circle;
            chart.Series.Add(minimumSeries);

            Series iterationsSeries = new Series("Итерации");
            iterationsSeries.ChartType = SeriesChartType.Point;
            iterationsSeries.Color = System.Drawing.Color.Green;
            iterationsSeries.MarkerSize = 6;
            iterationsSeries.MarkerStyle = MarkerStyle.Triangle;
            chart.Series.Add(iterationsSeries);

            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = chart;

            System.Windows.Controls.Grid chartGrid = new System.Windows.Controls.Grid();
            chartGrid.Children.Add(host);
            ChartContainer.Content = chartGrid;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                string function = PreprocessFunction(FunctionTextBox.Text);
                double x0 = double.Parse(InitialPointTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double epsilon = double.Parse(PrecisionTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                int maxIterations = int.Parse(MaxIterationsTextBox.Text);
                double a = double.Parse(StartIntervalTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double b = double.Parse(EndIntervalTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);

                newtonMethod = new Calculator.NewtonMethod(function);

                if (!newtonMethod.TestFunctionOnInterval(a, b))
                {
                    MessageBox.Show("Функция не определена или имеет разрывы на заданном интервале\n" +
                                  "Проверьте правильность функции или измените интервал",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Calculator.NewtonResult result = newtonMethod.FindMinimum(x0, epsilon, maxIterations, a, b);
                DisplayResults(result);
                UpdateChart(a, b, result);
            }
            catch (FormatException)
            {
                MessageBox.Show("Некорректный формат чисел. Используйте точку или запятую как разделитель.",
                              "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string PreprocessFunction(string function)
        {
            if (string.IsNullOrWhiteSpace(function))
                return function;

            // Заменяем запятые на точки
            string result = function.Replace(",", ".");

            // Приводим к нижнему регистру
            result = result.ToLower();

            // Заменяем ^ на ** для DataTable
            result = result.Replace("^", "**");

            return result;
        }

        private void DisplayResults(Calculator.NewtonResult result)
        {
            try
            {
                ResultTextBox.Text = $"Метод Ньютона для поиска минимума:\n\n";
                ResultTextBox.Text += $"Точка минимума: x = {result.MinimumPoint:F6}\n";
                ResultTextBox.Text += $"Значение функции: f(x) = {result.MinimumValue:F6}\n";
                ResultTextBox.Text += $"Количество итераций: {result.Iterations}\n";
                ResultTextBox.Text += $"Первая производная: f'(x) = {result.FinalDerivative:E4}\n";
                ResultTextBox.Text += $"Вторая производная: f''(x) = {result.FinalSecondDerivative:E4}\n";
                ResultTextBox.Text += $"Статус: {result.ConvergenceMessage}\n\n";

                // Добавление пошаговых итераций
                if (result.StepByStepIterations != null && result.StepByStepIterations.Any())
                {
                    ResultTextBox.Text += $"ПОШАГОВЫЕ ИТЕРАЦИИ:\n";

                    foreach (var iteration in result.StepByStepIterations)
                    {
                        string iterationText = $"Итерация {iteration.Iteration + 1}: ";
                        iterationText += $"x = {iteration.X:F6}, ";
                        iterationText += $"f(x) = {iteration.FunctionValue:F6}, ";
                        iterationText += $"f' = {iteration.FirstDerivative:E3}, ";
                        iterationText += $"f'' = {iteration.SecondDerivative:E3}";

                        ResultTextBox.Text += $"{iterationText}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения результатов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateChart(double a, double b, NewtonResult result)
        {
            if (newtonMethod == null || chart == null)
                return;

            try
            {
                // Очищаем график
                foreach (var series in chart.Series)
                {
                    series.Points.Clear();
                }

                // Находим series по имени
                Series functionSeries = chart.Series.FirstOrDefault(s => s.Name == "f(x)");
                Series minimumSeries = chart.Series.FirstOrDefault(s => s.Name == "Минимум");
                Series iterationsSeries = chart.Series.FirstOrDefault(s => s.Name == "Итерации");

                if (functionSeries == null) return;

                // Генерация точек для графика функции
                int pointsCount = 400;
                double step = (b - a) / pointsCount;

                double minY = double.MaxValue;
                double maxY = double.MinValue;

                for (int i = 0; i <= pointsCount; i++)
                {
                    double x = a + i * step;
                    try
                    {
                        double y = newtonMethod.CalculateFunction(x);
                        if (!double.IsInfinity(y) && !double.IsNaN(y))
                        {
                            functionSeries.Points.AddXY(x, y);

                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                    catch
                    {
                        // Пропускаем проблемные точки
                    }
                }

                // Настраиваем оси
                if (chart.ChartAreas.Count > 0)
                {
                    // Ось X
                    chart.ChartAreas[0].AxisX.Minimum = a;
                    chart.ChartAreas[0].AxisX.Maximum = b;
                    chart.ChartAreas[0].AxisX.Interval = Math.Max((b - a) / 10, 0.1);

                    // Ось Y с отступами
                    if (minY != double.MaxValue && maxY != double.MinValue)
                    {
                        if (minY == maxY)
                        {
                            minY -= 1;
                            maxY += 1;
                        }

                        double padding = Math.Max(Math.Abs(maxY - minY) * 0.1, 0.1);
                        chart.ChartAreas[0].AxisY.Minimum = minY - padding;
                        chart.ChartAreas[0].AxisY.Maximum = maxY + padding;
                        chart.ChartAreas[0].AxisY.Interval = Math.Max((maxY - minY) / 10, 0.1);
                    }
                }

                // Добавляем точку минимума
                if (minimumSeries != null && result.IsMinimum)
                {
                    minimumSeries.Points.AddXY(result.MinimumPoint, result.MinimumValue);
                }

                // Добавляем точки итераций
                if (iterationsSeries != null && result.StepByStepIterations != null)
                {
                    foreach (var iteration in result.StepByStepIterations)
                    {
                        iterationsSeries.Points.AddXY(iteration.X, iteration.FunctionValue);
                    }
                }

                // Обновляем легенду
                if (chart.Legends.Count == 0)
                {
                    Legend legend = new Legend();
                    chart.Legends.Add(legend);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении графика: {ex.Message}");
            }
        }

        private void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputsSilent())
                {
                    MessageBox.Show("Сначала заполните все поля корректно", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string function = PreprocessFunction(FunctionTextBox.Text);
                double a = double.Parse(StartIntervalTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double b = double.Parse(EndIntervalTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);

                if (newtonMethod == null || newtonMethod.TestFunctionOnInterval(a, b) == false)
                {
                    newtonMethod = new Calculator.NewtonMethod(function);
                }

                if (newtonMethod != null)
                {
                    double goodStart = newtonMethod.FindGoodStartingPoint(a, b);
                    InitialPointTextBox.Text = goodStart.ToString("F6");
                    MessageBox.Show($"Автоматически подобрана начальная точка: x0 = {goodStart:F6}",
                                  "Автоподбор", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при автоподборе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FunctionTextBox.Text = "";
            InitialPointTextBox.Text = "1";
            PrecisionTextBox.Text = "0.001";
            MaxIterationsTextBox.Text = "100";
            StartIntervalTextBox.Text = "-2";
            EndIntervalTextBox.Text = "2";
            ResultTextBox.Text = "";

            // Очистка графика
            if (chart != null)
            {
                foreach (var series in chart.Series)
                {
                    series.Points.Clear();
                }
            }
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

        private void TestDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Тестовые данные для метода Ньютона
            FunctionTextBox.Text = "x^2 - 4*x + 5";
            InitialPointTextBox.Text = "0";
            PrecisionTextBox.Text = "0.0001";
            MaxIterationsTextBox.Text = "100";
            StartIntervalTextBox.Text = "-5";
            EndIntervalTextBox.Text = "5";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Безопасная проверка элементов
                if (CalculateButton == null ||
                    FunctionTextBox == null ||
                    InitialPointTextBox == null ||
                    PrecisionTextBox == null ||
                    MaxIterationsTextBox == null ||
                    StartIntervalTextBox == null ||
                    EndIntervalTextBox == null)
                {
                    Debug.WriteLine("Один или несколько элементов UI равны null");
                    return;
                }

                bool isValid = ValidateInputsSilent();
                if (CalculateButton != null)
                    CalculateButton.IsEnabled = isValid;

                if (AutoStartButton != null)
                    AutoStartButton.IsEnabled = ValidateIntervalSilent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в TextBox_TextChanged: {ex.Message}");
            }
        }

        private bool ValidateInputsSilent()
        {
            try
            {
                // Проверка наличия элементов
                if (FunctionTextBox == null ||
                    InitialPointTextBox == null ||
                    PrecisionTextBox == null ||
                    MaxIterationsTextBox == null ||
                    StartIntervalTextBox == null ||
                    EndIntervalTextBox == null)
                {
                    return false;
                }

                // Проверка функции
                if (string.IsNullOrWhiteSpace(FunctionTextBox.Text))
                    return false;

                // Проверка начальной точки
                if (!double.TryParse(InitialPointTextBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double x0))
                    return false;

                // Проверка точности
                if (!double.TryParse(PrecisionTextBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double epsilon))
                    return false;

                if (epsilon <= 0)
                    return false;

                // Проверка максимального количества итераций
                if (!int.TryParse(MaxIterationsTextBox.Text, out int maxIterations))
                    return false;

                if (maxIterations <= 0)
                    return false;

                // Проверка интервала
                if (!double.TryParse(StartIntervalTextBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    return false;

                if (!double.TryParse(EndIntervalTextBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                    return false;

                if (a >= b)
                    return false;

                if (x0 < a || x0 > b)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(FunctionTextBox.Text))
            {
                MessageBox.Show("Введите функцию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!double.TryParse(InitialPointTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double x0))
            {
                MessageBox.Show("Некорректное значение начальной точки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!double.TryParse(PrecisionTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double epsilon))
            {
                MessageBox.Show("Некорректное значение точности", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (epsilon <= 0)
            {
                MessageBox.Show("Точность должна быть положительной", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(MaxIterationsTextBox.Text, out int maxIterations))
            {
                MessageBox.Show("Некорректное значение максимального количества итераций", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (maxIterations <= 0)
            {
                MessageBox.Show("Максимальное количество итераций должно быть положительным", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!double.TryParse(StartIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
            {
                MessageBox.Show("Некорректное значение начала интервала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!double.TryParse(EndIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
            {
                MessageBox.Show("Некорректное значение конца интервала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (a >= b)
            {
                MessageBox.Show("Начало интервала должно быть меньше конца", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (x0 < a || x0 > b)
            {
                MessageBox.Show("Начальная точка должна находиться в интервале [a, b]", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool ValidateIntervalSilent()
        {
            try
            {
                if (StartIntervalTextBox == null || EndIntervalTextBox == null)
                    return false;

                if (!double.TryParse(StartIntervalTextBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    return false;

                if (!double.TryParse(EndIntervalTextBox.Text.Replace(",", "."),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                    return false;

                return a < b;
            }
            catch
            {
                return false;
            }
        }
    }
}