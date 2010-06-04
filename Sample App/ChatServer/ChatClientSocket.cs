using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class ChatClientSocket : WebSocket
    {

        private string name = "john doe";
        public static int num = 0;
        private int myNum;

        public ChatClientSocket()
        {
            num++;
            myNum = num;
        }

        public override void Incomming(string data)
        {
            if (data.StartsWith("/nick "))
            {
                name = data.Replace("/nick ", "");
            }

            Console.WriteLine("chat client "+myNum+" got: " + data);
            Send("blag");
        }

        public override void Connected()
        {
            Console.WriteLine("new client conencted: " + myNum);
        }

        public override void Disconnected()
        {
            Console.WriteLine("client "+myNum+" has disconnected");
        }
    }
}
