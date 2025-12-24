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
                { "HandleOpenDichtomy", HandleOpenDichotomy },
                { "HandleOpenSlau", HandleOpenSlau}
            };

            if (sender is Border border && border.Tag is string method)
            {
                var data = config.FindMethod(method);
                if (data == null) return;

                NameMethod.Text = data.name;
                DescMethod.Text = data.description;

                if (currentHandler != null) ActionButton.Click -= currentHandler;

                if (actions.ContainsKey(data.action))
                {
                    currentHandler = actions[data.action];
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
    }
}
