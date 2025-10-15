using OfficeOpenXml;
using System.IO;
using System;
using OfficeOpenXml.Style;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;


class Excel
{
    private ExcelPackage _package;
    private ExcelWorksheet _sheet;
    private string _filePath;

    static Excel()
    {
        ExcelPackage.License.SetNonCommercialPersonal("My Noncommercial organization");
    }

    public void CreateExcel(string filePath, int N)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }

            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                MessageBox.Show("Не удалось удалить файл :( ");
            }
        }

        _package = new ExcelPackage(new FileInfo(filePath));
        _sheet = _package.Workbook.Worksheets.Count > 0
                ? _package.Workbook.Worksheets[0]
                : _package.Workbook.Worksheets.Add("Sheet1");
        _sheet.Cells[1, 1].Value = "A";
        _sheet.Cells[1, N + 2].Value = "B";

        var headerRange = _sheet.Cells[1, 1, 1, N];

        var mergetToRemove = _sheet.MergedCells
            .Where(merge => _sheet.Cells[merge].Intersect(headerRange) != null)
            .ToList();

        foreach (var merge in mergetToRemove)
        {
            _sheet.Cells[merge].Merge = false;
        }

        headerRange.Merge = true;

        var cellB = _sheet.Cells[1, N + 2];

        _package.Save();
    }

    public void CreateExcelResult(string fullPathResult, int N)
    {
        if (File.Exists(fullPathResult))
        {
            try
            {
                File.Delete(fullPathResult);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                MessageBox.Show("Не удалось удалить прошлый файл!");
            }
        }

        _package = new ExcelPackage(new FileInfo(fullPathResult));
        _sheet = _package.Workbook.Worksheets.Count > 0
            ? _package.Workbook.Worksheets[0]
            : _package.Workbook.Worksheets.Add("Sheet1");

        _sheet.Cells[1, 1].Value = "X";

        var cellB = _sheet.Cells[1, 1];

        _package.Save();
    }

    public void OpenExcel(string fullPath)
    {
        _filePath = fullPath;
        _package = new ExcelPackage(new FileInfo(_filePath));
        _sheet = _package.Workbook.Worksheets[0];
    }

    public void CloseExcel()
    {
        if (_package != null)
        {
            _package.Save();
            _package.Dispose();
            _package = null;
            _sheet = null;
        }
    }

    public void InputDataInExcelManual(int N)
    {
        if (_sheet == null) MessageBox.Show("Excel не открыт!");

        int colB = N + 2;

        for (int row = 2; row <= N + 1; ++row)
        {
            for (int col = 1; col <= N; ++col)
            {
                _sheet.Cells[row, col].Value = 0;
            }

            _sheet.Cells[row, colB].Value = 0;
        }
    }

    public void InputDataInExcelAuto(int N)
    {
        if (_sheet == null) MessageBox.Show("Excel не открыт!");

        Random random = new Random();
        int colB = N + 2;

        for (int row = 2; row <= N + 1; ++row)
        {
            for (int col = 1; col <= N; ++col)
            {
                _sheet.Cells[row, col].Value = random.Next(50);
                _sheet.Cells[row, col].Style.Font.Size = 14;
                _sheet.Cells[row, col].Style.Font.Name = "Segoe UI";
            }

            _sheet.Cells[row, colB].Value = random.Next(50);
            _sheet.Cells[row, colB].Style.Font.Size = 14;
            _sheet.Cells[row, colB].Style.Font.Name = "Segoe UI";
        }
    }

    public void InputResultInExcel(double[] result, int N)
    {
        if (_sheet == null) MessageBox.Show("Excel не открыт!");

        for (int i = 0; i <= N - 1; ++i)
        {
            int row = i + 2;
            _sheet.Cells[row, 1].Value = result[i];
            _sheet.Cells[row, 1].Style.Font.Size = 14;
            _sheet.Cells[row, 1].Style.Font.Name = "Segoe UI";
        }
    }

    public void ReadData(out double[,] matrixA, out double[] vectorB, int N)
    {
        if (_sheet == null) MessageBox.Show("Excel не открыт!");

        matrixA = new double[N, N];
        vectorB = new double[N];

        for (int row = 2; row <= N + 1; ++row)
        {
            for (int col = 1; col <= N; ++col)
            {
                var val = _sheet.Cells[row, col].GetValue<double>();
                matrixA[row - 2, col - 1] = val;
            }

            var valB = _sheet.Cells[row, N + 2].GetValue<double>();
            vectorB[row - 2] = valB;
        }
    }

    public void ExportInExcel(double[] result)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            FileName = "Отчет.xlsx"
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Результаты");
                sheet.Cells[1, 1].Value = "Метод Гауса";

                for (int row = 1; row < result.Length; ++row)
                {
                    sheet.Cells[row + 1, 1].Value = result[row];
                }

                File.WriteAllBytes(saveDialog.FileName, package.GetAsByteArray());
            }

            MessageBox.Show("Файл успешно создан!");
        }
    }
}

