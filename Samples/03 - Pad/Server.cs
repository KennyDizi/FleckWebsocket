using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace Pad
{
    class PadSocket : WebSocket
    {
        static List<PadSocket> sockets = new List<PadSocket>();

        public override void Incomming(string data)
        {
            foreach (var item in sockets) // not much fun here, we are just ecco'ing the json string to the other sockets
            {
                if (item != this)
                    item.Send(data);
            }
        }

        public override void Disconnected()
        {
        }

        public override void Connected(ClientHandshake handshake)
        {
            sockets.Add(this);
        }
    }


    class Server
    {
        static void Main(string[] args)
        {
            var nugget = new WebSocketServer(8181, "null", "ws://localhost:8181");
            nugget.RegisterHandler<PadSocket>("/padsample");
            nugget.Start();
            Console.WriteLine("Server started, open client.html in a websocket-enabled browser");

            var input = Console.ReadLine();
            while (input != "exit")
            {
                input = Console.ReadLine();
            }
            nugget.Stop();
        }
    }
}
