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

namespace NLog 
{ 
    public class Networking
    {
        #region Member variables

        // create new client istance
        private static Socket client;

        // create new instance of NLog logger class
        private Logging logging = new Logging();

        #endregion

        #region TEST SOCKET REGION

        /// <summary>
        /// Method for creating listeners for incoming connection requests and sending messages to clients
        /// </summary>
        [STAThread]
        public void StartServer(string address, int port)
        {
            try
            {
                // Set the title displayed in the console title bar
                Console.Title = "Server";

                TcpListener tcpListener = setClientListener(address, port);
                // Create log messages by using NLog logging platform
                logging.LogMessage(Logging.Logs.Info, "Server is running");
                Console.WriteLine("\nServer is running");

                try
                {
                    while (true)
                    {
                        // accept a pending connection request
                        client = tcpListener.AcceptSocket();

                        // if socket is connected to a remote host, send server message
                        if (client.Connected)
                        {
                            // get client remote endpoint
                            string remoteEndPoint = client.RemoteEndPoint.ToString();
                            logging.LogMessage(Logging.Logs.Info, "Client connected", remoteEndPoint);
                            Console.WriteLine("Client connected - " + remoteEndPoint);

                            sendWelcomeMessage(remoteEndPoint);

                            // continue conversation on a new thread
                            Thread thread = new Thread(new ParameterizedThreadStart(listenClient));
                            thread.Start(client);
                        }
                    }
                }
                catch (Exception e)
                {
                    logging.LogMessage(Logging.Logs.Error, "Server error: ", e.ToString());
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                logging.LogMessage(Logging.Logs.Debug, "Server is already running");
                //Console.WriteLine("Server is already running! {0}", e);
            }
        }

        private void sendWelcomeMessage(string remoteEndPoint)
        {
            // send message to client after connection is established
            byte[] toBytes = Encoding.ASCII.GetBytes("Wellcome!");
            // send data to a connected Socket
            client.Send(toBytes);
            logging.LogMessage(Logging.Logs.Info, "Server message sent to", remoteEndPoint);
            Console.WriteLine("Server message sent to {0}", remoteEndPoint);
        }

        private static TcpListener setClientListener(string address, int port)
        {
            // Set listener for incoming connection requests
            IPAddress ipAddress = IPAddress.Parse(address);
            TcpListener tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();
            return tcpListener;
        }

        /// <summary>
        /// Method for communicating with the client by writing data to the Network Stream on a new thread
        /// </summary>
        /// <param name="data">object data</param>
        public void listenClient(object data)
        {
            while (client.Connected)
            {
                try
                {
                    NetworkStream netstream = createNetworkStream(data);
                    byte[] buffer = sendTcpMessage(netstream);
                    buffer = readNewTcpMessage(netstream, buffer);

                    //  sequence of bytes
                    Stream stream = convertClientMessage(buffer);
                    deserializeXml(stream);
                }
                catch (IOException e)
                {
                    logging.LogMessage(Logging.Logs.Error, "IOException: ", e.ToString());
                    Console.WriteLine("An existing connection was forcibly closed", e.ToString());
                }
                catch (Exception e)
                {
                    logging.LogMessage(Logging.Logs.Error, "Unable to communicate with the client on a new thread", e.ToString());
                    Console.WriteLine("Unable to communicate with the client on a new thread", e.ToString());
                }
            }
        }

        private static void deserializeXml(Stream stream)
        {
            // for deserializing XML documents into objects of the specified type
            XmlSerializer serializer = new XmlSerializer(typeof(Serialization.StepList));

            // deserialize the XML document contained by the specified memory stream
            Serialization.StepList result = (Serialization.StepList)serializer.Deserialize(stream);
            result.Print();
            Console.WriteLine("\n");
        }

        private static Stream convertClientMessage(byte[] buffer)
        {
            Stream stream = new MemoryStream(buffer);
            // convert the client message into a string and display it
            string readData = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("Client {0} sent message: {1}", client.RemoteEndPoint, readData);
            return stream;
        }

        private static byte[] readNewTcpMessage(NetworkStream netstream, byte[] buffer)
        {
            // read the client message into a byte buffer
            buffer = new byte[1024];
            netstream.Read(buffer, 0, 1024);
            return buffer;
        }

        private static byte[] sendTcpMessage(NetworkStream netstream)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(" Are you receiving this message?");
            // send message to client by writing data to the NetworkStream
            netstream.Write(buffer, 0, buffer.Length);
            return buffer;
        }

        private static NetworkStream createNetworkStream(object data)
        {
            client = (Socket)data;

            // create new netstream
            NetworkStream netstream = new NetworkStream(client);

            // create stream for writing characters in a particular encoding
            StreamWriter streamWriter = new StreamWriter(netstream);
            return netstream;
        }

