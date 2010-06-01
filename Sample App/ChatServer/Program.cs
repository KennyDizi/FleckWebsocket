using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketServer;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sample web socket chat app:");
            Console.WriteLine("Navigate to http://localhost:8080/ to start chatting...\n");
            var cs = new ChatServer();
        }
    }
}
