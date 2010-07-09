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
        private void OnClientConnect(IAsyncResult ar)
        {

            var clientSocket = ListenerSocker.EndAccept(ar);
            var shaker = new HandshakeHandler(Origin, Location);
            shaker.OnSuccess = (handshake) =>
            {
                // create the web socket object based on the path requested
                var ws = SocketFactory.Create(handshake.ResourcePath);
                ws.Socket = clientSocket;

                // let the web socket know that it is connected
                ws.Connected(handshake);

                // start receiving data
                ws.Receive();
            };

            shaker.Shake(clientSocket);
            
            // listen some more
            ListenForClients();
        }
    }

}
