using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace ConsoleApp
{

    class Server
    {
        static void Main(string[] args)
        {
            // create the server
            var nugget = new WebSocketServer("ws://localhost:8181", "null");

            nugget.Start();
            
            nugget.OnConnect += (wsc) =>
            {
                wsc.Send("Hello World");
                Console.WriteLine("new connection");
            };
            

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
