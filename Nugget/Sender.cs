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
            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // wrap the array with the wrapper bytes
            byte[] wrappedArray = new byte[byteData.Length + 2];
            wrappedArray[0] = 0;
            wrappedArray[wrappedArray.Length - 1] = 255;
            Array.Copy(byteData, 0, wrappedArray, 1, byteData.Length);

            Socket.AsyncSend(wrappedArray, (byteCount) => 
            {
                Log.Debug(byteCount + " bytes send to " + Socket.RemoteEndPoint);
            });
        }
        
    }
}
