using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Nugget.Framework
{
    public interface IWebSocket
    {
        void Connected(ClientHandshake handshake);
        void Disconnected();
        void Incoming(string data);
    }
}
