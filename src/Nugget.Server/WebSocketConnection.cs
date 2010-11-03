using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget.Server
{

    public class WebSocketConnection
    {
        public Socket Socket { get; private set; }
        public ClientHandshake Handshake { get; private set; }
        
        private Action<WebSocketConnection, string> _receiveCallback;
        private Action<WebSocketConnection> _disconnectCallback;

        public const int BufferSize = 256;

        public WebSocketConnection(Socket socket, ClientHandshake handshake, Action<WebSocketConnection, string> receiveCallback, Action<WebSocketConnection> disconnectCallback)
        {
            Socket = socket;
            Handshake = handshake;

            _receiveCallback = receiveCallback;
            _disconnectCallback = disconnectCallback;
        }

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

                        _receiveCallback(this, data);

                        StartReceiving(); // start looking again
                    }
                    else // end is not is this buffer
                    {
                        StartReceiving(dataframe); // continue to read
                    }
                }
                else // no data - the socket must be closed
                {
                    _disconnectCallback(this);
                }
            });
        }

    }

}
