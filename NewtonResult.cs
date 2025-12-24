using System;
using System.Collections.Generic;

namespace Calculator
{
    public class NewtonResult
    {
        public double MinimumPoint { get; set; }
        public double MinimumValue { get; set; }
        public int Iterations { get; set; }
        public double FinalDerivative { get; set; }
        public double FinalSecondDerivative { get; set; }
        public bool IsMinimum { get; set; }
        public string ConvergenceMessage { get; set; }
        public List<NewtonIteration> StepByStepIterations { get; set; }

        public NewtonResult()
        {
            StepByStepIterations = new List<NewtonIteration>();
        }
    }

    public class NewtonIteration
    {
        public int Iteration { get; set; }
        public double X { get; set; }
        public double FunctionValue { get; set; }
        public double FirstDerivative { get; set; }
        public double SecondDerivative { get; set; }
    }
}