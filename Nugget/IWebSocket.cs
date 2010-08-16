using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget
{
    public interface IWebSocket<in T>
    {
        void Incomming(T data);
        void Disconnected();
        void Connected(ClientHandshake handshake);
    }

}
