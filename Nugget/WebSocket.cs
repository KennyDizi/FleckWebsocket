using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget
{

    public abstract class WebSocket
    {
        private Socket _socket;
        public Socket Socket
        {
            get { return _socket; }
            set { _socket = value; Receive(); }
        }
        public WebSocketProtocolIdentifier Protocol { get; set; }

        public abstract void Incomming(string data);
        public abstract void Disconnected();
        public abstract void Connected();

        #region state obj
        // State object for receiving data from remote device.
        private class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
            public bool readingData = false;
            public byte StartWrap = 0;
            public byte EndWrap = 255;
        }

        #endregion

        #region receive

        private void Read(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            int sizeOfReceivedData = 0;
            try
            {
                sizeOfReceivedData = state.workSocket.EndReceive(ar);
            }
            catch (Exception)
            {
                Console.WriteLine("read error");
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
                        Incomming(state.sb.ToString());
                        
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
                Disconnected();
            }
        }

        private void Receive()
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
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region send

        public void Send(string data)
        {
            // create a new state object
            StateObject state = new StateObject();
            state.workSocket = Socket;
           
            Socket.Send(new byte[] { state.StartWrap });

            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.
            Socket.BeginSend(byteData, 0, byteData.Length, 0,
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
                    // send to initial wrapper byte
                    state.workSocket.Send(new byte[] { state.StartWrap }, 1, 0);

                    // complete the send
                    state.workSocket.EndSend(ar);
                    // end with a end wrapper byte
                    state.workSocket.Send(new byte[] { state.EndWrap }, 1, 0);
                }
                catch
                {
                    Console.WriteLine("send error");
                    Disconnected();
                }
            }

        }
        #endregion
    }

}
