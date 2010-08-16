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

            // create a new state object
            StateObject state = new StateObject();
            state.workSocket = Socket;

            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // wrap the array with the wrapper bytes
            byte[] wrappedArray = new byte[byteData.Length + 2];
            wrappedArray[0] = state.StartWrap;
            wrappedArray[wrappedArray.Length - 1] = state.EndWrap;
            Array.Copy(byteData, 0, wrappedArray, 1, byteData.Length);

            Log.Info(String.Format("sending {0} bytes to {1}", wrappedArray.Length, state.workSocket.RemoteEndPoint));
            // Begin sending the data to the remote device.
            Socket.BeginSend(wrappedArray, 0, wrappedArray.Length, 0,
                new AsyncCallback(SendCallback), state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            // get the state object
            StateObject state = (StateObject)ar.AsyncState;

            if (state.workSocket.Connected)
            {
                try
                {
                    Log.Debug("completing the send to " + state.workSocket.RemoteEndPoint);
                    // complete the send
                    state.workSocket.EndSend(ar);
                }
                catch (Exception e)
                {
                    state.workSocket.Close();
                    Log.Error("Exception thrown from method SendCallback:\n" + e.Message);
                    WebSocket.Disconnected();
                }
            }

        }

    }
}
