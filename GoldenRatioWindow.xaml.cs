using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;

namespace Calculator.page
{
    public partial class GoldenRatioWindow : Page
    {
        private GoldenRatioMethod goldenRatioMethod;
        private bool isFindingExtremum = false;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;

        public GoldenRatioWindow()
        {
            InitializeComponent();

            // Проверяем, что элементы были инициализированы
            if (CalculateRootButton == null || CalculateExtremumButton == null)
            {
                MessageBox.Show("Ошибка загрузки элементов управления");
            }

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Инициализация ComboBox
            TaskTypeComboBox.SelectedIndex = 0;
            ExtremumTypeComboBox.SelectedIndex = 0;

            // Инициализация графика
            InitializeChart();

            // Изначально скрываем панель выбора типа экстремума
            ExtremumTypePanel.Visibility = Visibility.Collapsed;

            // Проверяем кнопки на null перед обращением
            if (CalculateExtremumButton != null)
                CalculateExtremumButton.IsEnabled = false;

            if (CalculateRootButton != null)
                CalculateRootButton.IsEnabled = true;
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

            // Добавляем серию для точки решения
            Series solutionSeries = new Series("Решение");
            solutionSeries.ChartType = SeriesChartType.Point;
            solutionSeries.Color = System.Drawing.Color.Red;
            solutionSeries.MarkerSize = 10;
            solutionSeries.MarkerStyle = MarkerStyle.Circle;
            chart.Series.Add(solutionSeries);

            // Добавляем в WindowsFormsHost
            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = chart;

            // Создаем контейнер для графика с указанием полного пространства имен
            System.Windows.Controls.Grid chartGrid = new System.Windows.Controls.Grid();
            chartGrid.Children.Add(host);
            ChartContainer.Content = chartGrid;
        }

