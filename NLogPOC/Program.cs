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
using System.Runtime.Serialization.Formatters.Soap;

namespace NLogPOC
{
    internal sealed class Program : IDisposable
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

        // stores list of serializable test objects
        private Dictionary<int, TestSimpleObject> m_listConversations = new Dictionary<int, TestSimpleObject>();

        // lock
        private static readonly Object obj = new Object();



        #endregion

        #region Public methods

        #region Logging

        /*
            Six types of log levels in NLog:

            1) Trace - very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
            2) Debug - debugging information, less detailed than trace, typically not enabled in production environment.
            3) Info - information messages, which are normally enabled in production environment
            4) Warn - warning messages, typically for non-critical issues, which can be recovered or which are temporary failures
            5) Error - error messages - most of the time these are Exceptions
            6) Fatal - very serious errors!
        */

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
            logger.Fatal("Sample fatal error message");

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

        /// <summary>
        /// Method for creating listeners for incoming connection requests and sending messages to clients
        /// </summary>
        [STAThread]
        static void StartServer()
        {
            try
            {
                // Set the title displayed in the console title bar
                Console.Title = "Server";

                // Set listener for incoming connection requests
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                TcpListener tcpListener = new TcpListener(ipAddress, 8080);
                tcpListener.Start();
                // Create log messages by using NLog logging platform
                LogMessage(Logs.Info, "Server is running");
                Console.WriteLine("\nServer is running");

                try
                {
                    while (true)
                    {
                        // accept a pending connection request
                        client = tcpListener.AcceptSocket();
                        string remoteEndPoint = "";

                        // if socket is connected to a remote host, send server message
                        if (client.Connected)
                        {
                            // get client remote endpoint
                            remoteEndPoint = client.RemoteEndPoint.ToString();
                            LogMessage(Logs.Info, "Client connected", remoteEndPoint);
                            Console.WriteLine("Client connected - " + remoteEndPoint);

                            // send message to client after connection is established
                            byte[] toBytes = Encoding.ASCII.GetBytes("Wellcome!");
                            // send data to a connected Socket
                            client.Send(toBytes);
                            LogMessage(Logs.Info, "Server message sent to", remoteEndPoint);
                            Console.WriteLine("Server message sent to {0}", remoteEndPoint);

                            // continue conversation on a new thread
                            Thread thread = new Thread(new ParameterizedThreadStart(listenClient));
                            thread.Start(client);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogMessage(Logs.Error, "Server error: ", e.ToString());
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                LogMessage(Logs.Debug, "Server is already running");
                //Console.WriteLine("Server is already running! {0}", e);
            }
        }

        /// <summary>
        /// Method for communicating with the client by writing data to the Network Stream on a new thread
        /// </summary>
        /// <param name="data"></param>
        static void listenClient(object data)
        {
            while (client.Connected)
            {
                try
                {
                    client = (Socket)data;
                    netstream = new NetworkStream(client);

                    // create stream for writing characters in a particular encoding
                    StreamWriter streamWriter = new StreamWriter(netstream);
                    byte[] buffer = Encoding.ASCII.GetBytes(" Are you receiving this message?");
                    // send message to client by writing data to the NetworkStream
                    netstream.Write(buffer, 0, buffer.Length);

                    // read the client message into a byte buffer
                    buffer = new byte[1024];
                    netstream.Read(buffer, 0, 1024);
                    // convert the client message into a string and display it
                    string readData = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("Client {0} sent message: {1}", client.RemoteEndPoint, readData);
                }
                catch (IOException e)
                {
                    LogMessage(Logs.Error, "IOException: ", e.ToString());
                    Console.WriteLine("An existing connection was forcibly closed", e.ToString());
                }
                catch (Exception e)
                {
                    LogMessage(Logs.Error, "Unable to communicate with the client on a new thread", e.ToString());
                    Console.WriteLine("Unable to communicate with the client on a new thread", e.ToString());
                }
            }
        }

        /// <summary>
        /// Method for instanting a client connections, sending messages to server, reading messages from server and closing TCP connection
        /// </summary>
        [STAThread]
        static void StartClient()
        {
            // set the title to display in the console title bar
            Console.Title = "Client";
            string remoteEndpoint = "";
            TcpClient tcpClient = new TcpClient();
            Console.WriteLine("Connecting...");

            try
            {
                // connect the client to the specified port on the specifed host
                tcpClient.Connect("127.0.0.1", 8080);
                // get socket remote endpoint
                remoteEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                Console.WriteLine("Connected - {0}", remoteEndpoint);
                LogMessage(Logs.Info, "Connected ", remoteEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect the client", e.ToString());
                LogMessage(Logs.Error, "Unable to connect the client", e.ToString());
            }

            try
            {
                // Get the stream used to read the message sent by the server.
                NetworkStream networkStream = tcpClient.GetStream();
                // Read the server message into a byte buffer.
                byte[] bytes = new byte[1024];
                networkStream.Read(bytes, 0, 1024);
                //Convert the server's message into a string and display it.
                string data = Encoding.UTF8.GetString(bytes);
                Console.WriteLine("Server sent message: {0}", data);

                if (data.Length != 0)
                {
                    // encode all the characters in the specifedstring into a sequence of bytes
                    byte[] buffer = Encoding.ASCII.GetBytes("\nI got your message, thank you!");
                    // write data to Network Stream in order to send return message to server
                    networkStream.Write(buffer, 0, buffer.Length);
                    LogMessage(Logs.Info, "Response was sent by client", remoteEndpoint);
                    Console.WriteLine("Response was sent by client {0}", remoteEndpoint);


                    using (var stream = tcpClient.GetStream())
                    {
                        // tu dolazi kolekcija
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Unable to read server message {0}", e.ToString());
                LogMessage(Logs.Error, "Unable to read server message", e.ToString());
            }

            try
            {
                // wait for 5 seconds and then disconnect, 1000 milliseconds is one second.
                Thread.Sleep(5000);
                // close the current stream and release any resources associated with the current stream
                tcpClient.GetStream().Close();
                // dispose client instance and request closing of underlying TCP connection
                tcpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to close client", remoteEndpoint);
                LogMessage(Logs.Error, "Unable to close client.", e.ToString());
            }

            // check if client is disconnected
            if (!tcpClient.Connected)
            {
                Console.WriteLine("Disconnected {0}", remoteEndpoint);
                LogMessage(Logs.Info, "Client disconnected", remoteEndpoint);
            }
            else
            {
                Console.WriteLine("The underlying TCP connection was not closed by client {0}", remoteEndpoint);
                LogMessage(Logs.Warn, "The underlying TCP connection was not closed by client {0}", remoteEndpoint);
            }

        }

        #endregion

        #endregion

        #region Main method

        static void Main(string[] args)
        {
            Console.WriteLine("NLog is a free logging platform for .NET\n");

            // example of possible log levels
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

        #region IDisposable members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Collection serialization

        // A test object that needs to be serialized.
        [Serializable()]
        public class TestSimpleObject
        {
            public int member1;
            public string member2;
            public string member3;
            public double member4;

            // A field that is not serialized.
            [NonSerialized()]
            public string member5;

            public TestSimpleObject()
            {
                member1 = 11;
                member2 = "hello";
                member3 = "hello";
                member4 = 3.14159265;
                member5 = "hello world!";
            }

            public void Print()
            {
                Console.WriteLine("member1 = '{0}'", member1);
                Console.WriteLine("member2 = '{0}'", member2);
                Console.WriteLine("member3 = '{0}'", member3);
                Console.WriteLine("member4 = '{0}'", member4);
                Console.WriteLine("member5 = '{0}'", member5);
            }
        }


        static Stream CreateSerializedCollection()
        {
            //Creates a new TestSimpleObject object.
            TestSimpleObject obj = new TestSimpleObject();

            Console.WriteLine("Before serialization the object contains: ");
            obj.Print();

            //Opens a file and serializes the object into it in binary format.
            Stream stream = File.Open("data.xml", FileMode.Create);
            SoapFormatter formatter = new SoapFormatter();

            //BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);
            stream.Close();

            return stream;
        }

        static void DeserializeCollection(Stream stream)
        {
            TestSimpleObject obj = new TestSimpleObject();
            //Empties obj.
            obj = null;

            //Opens file "data.xml" and deserializes the object from it.
            stream = File.Open("data.xml", FileMode.Open);

            SoapFormatter formatter = new SoapFormatter();
            formatter = new SoapFormatter();

            //formatter = new BinaryFormatter();

            obj = (TestSimpleObject)formatter.Deserialize(stream);
            stream.Close();

            Console.WriteLine("");
            Console.WriteLine("After deserialization the object contains: ");
            obj.Print();
        }


        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                try
                {
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }
                catch (ObjectDisposedException e)
                {

                }

                return ms.ToArray();
            }
        }


        #endregion
    }
}
