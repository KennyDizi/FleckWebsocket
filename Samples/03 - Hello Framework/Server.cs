using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Framework;
using Nugget.Server;

namespace Nugget.Samples.HelloFramework
{

    class EchoSocket : WebSocket
    {

        public override void Incoming(string data)
        {
            Send(data); // echo the received message
        }

        public override void Disconnected()
        {
            Console.WriteLine("echo client disconnected");
        }

        public override void Connected(ClientHandshake handshake)
        {
            Console.WriteLine("new echo connection");
        }
    }

    class DoubleEchoSocket : WebSocket
    {

        public override void Incoming(string data)
        {
            Send(data + data); // echo the received message twice
        }

        public override void Disconnected()
        {
            Console.WriteLine("double echo client disconnected");
        }

        public override void Connected(ClientHandshake handshake)
        {
            Console.WriteLine("new double echo connection");
        }
    }


    class Server
    {
        static void Main(string[] args)
        {
            var nugget = new WebSocketServer("ws://localhost:8181", "null");
            var wsf = new WebSocketFactory(nugget);
            wsf.Register<EchoSocket>("/echo");
            wsf.Register<DoubleEchoSocket>("/echo2");

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
