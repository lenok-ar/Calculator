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

namespace Calculator.page
{
    public partial class NewtonMethodWindow : Page
    {
        private NewtonMethod newtonMethod;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;

        public NewtonMethodWindow()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Инициализация графика
            InitializeChart();
        }

        private void InitializeChart()
        {
            // Создаем Windows Forms Chart
            chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart.Size = new System.Drawing.Size(600, 400);

            // Настраиваем область графика
            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "x";
            chartArea.AxisY.Title = "f(x)";
            chartArea.AxisX.LabelStyle.Format = "F2";
            chartArea.AxisY.LabelStyle.Format = "F2";
            chart.ChartAreas.Add(chartArea);

            // Добавляем серию для функции
            Series functionSeries = new Series("f(x)");
            functionSeries.ChartType = SeriesChartType.Line;
            functionSeries.Color = System.Drawing.Color.Blue;
            functionSeries.BorderWidth = 2;
            chart.Series.Add(functionSeries);

            // Добавляем серию для точки минимума
            Series minimumSeries = new Series("Минимум");
            minimumSeries.ChartType = SeriesChartType.Point;
            minimumSeries.Color = System.Drawing.Color.Red;
            minimumSeries.MarkerSize = 10;
            minimumSeries.MarkerStyle = MarkerStyle.Circle;
            chart.Series.Add(minimumSeries);

            // Добавляем серию для итераций
            Series iterationsSeries = new Series("Итерации");
            iterationsSeries.ChartType = SeriesChartType.Point;
            iterationsSeries.Color = System.Drawing.Color.Green;
            iterationsSeries.MarkerSize = 6;
            iterationsSeries.MarkerStyle = MarkerStyle.Triangle;
            chart.Series.Add(iterationsSeries);

            // Добавляем в WindowsFormsHost
            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = chart;

            // Создаем контейнер для графика
            System.Windows.Controls.Grid chartGrid = new System.Windows.Controls.Grid();
            chartGrid.Children.Add(host);
            ChartContainer.Content = chartGrid;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка входных данных
                if (!ValidateInputs())
                    return;

                // Получение значений
                string function = FunctionTextBox.Text;
                double x0 = double.Parse(InitialPointTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double epsilon = double.Parse(PrecisionTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                int maxIterations = int.Parse(MaxIterationsTextBox.Text);
                double a = double.Parse(StartIntervalTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double b = double.Parse(EndIntervalTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);

                // Создание метода Ньютона
                newtonMethod = new NewtonMethod(function);

                // Проверка функции на интервале
                if (!newtonMethod.TestFunctionOnInterval(a, b))
                {
                    MessageBox.Show("Функция не определена или имеет разрывы на заданном интервале",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Поиск минимума методом Ньютона
                NewtonResult result = newtonMethod.FindMinimum(x0, epsilon, maxIterations, a, b);

                // Отображение результатов
                DisplayResults(result);

                // Обновление графика
                UpdateChart(a, b, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayResults(NewtonResult result)
        {
            int decimalPlaces = GetDecimalPlaces(result.MinimumPoint);

            ResultTextBox.Text = $"Метод Ньютона для поиска минимума:\n\n";
            ResultTextBox.Text += $"Точка минимума: x = {result.MinimumPoint:F6}\n";
            ResultTextBox.Text += $"Значение функции: f(x) = {result.MinimumValue:F6}\n";
            ResultTextBox.Text += $"Количество итераций: {result.Iterations}\n";
            ResultTextBox.Text += $"Первая производная: f'(x) = {result.FinalDerivative:E4}\n";
            ResultTextBox.Text += $"Вторая производная: f''(x) = {result.FinalSecondDerivative:E4}\n";
            ResultTextBox.Text += $"Статус: {(result.IsMinimum ? "Минимум найден" : "Минимум не найден")}\n";
            ResultTextBox.Text += $"Сообщение: {result.ConvergenceMessage}";

            // Очистка списка итераций
            IterationsListBox.Items.Clear();

            // Добавление пошаговых итераций
            if (result.StepByStepIterations != null && result.StepByStepIterations.Any())
            {
                foreach (var iteration in result.StepByStepIterations)
                {
                    string iterationText = $"Итерация {iteration.Iteration + 1}: ";
                    iterationText += $"x = {iteration.X:F6}, ";
                    iterationText += $"f(x) = {iteration.FunctionValue:F6}, ";
                    iterationText += $"f' = {iteration.FirstDerivative:E3}, ";
                    iterationText += $"f'' = {iteration.SecondDerivative:E3}";

                    IterationsListBox.Items.Add(iterationText);
                }

                // Прокрутка к последней итерации
                if (IterationsListBox.Items.Count > 0)
                {
                    IterationsListBox.ScrollIntoView(IterationsListBox.Items[IterationsListBox.Items.Count - 1]);
                }
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

                // Генерация точек для графика функции
                int pointsCount = 400;
                double step = (b - a) / pointsCount;

                Series functionSeries = chart.Series["f(x)"];

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

                // Настраиваем ось X
                chart.ChartAreas[0].AxisX.Minimum = a;
                chart.ChartAreas[0].AxisX.Maximum = b;
                chart.ChartAreas[0].AxisX.Interval = Math.Max((b - a) / 10, 0.1);

                // Настраиваем ось Y с отступами
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

                // Добавляем точку минимума
                Series minimumSeries = chart.Series["Минимум"];
                minimumSeries.Points.Clear();
                if (result.IsMinimum)
                {
                    minimumSeries.Points.AddXY(result.MinimumPoint, result.MinimumValue);
                }

                // Добавляем точки итераций
                Series iterationsSeries = chart.Series["Итерации"];
                iterationsSeries.Points.Clear();
                if (result.StepByStepIterations != null)
                {
                    foreach (var iteration in result.StepByStepIterations)
                    {
                        iterationsSeries.Points.AddXY(iteration.X, iteration.FunctionValue);
                    }
                }

                // Обновляем легенду
                chart.Legends.Clear();
                Legend legend = new Legend();
                chart.Legends.Add(legend);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении графика: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            // Проверка функции
            if (string.IsNullOrWhiteSpace(FunctionTextBox.Text))
            {
                MessageBox.Show("Введите функцию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка начальной точки
            if (!double.TryParse(InitialPointTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double x0))
            {
                MessageBox.Show("Некорректное значение начальной точки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка точности
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

            // Проверка максимального количества итераций
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

            // Проверка интервала
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

            // Проверка, что начальная точка в интервале
            if (x0 < a || x0 > b)
            {
                MessageBox.Show("Начальная точка должна находиться в интервале [a, b]", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private int GetDecimalPlaces(double value)
        {
            if (value == 0) return 6;

            string str = Math.Abs(value).ToString(CultureInfo.InvariantCulture);
            if (str.Contains("."))
            {
                int decimalPlaces = str.Split('.')[1].Length;
                return Math.Min(Math.Max(decimalPlaces, 3), 12);
            }

            return 6;
        }

        private void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(StartIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a) ||
                    !double.TryParse(EndIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                {
                    MessageBox.Show("Сначала укажите корректный интервал [a, b]", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newtonMethod == null && !string.IsNullOrWhiteSpace(FunctionTextBox.Text))
                {
                    newtonMethod = new NewtonMethod(FunctionTextBox.Text);
                }

                if (newtonMethod != null)
                {
                    double goodStart = newtonMethod.FindGoodStartingPoint(a, b);
                    InitialPointTextBox.Text = goodStart.ToString("F3");
                    MessageBox.Show($"Автоматически подобрана начальная точка: x0 = {goodStart:F3}",
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
            IterationsListBox.Items.Clear();

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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    saveDialog.Title = "Экспорт результатов";
                    saveDialog.DefaultExt = "txt";
                    saveDialog.FileName = $"newton_method_results_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                    if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string content = $"Результаты метода Ньютона\n";
                        content += $"Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                        content += $"Функция: {FunctionTextBox.Text}\n";
                        content += $"Начальная точка: {InitialPointTextBox.Text}\n";
                        content += $"Точность: {PrecisionTextBox.Text}\n";
                        content += $"Макс. итераций: {MaxIterationsTextBox.Text}\n";
                        content += $"Интервал: [{StartIntervalTextBox.Text}, {EndIntervalTextBox.Text}]\n\n";
                        content += $"РЕЗУЛЬТАТЫ:\n{ResultTextBox.Text}\n\n";
                        content += $"ПОШАГОВЫЕ ИТЕРАЦИИ:\n";

                        foreach (var item in IterationsListBox.Items)
                        {
                            content += $"{item}\n";
                        }

                        File.WriteAllText(saveDialog.FileName, content);
                        MessageBox.Show("Результаты успешно экспортированы", "Экспорт",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
            // Валидация при изменении текста
            bool isValid = ValidateInputsSilent();
            CalculateButton.IsEnabled = isValid;
            AutoStartButton.IsEnabled = ValidateIntervalSilent();
        }

        private bool ValidateInputsSilent()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FunctionTextBox.Text))
                    return false;

                if (!double.TryParse(InitialPointTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double x0))
                    return false;

                if (!double.TryParse(PrecisionTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double epsilon))
                    return false;

                if (epsilon <= 0)
                    return false;

                if (!int.TryParse(MaxIterationsTextBox.Text, out int maxIterations) || maxIterations <= 0)
                    return false;

                if (!double.TryParse(StartIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    return false;

                if (!double.TryParse(EndIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
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

        private bool ValidateIntervalSilent()
        {
            try
            {
                if (!double.TryParse(StartIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    return false;

                if (!double.TryParse(EndIntervalTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                    return false;

                if (a >= b)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}