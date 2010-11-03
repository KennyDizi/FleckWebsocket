using System;
using System.Net.Sockets;
using System.Net;

namespace Nugget.Server
{
    public delegate void ConnectedEventHandler(WebSocketConnection wsc);
    public delegate void ReceiveEventHandler(WebSocketConnection wsc, string data);
    public delegate void DisconnectedEventHandler(WebSocketConnection wsc);

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
        public event ReceiveEventHandler OnReceive;
        public event DisconnectedEventHandler OnDisconnect;

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
            shaker.OnSuccess = (handshake) =>
            {
                var wsc = new WebSocketConnection(clientSocket, handshake, OnClientData, OnClientDisconnect);
                OnConnect(wsc);
                wsc.StartReceiving();
            };

            shaker.Shake(clientSocket);
            
            // listen some more
            ListenForClients();
        }

        private void OnClientData(WebSocketConnection wsc, string data)
        {
            Log.Info("incomming data: " + data);
            OnReceive(wsc, data);
        }

        private void OnClientDisconnect(WebSocketConnection wsc)
        {
            Log.Info("client disconnected");
            OnDisconnect(wsc);
            wsc.Socket.Dispose();
        }

        public void Dispose()
        {
            ListenerSocket.Dispose();
        }
    }

}
