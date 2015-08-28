﻿using Hik.Communication.ScsServices.Service;

namespace CalculatorCommonLib
{
    /// <summary>
    /// This interface defines methods of calculator service that can be called by clients.
    /// </summary>
    [ScsService]
    public interface ICalculatorService
    {
        int Add(int number1, int number2);

        double Divide(double number1, double number2);

        double multiplicar(double a, double b, ref double resultado, out string mensaje, string[] otrosValores, ref int k, bool haceAlgo, out string[] otrosValores2);
    }
}
