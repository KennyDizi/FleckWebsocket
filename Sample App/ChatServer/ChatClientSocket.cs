using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class ChatClientSocket : WebSocket
    {
        public static int num = 0;
        private int myNum;

        public ChatClientSocket()
        {
            num++;
            myNum = num;
        }

        public override void Incomming(string data)
        {
            Console.WriteLine("chat client "+myNum+" got: " + data);
        }

        public override void Connected()
        {
            Console.WriteLine("new client conencted: " + myNum);
        }
    }
}
