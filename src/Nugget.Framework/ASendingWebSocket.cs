using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Nugget.Framework
{
    public abstract class ASendingWebSocket
    {
        public WebSocketConnection Connection { get; internal set; }
        /// <summary>
        /// Send data to the client socket
        /// </summary>
        /// <param name="data">the data to send</param>
        public void Send(string data)
        {
            Connection.Send(data);
        }

    }
}
