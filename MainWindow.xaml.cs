using LiveChartsCore;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Menu());
        }
    }
}