        /// <summary>
        /// Method for instanting a client connections, sending messages to server, reading messages from server and closing TCP connection
        /// </summary>
        [STAThread]
        public void StartClient(string ipAddress, int port)
        {
            // set the title to display in the console title bar
            string remoteEndpoint;
            TcpClient tcpClient;
            setClientEndpoint(out remoteEndpoint, out tcpClient);

            remoteEndpoint = newTcpConnection(ipAddress, port, remoteEndpoint, tcpClient);

            try
            {
                NetworkStream networkStream;
                string data;
                readServerMessage(tcpClient, out networkStream, out data);

                // send response to server
                if (data.Length != 0)
                {
                    logging.LogMessage(Logging.Logs.Info, "Response was sent by client", remoteEndpoint);
                    Console.WriteLine("Response was sent by client {0}", remoteEndpoint);

                    List<Serialization.CollectionTestObject> entries = serializeTestObjects();

                    XDocument xdoc = createXmlTree(entries);

                    sendNetworkStream(networkStream, xdoc);

                    logging.LogMessage(Logging.Logs.Info, "Collection was sent by client", remoteEndpoint);
                    Console.WriteLine("Collection was sent by client {0}", remoteEndpoint);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Unable to read server message {0}", e.ToString());
                logging.LogMessage(Logging.Logs.Error, "Unable to read server message", e.ToString());
            }

            try
            {
                testDelayAndDisconnect(tcpClient);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to handle delay and disconnect", remoteEndpoint);
                logging.LogMessage(Logging.Logs.Error, "Unable to handle delay and disconnect", e.ToString());
            }

            checkClientConnection(remoteEndpoint, tcpClient);

        }

        private static void testDelayAndDisconnect(TcpClient tcpClient)
        {
            // wait for 5 seconds and then disconnect, 1000 milliseconds is one second.
            Thread.Sleep(5000);
            // close the current stream and release any resources associated with the current stream
            tcpClient.GetStream().Close();
            // dispose client instance and request closing of underlying TCP connection
            tcpClient.Close();
        }

        private void checkClientConnection(string remoteEndpoint, TcpClient tcpClient)
        {
            // check if client is disconnected
            if (!tcpClient.Connected)
            {
                Console.WriteLine("Disconnected {0}", remoteEndpoint);
                logging.LogMessage(Logging.Logs.Info, "Client disconnected", remoteEndpoint);
            }
            else
            {
                Console.WriteLine("The underlying TCP connection was not closed by client {0}", remoteEndpoint);
                logging.LogMessage(Logging.Logs.Warn, "The underlying TCP connection was not closed by client {0}", remoteEndpoint);
            }
        }

        private static List<Serialization.CollectionTestObject> serializeTestObjects()
        {
            Serialization.CollectionTestObject c = new Serialization.CollectionTestObject();

            // test instances that will be sent through Socket
            Serialization.CollectionTestObject e1 = new Serialization.CollectionTestObject(1, "one");
            Serialization.CollectionTestObject e2 = new Serialization.CollectionTestObject(2, "two");
            Serialization.CollectionTestObject e3 = new Serialization.CollectionTestObject(3, "three");
            Serialization.CollectionTestObject e4 = new Serialization.CollectionTestObject(4, "four");

            // collection of test objects for serialization
            List<Serialization.CollectionTestObject> entries = new List<Serialization.CollectionTestObject>();
            entries.Add(e1);
            entries.Add(e2);
            entries.Add(e3);
            entries.Add(e4);
            entries.Add(e1);
            entries.Add(e2);
            entries.Add(e3);
            entries.Add(e4);
            return entries;
        }

        private static void readServerMessage(TcpClient tcpClient, out NetworkStream networkStream, out string data)
        {
            // Get the stream used to read the message sent by the server.
            networkStream = tcpClient.GetStream();
            // Read the server message into a byte buffer.
            byte[] bytes = new byte[1024];
            networkStream.Read(bytes, 0, 1024);
            //Convert the server's message into a string and display it.
            data = Encoding.UTF8.GetString(bytes);
            Console.WriteLine("Server sent message: {0}", data);
        }

        private string newTcpConnection(string ipAddress, int port, string remoteEndpoint, TcpClient tcpClient)
        {
            try
            {
                // connect the client to the specified port on the specifed host
                tcpClient.Connect(ipAddress, port);
                // get socket remote endpoint
                remoteEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                Console.WriteLine("Connected - {0}", remoteEndpoint);
                logging.LogMessage(Logging.Logs.Info, "Connected ", remoteEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect the client", e.ToString());
                logging.LogMessage(Logging.Logs.Error, "Unable to connect the client", e.ToString());
            }
            return remoteEndpoint;
        }

        private static void setClientEndpoint(out string remoteEndpoint, out TcpClient tcpClient)
        {
            Console.Title = "Client";
            remoteEndpoint = "";
            tcpClient = new TcpClient();
            Console.WriteLine("Connecting...");
        }

        private static XDocument createXmlTree(List<Serialization.CollectionTestObject> entries)
        {
            // LINQ to XML for creating XML tree
            XDocument xdoc = new XDocument(
                new XElement("StepList",
                entries.Select(i => new XElement("Step",
                new XElement("Name", (i.Value)),
                new XElement("Desc", (i.Key))))));
            return xdoc;
        }

        private static void sendNetworkStream(NetworkStream networkStream, XDocument xdoc)
        {
            // encode all the characters in the specifed string into a sequence of bytes
            byte[] buffer = Encoding.ASCII.GetBytes(xdoc.ToString());
            // write data to Network Stream in order to send return message to server
            networkStream.Write(buffer, 0, buffer.Length);
        }

        #endregion
    }
}
