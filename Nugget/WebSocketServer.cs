using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Nugget
{
    public enum ServerLogLevel { Nothing, Subtle, Verbose };
    public delegate void ClientConnectedEventHandler(WebSocketConnection sender, EventArgs e);

    public class WebSocketServer
    {
        private WebSocketFactory SocketFactory = new WebSocketFactory();
        List<WebSocket> Sockets = new List<WebSocket>();
        public Socket ListenerSocker { get; private set; }
        public string LocationRoot { get; private set; }
        public string LocationFull { get; private set; }
        public int Port { get; private set; }

        public string Origin { get; private set; }


        public WebSocketServer(int port, string origin, string location, string path)
        {
            Port = port;
            Origin = origin;
            LocationFull = location +'/'+ path;
            LocationRoot = path;
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
            var path = ShakeHands(clientSocket);
            Sockets.Add(SocketFactory.Create(path, clientSocket));
            ListenForClients();
        }

        private string ShakeHands(Socket conn)
        {
            string GETPath = null;
            using (var stream = new NetworkStream(conn))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                var pathStripper = new Regex(@"ws:\/\/(\w+:[0-9]+).*"); // cut the path just after the port number
                var strippedLocation = "";
                if(pathStripper.IsMatch(LocationFull))
                {
                    strippedLocation = pathStripper.Replace(LocationFull, "$1");
                }

                string[] protocolPatterns = {
                                                @"GET\s\/" + LocationRoot + @"(.*)\sHTTP\/1\.1", // GET <path> HTTP/1.1
                                                "Upgrade: WebSocket",
                                                "Connection: Upgrade",
                                                "Host: "+ strippedLocation,
                                                "Origin: "+Origin,
                                            };
                

                for (int i = 0; i < 5; i++) // five lines of handshake
                {
                    var regex = new Regex(protocolPatterns[i]);
                    var prot = reader.ReadLine();
                    if (regex.IsMatch(prot))
                    {
                        if (i == 0)
                        {
                            GETPath = regex.Replace(prot, "$1"); // get the path the web socket is requesting
                        }
                    }
                    else
                    {
                        throw new Exception("Client-part of the handshake doesn't match");
                    }
                }

                // send handshake to the client
                writer.WriteLine("HTTP/1.1 101 Web Socket Protocol Handshake");
                writer.WriteLine("Upgrade: WebSocket");
                writer.WriteLine("Connection: Upgrade");
                writer.WriteLine("WebSocket-Origin: " + Origin);
                writer.WriteLine("WebSocket-Location: " + LocationFull+'/'+GETPath);
                writer.WriteLine("");
            }

            return GETPath;
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
