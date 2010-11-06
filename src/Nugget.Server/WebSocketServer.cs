using System;
using System.Net.Sockets;
using System.Net;

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

        public event ConnectedEventHandler OnConnect;

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
                
                // fire the connected event
                OnConnect(wsc);

                // start looking for data
                wsc.StartReceiving();
            });
            
            // listen some more
            ListenForClients();
        }

        void OnClientDisconnect(WebSocketConnection wsc)
        {
            Log.Info("client disconnected");
            wsc.Socket.Dispose();
        }

        private void OnClientData(WebSocketConnection wsc, string data)
        {
            Log.Info("incomming data: " + data);
        }

        public void Dispose()
        {
            ListenerSocket.Dispose();
        }
    }

}
