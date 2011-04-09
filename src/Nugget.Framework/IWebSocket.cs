using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Nugget
{
    public interface IWebSocket
    {
        void Disconnected();
        void Connected(ClientHandshake handshake);
    }
}
