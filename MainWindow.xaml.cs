using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyProject
{
    public partial class MainWindow : Window
    {
        private string function;
        private double a;
        private double b;
        private double E;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dichotomy dichotomy = new Dichotomy(function, a, b, E);

            if (!string.IsNullOrEmpty(function) && E > 0)
            {
                if (!dichotomy.CheckInterval())
                {
                    MessageBox.Show("Выберите другой интервал.");
                    return;
                }
                
                double root = dichotomy.Solve();

                if (root != 0 || (a != 0 && b != 0))
                {
                    ChartBuilder chart = new ChartBuilder(function);
                    chart.DrawChart(ChartLine, a, b);
                    inputRoot.Text = root.ToString();
                    try
                    {
                        chart.PointRoot(ChartLine, root);
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Корень не найден");
                }
            }
            else
            {
                MessageBox.Show("Введите данные!");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            inputF.Text = ""; 
            inputA.Text = "";
            inputB.Text = "";
            inputE.Text = "";
            inputRoot.Text = "";
            
            ChartLine.Series.Clear();
            
            function = "";
            a = 0;
            b = 0;
            E = 0;
            
            inputA.BorderBrush = Brushes.Gray;
            inputB.BorderBrush = Brushes.Gray;
            inputE.BorderBrush = Brushes.Gray;
                     
            MessageBox.Show("Данные очищены!");
        }

        private void TextBox_TextChanged_f(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            function = textBox.Text;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string input = textBox.Text.Replace(".", ",");

            if (!double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value = 0.0;
                textBox.Foreground = Brushes.Red;
                textBox.BorderBrush = Brushes.Red;
                Console.WriteLine("Недопустимый формат данных!");
            }
            else
            {
                textBox.Foreground = Brushes.Green;
                textBox.BorderBrush = Brushes.Green;
            }

            switch (textBox.Tag?.ToString())
            {
                case "inputA":
                    a = value;
                    break;

                case "inputB":
                    b = value;
                    break;

                case "inputE":
                    E = value;
                    break;
            }
        }

        private void Method_Dichotomy(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Метод половинного деления (метод дихотомии или метод бисекции)\n\n" +
                "Данный метод описывает алгоритм нахождение корней (нулей) функции. " +
                "Чтобы найти минимум целевой функции методом дихотомии используйте этот калькулятор.");
        }
    }
}

