using System;
using System.Collections.Generic;
using System.Linq;
using System.Data; // для DataTable
using System.Globalization; // добавляем для CultureInfo

namespace Calculator
{
    public class NewtonMethod
    {
        private string function;
        private Func<double, double> compiledFunction;

        public NewtonMethod(string function)
        {
            this.function = function;
            this.compiledFunction = CompileFunction(function);
        }

        private Func<double, double> CompileFunction(string func)
        {
            return x =>
            {
                try
                {
                    // Заменяем x на значение и приводим к нижнему регистру
                    string expression = func
                        .Replace("x", $"({x.ToString(CultureInfo.InvariantCulture)})")
                        .Replace("X", $"({x.ToString(CultureInfo.InvariantCulture)})")
                        .Replace(" ", "")
                        .ToLower();

                    // Обработка математических функций для DataTable
                    expression = expression
                        .Replace("sin(", "sin(")
                        .Replace("cos(", "cos(")
                        .Replace("tan(", "tan(")
                        .Replace("exp(", "exp(")
                        .Replace("log(", "log(")
                        .Replace("sqrt(", "sqrt(")
                        .Replace("abs(", "abs(")
                        .Replace("^", "**"); // Исправление для степеней

                    // Константы
                    expression = expression
                        .Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture))
                        .Replace("e", Math.E.ToString(CultureInfo.InvariantCulture));

                    return EvaluateExpression(expression);
                }
                catch
                {
                    return double.NaN;
                }
            };
        }

        private double EvaluateExpression(string expression)
        {
            try
            {
                // Используем DataTable для вычисления математических выражений
                var dataTable = new DataTable();
                var result = dataTable.Compute(expression, "");
                return Convert.ToDouble(result);
            }
            catch
            {
                return double.NaN;
            }
        }

        public double CalculateFunction(double x)
        {
            try
            {
                return compiledFunction(x);
            }
            catch
            {
                return double.NaN;
            }
        }

        public bool TestFunctionOnInterval(double a, double b)
        {
            try
            {
                // Проверяем несколько точек на интервале
                int testPoints = 10;
                double step = (b - a) / testPoints;
                int validPoints = 0;

                for (int i = 0; i <= testPoints; i++)
                {
                    double x = a + i * step;
                    double value = CalculateFunction(x);

                    if (!double.IsNaN(value) && !double.IsInfinity(value) && Math.Abs(value) < 1e10)
                    {
                        validPoints++;
                    }
                }

                // Считаем функцию корректной, если хотя бы 70% точек работают
                return validPoints >= testPoints * 0.7;
            }
            catch
            {
                return false;
            }
        }

        public double CalculateFirstDerivative(double x, double h = 1e-5)
        {
            try
            {
                double f_plus = CalculateFunction(x + h);
                double f_minus = CalculateFunction(x - h);

                if (double.IsNaN(f_plus) || double.IsInfinity(f_plus) ||
                    double.IsNaN(f_minus) || double.IsInfinity(f_minus))
                {
                    return double.NaN;
                }

                return (f_plus - f_minus) / (2 * h);
            }
            catch
            {
                return double.NaN;
            }
        }

        public double CalculateSecondDerivative(double x, double h = 1e-4)
        {
            try
            {
                double f_x = CalculateFunction(x);
                double f_plus = CalculateFunction(x + h);
                double f_minus = CalculateFunction(x - h);

                if (double.IsNaN(f_x) || double.IsInfinity(f_x) ||
                    double.IsNaN(f_plus) || double.IsInfinity(f_plus) ||
                    double.IsNaN(f_minus) || double.IsInfinity(f_minus))
                {
                    return double.NaN;
                }

                return (f_plus - 2 * f_x + f_minus) / (h * h);
            }
            catch
            {
                return double.NaN;
            }
        }

        public NewtonResult FindMinimum(double x0, double epsilon, int maxIterations, double a, double b)
        {
            var result = new NewtonResult();
            var iterations = new List<NewtonIteration>();

            double x = x0;
            bool converged = false;
            int iteration = 0;
            string convergenceMessage = "Расчет начат";

            while (iteration < maxIterations && !converged)
            {
                double functionValue = CalculateFunction(x);
                double firstDerivative = CalculateFirstDerivative(x);
                double secondDerivative = CalculateSecondDerivative(x);

                // Проверяем на корректность вычислений
                if (double.IsNaN(firstDerivative) || double.IsInfinity(firstDerivative) ||
                    double.IsNaN(secondDerivative) || double.IsInfinity(secondDerivative))
                {
                    convergenceMessage = "Ошибка вычисления производных";
                    break;
                }

                // Сохраняем итерацию
                iterations.Add(new NewtonIteration
                {
                    Iteration = iteration,
                    X = x,
                    FunctionValue = functionValue,
                    FirstDerivative = firstDerivative,
                    SecondDerivative = secondDerivative
                });

                // Критерий остановки: производная близка к нулю
                if (Math.Abs(firstDerivative) < epsilon)
                {
                    if (secondDerivative > 0)
                    {
                        result.IsMinimum = true;
                        convergenceMessage = $"Минимум найден (|f'(x)| = {Math.Abs(firstDerivative):E3} < ε)";
                    }
                    else if (secondDerivative < 0)
                    {
                        result.IsMinimum = false;
                        convergenceMessage = $"Максимум найден (|f'(x)| = {Math.Abs(firstDerivative):E3} < ε)";
                    }
                    else
                    {
                        result.IsMinimum = false;
                        convergenceMessage = $"Стационарная точка (f''(x) = 0)";
                    }
                    converged = true;
                }
                else if (Math.Abs(secondDerivative) < 1e-12)
                {
                    convergenceMessage = "Вторая производная слишком мала, метод может расходиться";
                    break;
                }
                else
                {
                    // Метод Ньютона: x_{n+1} = x_n - f'(x_n)/f''(x_n)
                    double step = -firstDerivative / secondDerivative;
                    double xNew = x + step;

                    // Ограничиваем слишком большие шаги
                    if (Math.Abs(step) > Math.Abs(b - a))
                    {
                        step = Math.Sign(step) * Math.Abs(b - a) * 0.5;
                        xNew = x + step;
                    }

                    // Проверка на выход за границы
                    if (xNew < a) xNew = a + (b - a) * 0.01;
                    if (xNew > b) xNew = b - (b - a) * 0.01;

                    // Проверка сходимости
                    if (Math.Abs(xNew - x) < epsilon)
                    {
                        x = xNew;
                        result.IsMinimum = secondDerivative > 0;
                        convergenceMessage = $"Сходимость достигнута (Δx = {Math.Abs(xNew - x):E3} < ε)";
                        converged = true;
                    }
                    else
                    {
                        x = xNew;
                    }
                }

                iteration++;
            }

            if (!converged)
            {
                convergenceMessage = $"Достигнуто максимальное количество итераций ({maxIterations})";
            }

            result.MinimumPoint = x;
            result.MinimumValue = CalculateFunction(x);
            result.Iterations = iteration;
            result.FinalDerivative = CalculateFirstDerivative(x);
            result.FinalSecondDerivative = CalculateSecondDerivative(x);
            result.ConvergenceMessage = convergenceMessage;
            result.StepByStepIterations = iterations;

            return result;
        }

        public double FindGoodStartingPoint(double a, double b)
        {
            try
            {
                // Простой поиск лучшей точки сканированием
                int points = 20;
                double step = (b - a) / points;
                double bestX = a;
                double bestValue = double.MaxValue;

                for (int i = 0; i <= points; i++)
                {
                    double x = a + i * step;
                    double value = CalculateFunction(x);

                    if (!double.IsNaN(value) && !double.IsInfinity(value) && value < bestValue)
                    {
                        bestValue = value;
                        bestX = x;
                    }
                }

                return bestX;
            }
            catch
            {
                return (a + b) / 2; // Возвращаем середину в случае ошибки
            }
        }
    }
}