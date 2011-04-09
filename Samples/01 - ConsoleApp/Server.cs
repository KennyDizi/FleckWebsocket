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
            // the parameters describe where to listen for connections (the port) and which connections to accept (the origin and location)
            // it is important that these are correct, or the server might not accept the incoming connections
            // see http://tools.ietf.org/html/draft-hixie-thewebsocketprotocol, to learn more about these parameters
            var nugget = new WebSocketServer("ws://localhost:8181", "null");

            nugget.Start();
            
            nugget.OnConnect += (wsc) =>
            {
                wsc.Send("Welcome! you are now connected to the web socket server.");
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
