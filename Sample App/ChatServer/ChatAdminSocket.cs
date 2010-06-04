using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    class ChatAdminSocket : ChatClientSocket
    {
        public override void Incomming(string data)
        {
            if (data.Contains("/kick"))
            {

            }

            base.Incomming(data);
        }

        public override void Connected()
        {
            Console.WriteLine("new admin conencted");
        }
    }
}
