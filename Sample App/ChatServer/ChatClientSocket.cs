using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class ChatClientSocket : WebSocket
    {
        public override void Incomming(string data)
        {
            Console.WriteLine("chat controller got: " + data);
        }

        public override void Connected()
        {
            Console.WriteLine("new client conencted");
        }
    }
}
