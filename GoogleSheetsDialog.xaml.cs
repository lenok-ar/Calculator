using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Calculator
{
    public partial class GoogleSheetsDialog : Window
    {
        public string SheetsUrl { get; private set; }

        public GoogleSheetsDialog()
        {
            InitializeComponent();
            txtUrl.Focus();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string inputUrl = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(inputUrl))
            {
                ShowErrorMessageDialog("Введите ссылку на Google Sheets таблицу", "Ошибка ввода");
                return;
            }

            if (!ValidateGoogleSheetsUrlFormat(inputUrl))
            {
                var userConfirmation = ShowWarningMessageDialog(
                    "Ссылка не похожа на Google Sheets таблицу. Продолжить?",
                    "Предупреждение"
                );

                if (userConfirmation == MessageBoxResult.No)
                    return;
            }

            SheetsUrl = inputUrl;
            DialogResult = true;
            Close();
        }

        private bool ValidateGoogleSheetsUrlFormat(string urlToValidate)
        {
            try
            {
                return Regex.IsMatch(urlToValidate, @"docs\.google\.com\/spreadsheets", RegexOptions.IgnoreCase) ||
                       Regex.IsMatch(urlToValidate, @"\/d\/[a-zA-Z0-9-_]+", RegexOptions.IgnoreCase);
            }
            catch (Exception validationException)
            {
                Console.WriteLine($"Ошибка валидации URL: {validationException.Message}");
                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowErrorMessageDialog(string errorMessageText, string errorTitle)
        {
            MessageBox.Show(errorMessageText, errorTitle,
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private MessageBoxResult ShowWarningMessageDialog(string warningMessageText, string warningTitle)
        {
            return MessageBox.Show(warningMessageText, warningTitle,
                                 MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}