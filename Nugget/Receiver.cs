using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget
{
    class Receiver
    {
        public Socket Socket { get; set; }
        public WebSocketWrapper WebSocket { get; set; }
        public ModelFactoryWrapper Factory { get; set; }
        public WebSocketConnection Connection { get; set; }

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

        public Receiver(Socket socket, WebSocketWrapper websocket, ModelFactoryWrapper factory) : this(socket, websocket)
        {
            Factory = factory;
        }


        private void Read(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            int sizeOfReceivedData = 0;
            try
            {
                sizeOfReceivedData = state.workSocket.EndReceive(ar);
            }
            catch (Exception e)
            {
                Log.Error("Exception thrown from method Read:\n" + e.Message);
                WebSocket.Disconnected();
                return;
            }


            if (sizeOfReceivedData > 0)
            {
                int start = 0, end = state.buffer.Length - 1;

                // if we are not already reading something, look for the start byte as specified in the protocol
                if (!state.readingData)
                {
                    for (start = 0; start < state.buffer.Length - 1; start++)
                    {
                        if (state.buffer[start] == state.StartWrap)
                        {
                            state.readingData = true; // we found the begining and can now start reading
                            start++; // we dont need the start byte. Incrementing the start counter will walk us past it
                            break;
                        }
                    }
                } // no else here, the value of readingData might have changed

                // if a begining was found in the buffer, or if we are continuing from another buffer
                if (state.readingData)
                {
                    bool endIsInThisBuffer = false;
                    // look for the end byte in the received data
                    for (end = start; end < sizeOfReceivedData; end++)
                    {
                        byte currentByte = state.buffer[end];
                        if (state.buffer[end] == state.EndWrap)
                        {
                            endIsInThisBuffer = true; // we found the ending byte
                            break;
                        }
                    }

                    // the end is in this buffer, which means that we are done reading
                    if (endIsInThisBuffer)
                    {
                        // we are no longer reading data
                        state.readingData = false;
                        // put the data into the string builder
                        state.sb.Append(Encoding.UTF8.GetString(state.buffer, start, end - start));
                        // trigger the event
                        int size = Encoding.UTF8.GetBytes(state.sb.ToString().ToCharArray()).Length;
                        Log.Info(String.Format("Received {0} bytes of data from {1}", sizeOfReceivedData, state.workSocket.RemoteEndPoint));

                        var data = state.sb.ToString();
                        if (Factory != null)
                        {
                            WebSocket.Incomming(Factory.Create(data, Connection));
                        }
                        else
                        {
                            WebSocket.Incomming(data);
                        }

                    }
                    else // if the end is not in this buffer then put everyting from start to the end of the buffer into the datastring and keep on reading
                    {
                        state.sb.Append(Encoding.UTF8.GetString(state.buffer, start, end - start));
                    }
                }

                // continue listening for more data
                Receive();
            }
            else // the socket is closed
            {
                WebSocket.Disconnected();
            }
        }

        public void Receive()
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = Socket;

                // Begin receiving the data from the remote device.
                Socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(Read), state);
            }
            catch (Exception e)
            {
                Log.Error("Exception thrown from method Receive:\n" + e.Message);
            }
        }


    }
}
