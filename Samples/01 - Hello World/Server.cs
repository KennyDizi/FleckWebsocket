using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Nugget.Samples.HelloWorld
{
    public class Server
    {
        static void Main(string[] args)
        {
            // create the server
            var nugget = new WebSocketServer("ws://localhost:8181", "null");

            // subscribe to the OnConnect event
            nugget.OnConnect += (sender, e) =>
            {
                var wsc = (WebSocketConnection)e.Connection;
                wsc.Send("Hello World");
                Console.WriteLine("new connection from {0}", wsc.Socket.RemoteEndPoint);
            };

            // start the server
            nugget.Start();

            // keep alive loop
            var input = "";
            while (input != "exit")
            {
                input = Console.ReadLine();
                nugget.SendToAll(input);
            }
        }
    }
}
