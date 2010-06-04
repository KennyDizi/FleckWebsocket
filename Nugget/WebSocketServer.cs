using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Nugget
{
    public enum ServerLogLevel { Nothing, Subtle, Verbose };

    public class WebSocketServer
    {
        private WebSocketFactory SocketFactory = new WebSocketFactory();
        private HandshakeHandler Shaker = new HandshakeHandler();
        List<WebSocket> Sockets = new List<WebSocket>();
        public Socket ListenerSocker { get; private set; }
        
        public string Location { get; private set; }
        public int Port { get; private set; }

        public string Origin { get; private set; }


        public WebSocketServer(int port, string origin, string location)
        {
            Port = port;
            Origin = origin;
            Location = location;
        }

        public void RegisterSocket<TSocket>(string path) where TSocket : WebSocket
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
            ListenForClients();
        }

        private void ListenForClients()
        {
            ListenerSocker.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        private void OnClientConnect(IAsyncResult asyn)
        {
            var clientSocket = ListenerSocker.EndAccept(asyn);
            var shake = ShakeHands(clientSocket);

            var webSocket = SocketFactory.Create(shake.Fields["path"].Value);
            Sockets.Add(webSocket);
            webSocket.Socket = clientSocket;
            webSocket.Connected();
            webSocket.Protocol = shake.Protocol;
            
            ListenForClients();
        }

        private Handshake ShakeHands(Socket conn)
        {
            return Shaker.Shake(conn, Origin, Location);
        }

        public void SendToAll(string data)
        {
            foreach (var item in Sockets)
            {
                item.Send(data);
            }
        }
    }

}
