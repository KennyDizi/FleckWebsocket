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
        public const int BufferSize = 512;
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

        public void Receive(StringBuilder sb = null)
        {
            
            if (sb == null)
                sb = new StringBuilder();

            var buffer = new byte[BufferSize];

            Socket.AsyncReceive(buffer, sb, (sizeOfReceivedData, stringBuilder) =>
            {
                var builder = (StringBuilder)stringBuilder;

                if (sizeOfReceivedData > 0)
                {
                    int start = 0, end = buffer.Length - 1;
                    
                    var bufferList = buffer.ToList();
                    bool endIsInThisBuffer = buffer.Contains((byte)255); // 255 = end
                    if (endIsInThisBuffer)
                    {
                        end = bufferList.IndexOf((byte)255);
                        end--; // we dont want to include this byte
                    }

                    bool startIsInThisBuffer = buffer.Contains((byte)0); // 0 = start
                    if (startIsInThisBuffer)
                    {
                        var zeroPos = bufferList.IndexOf((byte)0);
                        if (zeroPos < end) // we might be looking at one of the bytes in the end of the array that hasn't been set
                        {
                            start = bufferList.IndexOf((byte)0);
                            start++; // we dont want to include this byte
                        }
                    }
                    
                    builder.Append(Encoding.UTF8.GetString(buffer, start, (end - start) + 1));

                    if (endIsInThisBuffer)
                    {
                        var data = builder.ToString();

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
                        Receive(builder); // continue to read
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
