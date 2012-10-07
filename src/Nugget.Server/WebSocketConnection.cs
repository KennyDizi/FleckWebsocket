using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget.Server
{
    /// <summary>
    /// Class representing a connection to a client
    /// </summary>
    public class WebSocketConnection
    {
        public event EventHandler<DataReceivedEventArgs> OnReceive;
        public event EventHandler<DisconnectedEventArgs> OnDisconnect;

        /// <summary>
        /// The socket connected to the client
        /// </summary>
        public Socket Socket { get; private set; }
        
        /// <summary>
        /// The handshake sent from the client upon connection
        /// </summary>
        public ClientHandshake Handshake { get; private set; }

        /// <summary>
        /// Create a new web socket connection
        /// </summary>
        /// <param name="socket">the connecting socket</param>
        /// <param name="handshake">the handshake sent upon connecting</param>
        public WebSocketConnection(Socket socket, ClientHandshake handshake)
        {
            Socket = socket;
            Handshake = handshake;
        }

        /// <summary>
        /// Asynchronously send data to the client
        /// </summary>
        /// <param name="data">the data to send</param>
        public void Send(string data)
        {
            if (Socket.Connected)
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                var fragment = DataFragment.Create(bytes, DataFragment.FragmentHead.Fin, DataFragment.FragmentOpcode.Text, true);
                Socket.AsyncSend(fragment.GetBytes(), (byteCount) =>
                {
                    Log.Debug(byteCount + " bytes send to " + Socket.RemoteEndPoint);
                });
            }
            else
            {
                OnDisconnect(this, null);
                Socket.Close();
            }
        }


        internal void StartReceiving()
        {
            if (Socket == null || !Socket.Connected)
                return;

            DataPackage.Read(Socket, (fragments) =>
            {
                if (OnReceive != null)
                {
                    OnReceive(this, new DataReceivedEventArgs(fragments));
                }
                StartReceiving();
            });
        }

    }

}
