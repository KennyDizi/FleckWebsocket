using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Chat
{
    class Server
    {
        static void Main(string[] args)
        {
            // create the server
            var nugget = new WebSocketServer("ws://localhost:8181", "null");
            
            nugget.OnConnect += (wsc) =>
            {
                wsc.Send("[server] Welcome to the chat");
                nugget.SendToAll(String.Format("[server] {0} connected", wsc.Socket.RemoteEndPoint), wsc);
                Console.WriteLine("new connection from {0}", wsc.Socket.RemoteEndPoint);
                
                wsc.OnReceive += (sender, data) =>
                {
                    nugget.SendToAll(String.Format("[{0}] {1}",sender.Socket.RemoteEndPoint,data));
                };

                wsc.OnDisconnect += (connection) =>
                {
                    nugget.SendToAll(String.Format("[server] {0} disconnected", connection.Socket.RemoteEndPoint), connection);
                };

            };

            // start the server
            nugget.Start();

            // keep alive loop
            var input = "";
            while (input != "exit")
            {
                input = Console.ReadLine();
                nugget.SendToAll("[server] "+input);
            }
        }

    }
}
