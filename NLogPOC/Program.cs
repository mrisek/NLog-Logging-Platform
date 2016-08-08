using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets; // provides client connections for TCP network services
using NLog;
using System.Net;
using System.Threading;
using System.IO;

namespace NLogPOC
{
    public sealed class Program : IDisposable
    {
        #region Member variables

        // create a logger instance with the same name of the class
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // it's also possible to control the Logger's name
        //Logger logger = LogManager.GetLogger("MyClassName")

        // create new client istance
        private static Socket client;

        // create new netstream
        private static NetworkStream netstream;

        // declare enumeration that consists of different NLog levels
        public enum Logs : byte { Trace = 1, Debug, Info, Warn, Error, Fatal };

        #endregion

        #region Public methods

        #region Logging

        /// <summary>   
        /// Method for writing sample diagnostic messages at six different log levels
        /// Those are Trace, Debug, Info, Warn, Error and Fatal level
        /// </summary>
        public static void WriteLogMessages()
        {
            logger.Trace("Sample trace message");
            logger.Debug("Sample debug message");
            logger.Info("Sample informational message");
            logger.Warn("Sample warning message");
            logger.Error("Sample error message");
            logger.Fatal("Sample fatal errror message");

            // alternatively you can call the Log() method and pass log level as the parameter
            logger.Log(LogLevel.Info, "Sample informational message");
        }

        /// <summary>
        /// Method for writing parameterizedLogMessages at six different log levels
        /// </summary>
        public static void WriteParameterizedLogMessagges()
        {
            // example of custom variables that are used for formating the string message
            int a = 23;
            int b = 54;

            // This is preferable way of formating string instead of String.Format() or concatenation due to NLog performace
            logger.Trace("Sample trace message, a={0}, b={1}", a, b);
            logger.Debug("Sample debug message, a={0}, b={1}", a, b);
            logger.Info("Sample informational message, a={0}, b={1}", a, b);
            logger.Trace("Sample warning message, a={0}, b={1}", a, b);
            logger.Trace("Sample error message, a={0}, b={1}", a, b);
            logger.Trace("Sample fatal errror message, a={0}, b={1}", a, b);
            logger.Log(LogLevel.Info, "Sample informational message, a={0}, b={1}", a, b);
        }

        /// <summary>
        /// Method for creating six possible NLog levels with custom text message
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="message"></param>
        public static void LogMessage(Logs logs, string message)
        {
            switch (logs)
            {
                case Logs.Trace:
                    logger.Trace(message);
                    break;
                case Logs.Debug:
                    logger.Debug(message);
                    break;
                case Logs.Info:
                    logger.Info(message);
                    break;
                case Logs.Warn:
                    logger.Warn(message);
                    break;
                case Logs.Error:
                    logger.Error(message);
                    break;
                case Logs.Fatal:
                    logger.Fatal(message);
                    break;
            }
        }

        /// <summary>
        /// Method for creating six possible NLog levels with custom text message and parameter
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="message"></param>
        /// <param name="param"></param>
        public static void LogMessage(Logs logs, string message, string param)
        {
            switch (logs)
            {
                case Logs.Trace:
                    logger.Trace("{0} - {1}", message, param);
                    break;
                case Logs.Debug:
                    logger.Debug("{0} - {1}", message, param);
                    break;
                case Logs.Info:
                    logger.Info("{0} - {1}", message, param);
                    break;
                case Logs.Warn:
                    logger.Warn("{0} - {1}", message, param);
                    break;
                case Logs.Error:
                    logger.Error("{0} - {1}", message, param);
                    break;
                case Logs.Fatal:
                    logger.Fatal("{0} - {1}", message, param);
                    break;
            }
        }

        #endregion

        #region Socket

