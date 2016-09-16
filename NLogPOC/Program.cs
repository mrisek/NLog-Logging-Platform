using System;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

namespace NLog
{
    public sealed class Program : IDisposable
    {
        #region Member variables

        // lock
        private static readonly Object obj = new Object();

        #endregion

        #region Main method

        static void Main(string[] args)
        {
            Networking networking = new Networking();
            Logging logging = new Logging();

            Console.WriteLine("NLog is a free logging platform for .NET\n");

            //// example of possible log levels
            //logging.WriteLogMessages();
            //logging.WriteParameterizedLogMessagges();

            //Thread t1 = new Thread(networking.StartServer);
            //t1.Start();

            //// Testing of lock
            //lock (obj)
            //{
            //    Thread t2 = new Thread(networking.StartClient);
            //    t2.Start();
            //}

            //// Testing of delays
            //Task.Run(async () => {
            //    Thread t3 = new Thread(networking.StartClient);
            //    await Task.Delay(8000);
            //    t3.Start();
            //});

            networking.StartServer(ConfigurationManager.AppSettings["IpAddress"], int.Parse(ConfigurationManager.AppSettings["SocketPort"]));
            networking.StartClient(ConfigurationManager.AppSettings["IpAddress"], int.Parse(ConfigurationManager.AppSettings["SocketPort"]));

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
