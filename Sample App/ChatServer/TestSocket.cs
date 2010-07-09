using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    class TestSocket : WebSocket
    {
        public override void Incomming(string data)
        {
            Send(data);
        }

        public override void Disconnected()
        {
            //
        }

        public override void Connected(ClientHandshake handshake)
        {
            //
        }
    }
}
