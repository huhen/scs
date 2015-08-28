using System;
using CalculatorCommonLib;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Client;

namespace CalculatorClient
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Press enter to connect to server and call methods...");
            Console.ReadLine();

            //Create a client that can call methods of Calculator Service that is running on local computer and 10083 TCP port
            //Since IScsServiceClient is IDisposible, it closes connection at the end of the using block
            using (var client = ScsServiceClientBuilder.CreateClient<ICalculatorService>(new ScsTcpEndPoint("127.0.0.1", 10083)))
            {
                //Connect to the server
                client.Connect();

                //Call a remote method of server
                double resultado = 0;
                
                //resultado = client.ServiceProxy.Divide(42, 3);

                //Write the result to the screen
              //  Console.WriteLine("Result division: " + resultado);

                double resultado2 = 0;
                string mensaje="";
                string[] otrosValores= new string[2] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
                int k = 100;
                string[] otrosValores2=null;
                //                       double multiplicar(double a, double b, ref double resultado, out string mensaje, string[] otrosValores, ref int k, bool haceAlgo, out string[] otrosValores2);
                resultado = client.ServiceProxy.multiplicar(42, 3, ref resultado2, out mensaje, otrosValores, ref k, true , out otrosValores2);

                //Write the result to the screen
                Console.WriteLine("Result multiplicar: " + resultado+ " resultado2: " + resultado2 + " mensaje:" + mensaje + " k: " + k);
                if (otrosValores2 != null)
                    foreach (string c in otrosValores2)
                        Console.WriteLine("Otros valores2 " + c);
            }

            Console.WriteLine("Press enter to stop client application");
            Console.ReadLine();
        }
    }
}
