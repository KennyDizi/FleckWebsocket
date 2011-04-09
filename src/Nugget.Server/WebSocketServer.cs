using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace Nugget.Server
{
    /// <summary>
    /// Called when a new client is connected
    /// </summary>
    /// <param name="wsc">the connectino representing the connection</param>
    public delegate void ConnectedEventHandler(WebSocketConnection wsc);

    public class WebSocketServer : IDisposable
    {
        /// <summary>
        /// The socket that listens for conenctions
        /// </summary>
        public Socket ListenerSocket { get; private set; }
        public string Location { get; private set; }
        public int Port { get; private set; }
        public string Origin { get; private set; }

        /// <summary>
        /// A list of the currently connected clients
        /// </summary>
        public List<WebSocketConnection> ConnectedClients { get; set; }

        /// <summary>
        /// Event that is fired when a new client connects
        /// </summary>
        public event ConnectedEventHandler OnConnect;

        /// <summary>
        /// Instantiate a new web socket server
        /// </summary>
        /// <param name="location">the address and port of this web socket server (e.g. ws://localhost:8181)</param>
        /// <param name="origin">the address from where connections are allowed to come (e.g. http://localhost)</param>
        public WebSocketServer(string location, string origin)
        {
            Origin = origin;
            Location = location;
            Port = new Uri(location).Port;
            ConnectedClients = new List<WebSocketConnection>();
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            // create the main server socket, bind it to the local ip address and start listening for clients
            ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Port);
            ListenerSocket.Bind(ipLocal);
            ListenerSocket.Listen(100);
            Log.Info("Server stated on " + ListenerSocket.LocalEndPoint);
            ListenForClients();
        }

        private void ListenForClients()
        {
            ListenerSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        // a new client is trying to connect
        private void OnClientConnect(IAsyncResult ar)
        {
            Socket clientSocket = null;
            
            try
            {
                clientSocket = ListenerSocket.EndAccept(ar);
            }
            catch
            {
                Log.Error("Listener socket is closed");
                return;
            }
            
            var shaker = new HandshakeHandler(Origin, Location);
            // shake hands - and provide a callback for when hands has been shaken
            shaker.Shake(clientSocket, (handshake) =>
            {
                // instantiate the connection and subscribe to the events
                var wsc = new WebSocketConnection(clientSocket, handshake);
                wsc.OnDisconnect += new DisconnectedEventHandler(OnClientDisconnect);
                wsc.OnReceive += new ReceiveEventHandler(OnClientData);
                
                // add the new client to the list
                ConnectedClients.Add(wsc);
                
                // fire the connected event
                if (OnConnect != null)
                {
                    OnConnect(wsc);
                }

                // start looking for data
                wsc.StartReceiving();
            });
            
            // listen some more
            ListenForClients();
        }

        private void OnClientDisconnect(WebSocketConnection wsc)
        {
            Log.Info("client disconnected");
            ConnectedClients.Remove(wsc);
        }

        private void OnClientData(WebSocketConnection wsc, string data)
        {
            Log.Info("incomming data: " + data);
        }

        public void Dispose()
        {
            ListenerSocket.Dispose();
        }

        /// <summary>
        /// Send a message to all the connected clients
        /// </summary>
        /// <param name="msg">the message to send</param>
        public void SendToAll(string msg)
        {
            foreach (var client in ConnectedClients)
            {
                client.Send(msg);
            }
        }

        /// <summary>
        /// Send a message to all the connected clients, excluding one.
        /// </summary>
        /// <param name="msg">the message to send</param>
        /// <param name="exclude">the connection to exclude</param>
        public void SendToAll(string msg, WebSocketConnection exclude)
        {
            foreach (var client in ConnectedClients.Where(x => !x.Equals(exclude)))
            {
                client.Send(msg);
            }
        }
    }

}
