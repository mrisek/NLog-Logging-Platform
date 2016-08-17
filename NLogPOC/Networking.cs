using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using static NLogPoC.Logging;
using static NLogPoC.Serialization;

namespace NLogPoC
{
    public class Networking
    {
        #region Member variables

        // create new client istance
        private static Socket client;

        // create new netstream
        private static NetworkStream netstream;

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
        public static void StartServer()
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
        public static void listenClient(object data)
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
        public static void StartClient()
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

                    Serialization.CollectionTestObject c = new Serialization.CollectionTestObject();

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
    }
}