        /// <summary>
        /// Method for establishing a TcpClient connection
        /// </summary>
        /// <param name="server"></param>
        /// <param name="message"></param>
        public static void Connect(String server, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 8080;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        /// <summary>
        /// Method for closing TcpClient connection
        /// </summary>
        public static void Close()
        {
            try
            {
                // Create a client that will connect to a 
                // server listening on the contosoServer computer
                // at port 11000.
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect("127.0.0.1", 8080);
                // Get the stream used to read the message sent by the server.
                NetworkStream networkStream = tcpClient.GetStream();
                // Set a 10 millisecond timeout for reading.
                networkStream.ReadTimeout = 10;
                // Read the server message into a byte buffer.
                byte[] bytes = new byte[1024];
                networkStream.Read(bytes, 0, 1024);
                //Convert the server's message into a string and display it.
                string data = Encoding.UTF8.GetString(bytes);
                Console.WriteLine("Server sent message: {0}", data);




                networkStream.Close();
                tcpClient.Close();

            }
            catch (System.IO.IOException e) 
            {
                Console.Write("Socket exception: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket exception: {0}", e);
            }
        }

        /// <summary>
        /// Method for connecting a client to existing server
        /// </summary>
        public static void ConnectClient()
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect("127.0.0.1", 8080);
                //DisconnectClient(tcpClient);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket exception: {0}", e);
            }
        }

        /// <summary>
        /// Method that disposes TcpClient instance and disconnects the TCP connenction
        /// </summary>
        /// <param name="client"></param>
        public static void DisconnectClient(TcpClient client)
        {
            client.GetStream().Close();
            client.Close();
        }

        public static void TcpListener()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests
                    // You could also user server.AcceptSocket() here
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }



        #endregion

        #region TEST SOCKET REGION

        [STAThread]
        static void StartServer()
        {
            try
            {
                Console.Title = "Server";

                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                TcpListener tcpListener = new TcpListener(ipAddress, 8080);
                tcpListener.Start();
                Console.WriteLine("\nServer is running");

                try
                {
                    while (true)
                    {
                        client = tcpListener.AcceptSocket();

                        if (client.Connected)
                        {
                            Console.WriteLine("Client connected - " + client.RemoteEndPoint.ToString());
                            Thread thread = new Thread(new ParameterizedThreadStart(listenClient));
                            thread.Start(client);

                            byte[] toBytes = Encoding.ASCII.GetBytes("Wellcome!");
                            client.Send(toBytes);

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Server is already running! {0}", e);
            }
        }



        static void listenClient(object data)
        {
            while (client.Connected)
            {
                try
                {
                    client = (Socket)data;
                    netstream = new NetworkStream(client);
                    StreamWriter streamWriter = new StreamWriter(netstream); // to client
                    StreamReader streamReader = new StreamReader(netstream); // from client

                    byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Are you receiving this message?");
                    netstream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                    var a = streamReader.ReadLine();
                    Console.Write(a);
                }
                catch (Exception e)
                {

                }
            }
        }


        [STAThread]
        static void StartClient()
        {
            Console.Title = "Client";

            TcpClient client = new TcpClient();
            Console.WriteLine("Connecting...");

            client.Connect("127.0.0.1", 8080);
            string remoteEndpoint = client.Client.RemoteEndPoint.ToString();

            Console.WriteLine("Connected {0}", remoteEndpoint);

            NetworkStream netstream = client.GetStream();
            StreamWriter streamWriter = new StreamWriter(netstream);
            StreamReader streamReader = new StreamReader(netstream);

            try
            {
                Thread.Sleep(5000);               //1000 milliseconds is one second.
                client.GetStream().Close();
                client.Close();
            }
            catch (Exception e)
            {

            }

            //client.Close();
            if (!client.Connected)
                Console.WriteLine("Disconnected {0}", remoteEndpoint);
        }




        #endregion

        #endregion

        #region Main method

        static void Main(string[] args)
        {
            Console.WriteLine("NLog is a free logging platform for .NET\n");
            //WriteLogMessages();
            //WriteParameterizedLogMessagges();

            //Close();
            //Connect("127.0.0.1", "message");
            //ConnectClient();
            //TcpListener();

            StartServer();

            Close();

           // StartClient();
            LogMessage(Logs.Fatal, "fatalna greska");
            LogMessage(Logs.Info, "neki info message", "intern name");

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
