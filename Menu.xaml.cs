using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Calculator.page;

namespace Calculator
{
    public partial class Menu : Page
    {
        private RoutedEventHandler currentHandler;

        public Menu()
        {
            InitializeComponent();
        }

        private void HandleChangeMethods(object sender, RoutedEventArgs e)
        {
            config config = new config();
            var actions = new Dictionary<string, RoutedEventHandler>
            {
                { "HandleOpenDichotomy", HandleOpenDichotomy },
                { "HandleOpenSlau", HandleOpenSlau },
                { "HandleOpenGoldenRatio", HandleOpenGoldenRatio },
                { "HandleOpenNewton", HandleOpenNewton },
                { "HandleOpenSorting", HandleOpenSorting },
                { "HandleOpenIntegral", HandleOpenIntegral },
                { "HandleOpenCoordinateDescent", HandleOpenCoordinateDescent },
                { "HandleOpenLeastSquares", HandleOpenLeastSquares }
            };

            if (sender is Border border && border.Tag is string method)
            {
                // Теперь используется ОДИН конкретный класс
                MethodConfig data = config.FindMethod(method);
                if (data == null) return;

                NameMethod.Text = data.Name;
                DescMethod.Text = data.Description;

                if (currentHandler != null)
                    ActionButton.Click -= currentHandler;

                if (actions.ContainsKey(data.Action))
                {
                    currentHandler = actions[data.Action];
                    ActionButton.Click += currentHandler;
                }
            }
        }

        private void HandleOpenDichotomy(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Dichotomy());
        }

        private void HandleOpenSlau(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Slau());
        }

        private void HandleOpenGoldenRatio(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new GoldenRatioWindow());
        }

        private void HandleOpenNewton(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new NewtonMethodWindow());
        }

        private void HandleOpenSorting(object sender, RoutedEventArgs e)
        {
            var sortingWindow = new SortingAlgorithms();
            sortingWindow.Show();
        }

        private void HandleOpenIntegral(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new IntegralWindow());
        }

        private void HandleOpenCoordinateDescent(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new CoordinateDescent());
        }

        private void HandleOpenLeastSquares(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LeastSquaresPage());
        }
    }
}