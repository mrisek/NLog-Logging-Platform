using System; // fundamental classes and commonly-used base classes 
using System.Threading.Tasks; // provides types for asynchronous operations
using System.Threading; // creates and controls a thread, sets priority and gets its status
using static NLogPoC.Networking;

namespace NLogPOC
{
    public class Program
    {
        #region Member variables

        // lock
        private static readonly Object obj = new Object();

        #endregion

        #region Main method

        static void Main(string[] args)
        {
            Console.WriteLine("NLog is a free logging platform for .NET\n");

            //// example of possible log levels
            //WriteLogMessages();
            //WriteParameterizedLogMessagges();

            //Connect("127.0.0.1", "message");

            //TcpListener();

            //ConnectClient();
            //Close();


            Thread t1 = new Thread(StartServer);
            t1.Start();

            // Testing of lock
            lock (obj)
            {
                Thread t2 = new Thread(StartClient);
                t2.Start();
            }

            // Testing of delays
            Task.Run(async () => {
                Thread t3 = new Thread(StartClient);
                await Task.Delay(8000);
                t3.Start();
            });

            //StartServer();
            //StartClient();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        #endregion
    }
}
