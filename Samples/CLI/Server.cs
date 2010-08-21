using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace CLI
{
    class CLISocket : WebSocket
    {
        public static List<CLISocket> Connections = new List<CLISocket>();

        public override void Incomming(string data)
        {
            Console.WriteLine(data);
        }

        public override void Disconnected()
        {
            Console.WriteLine("--- disconnected ---");
            Connections.Remove(this);
        }

        public override void Connected(ClientHandshake handshake)
        {
            Console.WriteLine("--- connected --- ");
            Connections.Add(this);
        }
    }
    
    class Server
    {
        static void Main(string[] args)
        {
            var nugget = new WebSocketServer(8181, "null", "ws://localhost:8181");
            nugget.RegisterHandler<CLISocket>("/clisample");
            Nugget.Log.Level = LogLevel.None;
            nugget.Start();
            
            Console.WriteLine("Server started, open client.html in a websocket-enabled browser");


            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var item in CLISocket.Connections)
                    item.Send(input);
                
                input = Console.ReadLine();
            }

        }
    }
}
