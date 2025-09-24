using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
                double root = dichotomy.Solve();

                if (root != 0 || (a == 0 || b == 0))
                {
                    ChartBuilder chart = new ChartBuilder(function);
                    chart.DrawChart(ChartLine, a, b);
                    chart.PointRoot(ChartLine, root);
                    inputRoot.Text = root.ToString();
                }
                else
                {
                    Console.WriteLine("Корень не найден");
                }
            }
            else
            {
                Console.WriteLine("Введите данные!");
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {

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
    }
}

