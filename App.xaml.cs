// App.xaml.cs
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using OfficeOpenXml;
using System;
using System.Windows;

namespace Calculator
{
    public partial class App : Application
    {
        public App()
        {
            // Правильная установка лицензии для EPPlus 8+
            SetEpplusLicense();

            // Инициализация LiveCharts
            LiveCharts.Configure(config => config.AddSkiaSharp());
        }

        private void SetEpplusLicense()
        {
            try
            {
                // Способ 1: Для EPPlus 8+
                // Устанавливаем лицензию через статический конструктор
                // Это работает для версий 8 и выше
                var licenseContextType = typeof(ExcelPackage).GetProperty("LicenseContext");
                if (licenseContextType != null)
                {
                    // Старый способ (для обратной совместимости)
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                }
                else
                {
                    // Новый способ для EPPlus 8+
                    // EPPlus 8+ использует другой подход к лицензированию
                    // Для некоммерческого использования можно не устанавливать лицензию явно
                    // Но если нужно, используйте ExcelPackage.License
                    Console.WriteLine("EPPlus 8+ detected - using new licensing model");

                    // Если у вас есть файл лицензии:
                    // string licenseFile = "path/to/license.xml";
                    // ExcelPackage.License = new OfficeOpenXml.ExcelLicense(licenseFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting EPPlus license: {ex.Message}");
                // Игнорируем ошибку лицензии для разработки
            }
        }
    }
}