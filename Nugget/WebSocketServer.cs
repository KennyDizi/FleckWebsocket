using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Nugget
{
    public class WebSocketServer
    {
        private WebSocketFactory SocketFactory = new WebSocketFactory();
        private HandshakeHandler Shaker = new HandshakeHandler();
        public Socket ListenerSocker { get; private set; }
        public string Location { get; private set; }
        public int Port { get; private set; }
        public string Origin { get; private set; }

        /// <summary>
        /// Instantiate a new web socket server
        /// </summary>
        /// <param name="port">the port to run on/listen to</param>
        /// <param name="origin">the url where connections are allowed to come from (e.g. http://localhost)</param>
        /// <param name="location">the url of this web socket server (e.g. ws://localhost:8181)</param>
        public WebSocketServer(int port, string origin, string location)
        {
            Port = port;
            Origin = origin;
            Location = location;
        }

        /// <summary>
        /// Register a class to handle a connection comming from the web sockets
        /// </summary>
        /// <typeparam name="TSocket">the class to handle the connection, a new object of this class is instantiated for every new connection</typeparam>
        /// <param name="path">the path the class should respond to</param>
        public void RegisterHandler<TSocket>(string path) where TSocket : WebSocket
        {
            SocketFactory.Register<TSocket>(path);
        }

        public void Start()
        {
            // create the main server socket, bind it to the local ip address and start listening for clients
            ListenerSocker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Port);
            ListenerSocker.Bind(ipLocal);
            ListenerSocker.Listen(100);
            Log.Info("Server stated on " + ListenerSocker.LocalEndPoint);
            ListenForClients();
        }

        private void ListenForClients()
        {
            ListenerSocker.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        // a new client is trying to connect
        private void OnClientConnect(IAsyncResult asyn)
        {
            
            // get the socket
            var clientSocket = ListenerSocker.EndAccept(asyn);
            Log.Info("new connection from " + clientSocket.RemoteEndPoint);
            
            WebSocket webSocket;
            try
            {
                // greet the newcommer with a (friendly) handshake
                var shake = Shaker.Shake(clientSocket, Origin, Location);
                // create the object to handle the new connection
                webSocket = SocketFactory.Create(shake.Fields["path"].Value);
                // tell the newcommer about the context
                webSocket.Socket = clientSocket;
                webSocket.Protocol = shake.Protocol;

                // workaround to counter a bug that causes the first message sent to the socket to be lost
                byte[] dummy = new byte[2] { 0, 255 };
                clientSocket.Send(dummy);

                // tell the client that it is succesfully connected
                webSocket.Connected();
                
            }
            catch (Exception e)
            {
                // log the failed connection attempt
                Log.Error("Exception thrown from method OnClientConnect:\n" + e.Message);
                clientSocket.Close();
            }
            
            // listen some more
            ListenForClients();
        }
    }

}
