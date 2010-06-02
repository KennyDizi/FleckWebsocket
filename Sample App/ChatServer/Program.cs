using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sample web socket chat app:");
            Console.WriteLine("Open Chrome at http://localhost:8080/ to start chatting...\n");
            var cs = new ChatServer();
        }
    }
}