        private void TaskTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskTypeComboBox.SelectedIndex == 0) // Поиск корня
            {
                ExtremumTypePanel.Visibility = Visibility.Collapsed;

                if (CalculateRootButton != null)
                    CalculateRootButton.IsEnabled = true;

                if (CalculateExtremumButton != null)
                    CalculateExtremumButton.IsEnabled = false;

                isFindingExtremum = false;
            }
            else // Поиск экстремума
            {
                ExtremumTypePanel.Visibility = Visibility.Visible;

                if (CalculateRootButton != null)
                    CalculateRootButton.IsEnabled = false;

                if (CalculateExtremumButton != null)
                    CalculateExtremumButton.IsEnabled = true;

                isFindingExtremum = true;
            }
        }

        private void CalculateRootButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateSolution(false);
        }

        private void CalculateExtremumButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateSolution(true);
        }

        private void CalculateSolution(bool findExtremum)
        {
            try
            {
                // Проверка входных данных
                if (!ValidateInputs())
                    return;

                // Получение значений
                string function = FunctionTextBox.Text;
                double a = double.Parse(StartTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double b = double.Parse(EndTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                double epsilon = double.Parse(PrecisionTextBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);

                // Создание метода золотого сечения
                goldenRatioMethod = new GoldenRatioMethod(function);

                // Проверка функции на интервале
                if (!goldenRatioMethod.TestFunctionOnInterval(a, b))
                {
                    MessageBox.Show("Функция не определена или имеет разрывы на заданном интервале",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (findExtremum)
                {
                    // Поиск экстремума методом золотого сечения
                    bool findMinimum = ExtremumTypeComboBox.SelectedIndex == 0;
                    GoldenRatioResult result;

                    if (findMinimum)
                        result = goldenRatioMethod.FindMinimum(a, b, epsilon);
                    else
                        result = goldenRatioMethod.FindMaximum(a, b, epsilon);

                    ResultTextBox.Text = $"Экстремум найден методом золотого сечения:\n";
                    ResultTextBox.Text += $"Точка: x = {result.ExtremumPoint:F6}\n";
                    ResultTextBox.Text += $"Значение: f(x) = {result.ExtremumValue:F6}\n";
                    ResultTextBox.Text += $"Тип: {(findMinimum ? "Минимум" : "Максимум")}\n";
                    ResultTextBox.Text += $"Количество итераций: {result.Iterations}";

                    // Обновление графика с точкой экстремума
                    UpdateChart(a, b, result.ExtremumPoint, result.ExtremumValue, findMinimum ? "Минимум" : "Максимум");
                }
                else
                {
                    // Поиск корня методом дихотомии (используем метод золотого сечения для корня)
                    GoldenRatioResult result = goldenRatioMethod.FindRoot(a, b, epsilon);

                    // Получаем значения функции на концах интервала для проверки
                    double fa = goldenRatioMethod.CalculateFunction(a);
                    double fb = goldenRatioMethod.CalculateFunction(b);

                    ResultTextBox.Text = $"Корень найден методом дихотомии:\n";
                    ResultTextBox.Text += $"x = {result.ExtremumPoint:F6}\n";
                    ResultTextBox.Text += $"f(x) = {result.ExtremumValue:F6}";

                    // Обновление графика с точкой корня
                    UpdateChart(a, b, result.ExtremumPoint, result.ExtremumValue, "Корень");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateChart(double a, double b, double solutionX, double solutionY, string solutionType)
        {
            if (goldenRatioMethod == null || chart == null)
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
                        double y = goldenRatioMethod.CalculateFunction(x);
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

                // Добавляем точку решения
                Series solutionSeries = chart.Series["Решение"];
                solutionSeries.Points.Clear();
                solutionSeries.Points.AddXY(solutionX, solutionY);

                // Настраиваем цвет точки решения
                if (solutionType == "Минимум")
                {
                    solutionSeries.Color = System.Drawing.Color.Green;
                    solutionSeries.Name = "Минимум";
                }
                else if (solutionType == "Максимум")
                {
                    solutionSeries.Color = System.Drawing.Color.Orange;
                    solutionSeries.Name = "Максимум";
                }
                else // Корень
                {
                    solutionSeries.Color = System.Drawing.Color.Red;
                    solutionSeries.Name = "Корень";
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

            // Проверка интервала
            if (!double.TryParse(StartTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
            {
                MessageBox.Show("Некорректное значение начала интервала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!double.TryParse(EndTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
            {
                MessageBox.Show("Некорректное значение конца интервала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (a >= b)
            {
                MessageBox.Show("Начало интервала должно быть меньше конца", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            return true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FunctionTextBox.Text = "";
            StartTextBox.Text = "0";
            EndTextBox.Text = "10";
            PrecisionTextBox.Text = "0.001";
            ResultTextBox.Text = "";
            TaskTypeComboBox.SelectedIndex = 0;
            ExtremumTypeComboBox.SelectedIndex = 0;

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
            // Тестовые данные для метода золотого сечения
            if (TaskTypeComboBox.SelectedIndex == 0) // Поиск корня
            {
                FunctionTextBox.Text = "x^2 - 4";
                StartTextBox.Text = "0";
                EndTextBox.Text = "5";
                PrecisionTextBox.Text = "0.0001";
            }
            else // Поиск экстремума
            {
                FunctionTextBox.Text = "x^2 - 4*x + 5";
                StartTextBox.Text = "-5";
                EndTextBox.Text = "5";
                PrecisionTextBox.Text = "0.0001";
                ExtremumTypeComboBox.SelectedIndex = 0; // Минимум
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Валидация при изменении текста
            bool isValid = ValidateInputsSilent();

            // Добавляем проверку на null для кнопок
            if (TaskTypeComboBox.SelectedIndex == 0) // Поиск корня
            {
                if (CalculateRootButton != null)
                    CalculateRootButton.IsEnabled = isValid;
            }
            else // Поиск экстремума
            {
                if (CalculateExtremumButton != null)
                    CalculateExtremumButton.IsEnabled = isValid;
            }
        }

        private bool ValidateInputsSilent()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FunctionTextBox.Text))
                    return false;

                if (!double.TryParse(StartTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double a))
                    return false;

                if (!double.TryParse(EndTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                    return false;

                if (a >= b)
                    return false;

                if (!double.TryParse(PrecisionTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double epsilon))
                    return false;

                if (epsilon <= 0)
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