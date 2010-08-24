using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Microsoft.Practices.Unity;

namespace Nugget
{
    class Receiver
    {
        public const int BufferSize = 256;
        public Socket Socket { get; set; }
        public WebSocketWrapper WebSocket { get; set; }
        public SubProtocolModelFactoryWrapper Factory { get; set; }
        public WebSocketConnection Connection { get; set; }

        #region ctors

        public Receiver()
        {

        }

        public Receiver(Socket socket)
        {
            Socket = socket;
        }

        public Receiver(Socket socket, WebSocketWrapper websocket) : this(socket)
        {
            WebSocket = websocket;
        }

        public Receiver(Socket socket, WebSocketWrapper websocket, SubProtocolModelFactoryWrapper factory) : this(socket, websocket)
        {
            Factory = factory;
        }

        #endregion

        private object CreateModel(string data)
        {
            if (Factory != null)
            {
                // call the create method on the factory(wrapper)
                return Factory.Create(data, Connection);
            }
            else
            {
                return null;
            }
        }

        private bool ModelIsValid(object model)
        {
            bool isValid = false;
            if (Factory != null)
            {
                isValid = Factory.IsValid(model);
            }
            return isValid;
        }

        public void Receive(DataFrame frame = null)
        {

            if (frame == null)
                frame = new DataFrame();

            var buffer = new byte[BufferSize];

            if(Socket == null || !Socket.Connected)
                WebSocket.Disconnected();

            Socket.AsyncReceive(buffer, frame, (sizeOfReceivedData, df) =>
            {
                var dataframe = (DataFrame)df;

                if (sizeOfReceivedData > 0)
                {
                    dataframe.Append(buffer);

                    if (dataframe.IsComplete)
                    {
                        var data = dataframe.ToString();

                        var model = CreateModel(data);
                        var isValid = ModelIsValid(model);

                        // if the model was created it must be valid,
                        if (isValid && Factory != null || model == null && Factory == null)
                        {
                            if (model == null && Factory == null) // if the factory is null, use the raw string
                                model = (object)data;

                            WebSocket.Incomming(model);
                        }

                        Receive();

                    }
                    else // end is not is this buffer
                    {
                        Receive(dataframe); // continue to read
                    }
                }
                else // no data - the socket must be closed
                {
                    WebSocket.Disconnected();
                }
            });
        }
    }
}
