using System; // fundamental classes and commonly-used base classes 
using System.Collections.Generic; // provides strongly typed collections
using System.Linq; // provides patterns for querying and updating data
using System.Text; // string formating, converting characters from/to bytes
using System.Threading.Tasks; // provides types for asynchronous operations
using System.Net.Sockets; // provides client connections for TCP network services
using NLog; // provides logging platform with rich log routing and management capabilities
using System.Net; // simple programming interface for network protocols
using System.Threading; // creates and controls a thread, sets priority and gets its status
using System.IO; // reading and writing to files and data streams, basic file support
using System.Runtime.Serialization.Formatters.Soap; // (de)serializing objects in the SOAP format
using System.Xml.Linq; // in-memory XML programming interface for modifying XML documents
using System.Xml.Serialization; // serialization of  objects into XML format documents or streams
using System.Configuration; // get the AppSettings data for the current app's default configuration

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

        // stores list of serializable test objects
        private Dictionary<int, TestSimpleObject> m_listConversations = new Dictionary<int, TestSimpleObject>();

        // lock
        private static readonly Object obj = new Object();

        #endregion

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
        /// <param name="logs">log level</param>
        /// <param name="message">custom text message</param>
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
        /// <param name="logs">log level</param>
        /// <param name="message">custom text message</param>
        /// <param name="param">extra parameter</param>
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
        /// <param name="server">server address</param>
        /// <param name="message">client message</param>
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
                responseData = Encoding.ASCII.GetString(data, 0, bytes);
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
        /// <param name="client">instance of TcpClient</param>
        public static void DisconnectClient(TcpClient client)
        {
            client.GetStream().Close();
            client.Close();
        }

        /// <summary>
        /// Method for listening for incoming connection attempts on the specified local IP address and port number
        /// </summary>
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
                IPAddress ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings["IpAddress"]);
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
        /// <param name="data">object data</param>
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

                    //  sequence of bytes
                    Stream stream = new MemoryStream(buffer);

                    // convert the client message into a string and display it
                    string readData = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("Client {0} sent message: {1}", client.RemoteEndPoint, readData);

                    // for deserializing XML documents into objects of the specified type
                    XmlSerializer serializer = new XmlSerializer(typeof(StepList));

                    // deserialize the XML document contained by the specified memory stream
                    StepList result = (StepList)serializer.Deserialize(stream);
                    result.Print();
                    Console.WriteLine("\n");

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
                tcpClient.Connect(ConfigurationManager.AppSettings["IpAddress"],
                    int.Parse(ConfigurationManager.AppSettings["SocketPort"]));
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

                // send response to server
                if (data.Length != 0)
                {
                    //// encode all the characters in the specified string into a sequence of bytes
                    //byte[] buffer = Encoding.ASCII.GetBytes("\nI got your message, thank you!");
                    //// write data to Network Stream in order to send return message to server
                    //networkStream.Write(buffer, 0, buffer.Length);

                    LogMessage(Logs.Info, "Response was sent by client", remoteEndpoint);
                    Console.WriteLine("Response was sent by client {0}", remoteEndpoint);

                    // test instances that will be sent through Socket
                    CollectionTestObject e1 = new CollectionTestObject(1, "one");
                    CollectionTestObject e2 = new CollectionTestObject(2, "two");
                    CollectionTestObject e3 = new CollectionTestObject(3, "three");
                    CollectionTestObject e4 = new CollectionTestObject(4, "four");

                    // collection of test objects for serialization
                    List<CollectionTestObject> entries = new List<CollectionTestObject>();
                    entries.Add(e1);
                    entries.Add(e2);
                    entries.Add(e3);
                    entries.Add(e4);
                    entries.Add(e1);
                    entries.Add(e2);
                    entries.Add(e3);
                    entries.Add(e4);

                    // LINQ to XML for creating XML tree
                    XDocument xdoc = new XDocument(
                        new XElement("StepList",
                        entries.Select(i => new XElement("Step",
                        new XElement("Name", (i.Value)),
                        new XElement("Desc", (i.Key))))));

                    //Console.Write("\n\n" + xdoc);

                    // encode all the characters in the specifedstring into a sequence of bytes
                    byte[] buffer = Encoding.ASCII.GetBytes(xdoc.ToString());
                    // write data to Network Stream in order to send return message to server
                    networkStream.Write(buffer, 0, buffer.Length);

                    LogMessage(Logs.Info, "Collection was sent by client", remoteEndpoint);
                    Console.WriteLine("Collection was sent by client {0}", remoteEndpoint);
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
                Console.WriteLine("Unable to handle delay and disconnect", remoteEndpoint);
                LogMessage(Logs.Error, "Unable to handle delay and disconnect", e.ToString());
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

        // A test objects serialized by using SOAP format
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

            /// <summary>
            /// Method for printing all elements of TestSimpleObject instance
            /// </summary>
            public void Print()
            {
                Console.WriteLine("member1 = '{0}'", member1);
                Console.WriteLine("member2 = '{0}'", member2);
                Console.WriteLine("member3 = '{0}'", member3);
                Console.WriteLine("member4 = '{0}'", member4);
                Console.WriteLine("member5 = '{0}'", member5);
            }
        }

        /// <summary>
        /// Method for creating serialized collection by using SOAP format
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Method for deserializing collections in SOAP format
        /// </summary>
        /// <param name="stream">serialized object in binary format that we want to deserialize</param>
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

        /// <summary>
        /// Method for converting stream to byte array
        /// </summary>
        /// <param name="input">input stream that we want to convert</param>
        /// <returns></returns>
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
                    Console.WriteLine("Conversion failed" + e.Message);
                }

                return ms.ToArray();
            }
        }


        #endregion

        #region XML deserialization

        /// <summary>
        /// Sample collection of objects deserialized by 
        /// </summary>
        public class CollectionTestObject
        {
            public object Key;
            public object Value;

            /// <summary>
            /// Default constructor for CollectionTestObject
            /// </summary>
            public CollectionTestObject()
            {
            }

            /// <summary>
            /// Constructor for CollectionTestObject
            /// </summary>
            /// <param name="key">sample key</param>
            /// <param name="value">sample value</param>
            public CollectionTestObject(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }

        [XmlRoot("StepList")]
        public class StepList
        {
            [XmlElement("Step")]
            public List<Step> Steps { get; set; }

            /// <summary>
            /// Method for printing out all objects from collection
            /// </summary>
            public void Print()
            {
                int count = 1;
                Steps.ForEach(
                    item => Console.Write(
                        count++ +
                        ". Value: " +
                        item.Name +
                        ", Key: " +
                        item.Desc +
                        "\n"
                    )
                );
            }
        }

        public class Step
        {
            [XmlElement("Name")]
            public string Name { get; set; }
            [XmlElement("Desc")]
            public string Desc { get; set; }
        }

        #endregion
    }
}
