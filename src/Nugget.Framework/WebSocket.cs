using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget
{
    public abstract class WebSocket<T> : ASendingWebSocket, IWebSocket, IReceivingWebSocket<T>
    {
        public abstract void Incoming(T data);
        public abstract void Disconnected();
        public abstract void Connected(ClientHandshake handshake);

    }

    public abstract class WebSocket : WebSocket<string> { } // default to string
}
