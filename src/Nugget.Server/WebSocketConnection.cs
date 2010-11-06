using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget.Server
{
    /// <summary>
    /// Called when a client receives data
    /// </summary>
    /// <param name="wsc">the connection representing the client receiving the data</param>
    /// <param name="data">the data received</param>
    public delegate void ReceiveEventHandler(WebSocketConnection wsc, string data);
    
    /// <summary>
    /// Called when the client disconnects
    /// </summary>
    /// <param name="wsc">the connection representing the client disconnecting</param>
    public delegate void DisconnectedEventHandler(WebSocketConnection wsc);

    /// <summary>
    /// Class representing a connection to a client
    /// </summary>
    public class WebSocketConnection
    {

        public event ReceiveEventHandler OnReceive;
        public event DisconnectedEventHandler OnDisconnect;

        /// <summary>
        /// The socket connected to the client
        /// </summary>
        public Socket Socket { get; private set; }
        
        /// <summary>
        /// The handshake sent from the client upon connection
        /// </summary>
        public ClientHandshake Handshake { get; private set; }

        /// <summary>
        /// The size of the buffer used when data is sent or received
        /// </summary>
        public const int BufferSize = 256;
        
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
                Socket.AsyncSend(DataFrame.Wrap(data), (byteCount) =>
                {
                    Log.Debug(byteCount + " bytes send to " + Socket.RemoteEndPoint);
                });
            }
            else
            {
                OnDisconnect(this);
                Socket.Close();
            }

        }


        public void StartReceiving(DataFrame frame = null)
        {

            if (frame == null)
                frame = new DataFrame();

            var buffer = new byte[BufferSize];

            if (Socket == null || !Socket.Connected)
                return;

            Socket.AsyncReceive(buffer, frame, (sizeOfReceivedData, df) =>
            {
                var dataframe = (DataFrame)df;

                if (sizeOfReceivedData > 0)
                {
                    dataframe.Append(buffer);

                    if (dataframe.IsComplete)
                    {
                        var data = dataframe.ToString();

                        OnReceive(this, data);

                        StartReceiving(); // start looking again
                    }
                    else // end is not is this buffer
                    {
                        StartReceiving(dataframe); // continue to read
                    }
                }
                else // no data - the socket must be closed
                {
                    OnDisconnect(this);
                }
            });
        }

    }

}
