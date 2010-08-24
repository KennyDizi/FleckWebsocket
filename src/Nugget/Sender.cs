using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget
{
    class Sender
    {
        public Socket Socket { get; set; }
        public WebSocketWrapper WebSocket { get; set; }
        public WebSocketConnection Connection { get; set; }

        public Sender()
        {

        }
        
        public Sender(Socket socket)
        {
            Socket = socket;
        }

        public Sender(Socket socket, WebSocketWrapper websocket) : this(socket)
        {
            WebSocket = websocket;
        }

        public void Send(string data)
        {
            Socket.AsyncSend(DataFrame.Wrap(data), (byteCount) => 
            {
                Log.Debug(byteCount + " bytes send to " + Socket.RemoteEndPoint);
            });
        }
        
    }
}
