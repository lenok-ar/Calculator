using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Calculator
{
    public partial class Dichotomy : Page
    {
        private string fun { get; set; }
        private double rangeA { get; set; }
        private double rangeB { get; set; }
        private double accuracy { get; set; }

        public Dichotomy()
        {
            InitializeComponent();
        }

        private void HandleBackMenu(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Menu());
        }

        private void TextBox_F(object sender, TextChangedEventArgs e)
        {
            fun = ((TextBox)sender).Text;
        }

        private void TextBox_NumberChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string input = textBox.Text;
            double value;

            if (!double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                if (!double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    value = 0.0;
                    Console.WriteLine("Недопустимый формат данных!");
                }
            }

            switch (textBox.Tag?.ToString())
            {
                case "A":
                    rangeA = value;
                    break;

                case "B":
                    rangeB = value;
                    break;

                case "E":
                    accuracy = value;
                    break;
            }
        }

        private async void HandleCalculate(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(fun) && accuracy > 0)
            {
                try
                {
                    var dichotomyMethod = new DichotomyMethod(fun);

                    if (!dichotomyMethod.TestFunctionOnInterval(rangeA, rangeB))
                    {
                        MessageBox.Show("Функция не определена или имеет разрывы на заданном интервале",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Получаем результат как кортеж
                    var result = await Task.Run(() => dichotomyMethod.FindRoot(rangeA, rangeB, accuracy));

                    // Проверяем ошибку
                    if (!string.IsNullOrEmpty(result.error))
                    {
                        MessageBox.Show(result.error, "Информация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Выводим корень
                    inputRoot.Text = Math.Round(result.root, 8).ToString();

                    // Выводим полную информацию
                    string fullInfo = $"Корень: {result.root:F10}\n" +
                                    $"Значение функции в корне: {result.fValue:E6}\n" +
                                    $"Количество итераций: {result.iterations}\n" +
                                    $"Интервал: [{rangeA:F4}, {rangeB:F4}]\n" +
                                    $"Точность: {accuracy:E6}";

                    fullInfoTextBox.Text = fullInfo;

                    // Построение графика
                    try
                    {
                        ChartBuilder chart = new ChartBuilder(fun);

                        double plotA = Math.Min(rangeA, rangeB);
                        double plotB = Math.Max(rangeA, rangeB);

                        if (Math.Abs(plotB - plotA) > 20)
                        {
                            plotA = result.root - 5;
                            plotB = result.root + 5;
                        }
                        else if (Math.Abs(plotB - plotA) < 0.1)
                        {
                            double center = (plotA + plotB) / 2;
                            plotA = center - 1;
                            plotB = center + 1;
                        }

                        chart.Draw(ChartLine, plotA, plotB);
                        chart.DrawRootPoint(ChartLine, result.root);
                    }
                    catch (Exception ex)
                    {
                        fullInfoTextBox.Text += $"\n\nПримечание: Ошибка построения графика: {ex.Message}";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка вычисления: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите функцию и точность!",
                              "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Метод для очистки графика и полей
        private void ClearAll()
        {
            try
            {
                // Очищаем результаты
                inputRoot.Text = "";
                fullInfoTextBox.Text = "";

                // Очищаем график
                ChartLine.Series.Clear();
                ChartLine.ChartAreas.Clear();
            }
            catch
            {
                // Игнорируем ошибки очистки
            }
        }

        // Очистка при изменении входных данных
        private void ClearResultsOnInputChange()
        {
            inputRoot.Text = "";
            fullInfoTextBox.Text = "";
        }

        // Обновляем обработчики для очистки результатов при изменении входных данных
        private void TextBox_F_Updated(object sender, TextChangedEventArgs e)
        {
            fun = ((TextBox)sender).Text;
            ClearResultsOnInputChange();
        }

        private void TextBox_NumberChanged_Updated(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string input = textBox.Text;
            double value;

            if (!double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                if (!double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    value = 0.0;
                    Console.WriteLine("Недопустимый формат данных!");
                }
            }

            switch (textBox.Tag?.ToString())
            {
                case "A":
                    rangeA = value;
                    break;

                case "B":
                    rangeB = value;
                    break;

                case "E":
                    accuracy = value;
                    break;
            }

            ClearResultsOnInputChange();
        }
    }
}