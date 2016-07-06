﻿using System;
using CalculatorCommonLib;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;

namespace CalculatorServer
{
    class Program
    {
        static void Main()
        {
            //Create a service application that runs on 10083 TCP port
            var serviceApplication = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(10083));

            //Create a CalculatorService and add it to service application
            serviceApplication.AddService<ICalculatorService, CalculatorService>(new CalculatorService());
            
            //Start service application
            serviceApplication.Start();

            Console.WriteLine("Calculator service is started. Press enter to stop...");
            Console.ReadLine();

            //Stop service application
            serviceApplication.Stop();
        }
    }

    public class CalculatorService : ScsService, ICalculatorService
    {
        public int Add(int number1, int number2)
        {
            return number1 + number2;
        }

        public double Divide(double number1, double number2)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(number2 == 0.0)
            {
                throw new DivideByZeroException("number2 can not be zero!");
            }

            return number1 / number2;
        }

        public double Multiplicar(double a, double b, out double resultado, out string mensaje, string[] otrosValores, ref int k, bool haceAlgo, out string[] otrosValores2)
        {
            otrosValores2 = null;
            mensaje = Guid.NewGuid().ToString();
            resultado = a*b;
            k += (int)resultado; 
            if (haceAlgo)
            {
                otrosValores2 = new [] { "pedro", "almodovar" };
            }
            return resultado;
        }
    }
}
