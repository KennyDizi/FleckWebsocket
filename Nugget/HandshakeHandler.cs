using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;


namespace Nugget
{
    /// <summary>
    /// Handles the handshaking between the client and the host, when a new connection is created
    /// </summary>
    class HandshakeHandler
    {
        public string Origin { get; set; }
        public string Location { get; set; }

        public HandshakeHandler(string origin, string location)
        {
            Origin = origin;
            Location = location;
        }

        class HandShakeState
        {
            public Socket workSocket;
            public Action<Handshake, Socket> callback;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public Handshake handshake;
        }

   
        /// <summary>
        /// Shake hands with the connecting socket
        /// </summary>
        /// <param name="socket">The socket to send the handshake to</param>
        /// <param name="callback">a callback function that is called when the send has completed</param>
        public void Shake(Socket socket, Action<Handshake, Socket> callback)
        {
            try
            {
                // Create the state object.
                HandShakeState state = new HandShakeState();
                state.workSocket = socket;
                state.callback = callback;

                // Begin receiving the data from the remote device.
                state.workSocket.BeginReceive(state.buffer, 0, HandShakeState.BufferSize, 0,
                    new AsyncCallback(ReadShake), state);
            }
            catch (Exception e)
            {
                Log.Error("Exception thrown from method Receive:\n" + e.Message);
            }
        }

        private void ReadShake(IAsyncResult ar)
        {
            var state = (HandShakeState)ar.AsyncState;
            int size = state.workSocket.EndReceive(ar);
            var handshake = new Handshake(state.buffer, size);
            state.handshake = handshake;
            Log.Debug("protocol identified as: " + handshake.Protocol);

            string response = "";
            byte[] MD5Answer = null;

            // check if the client handshake is valid
            switch (handshake.Protocol)
            {
                case WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75:
                    if (handshake.Fields == null ||
                        handshake.Fields["origin"] != Origin || // is the connection comming from the right place
                        handshake.Fields["host"] != Location.Replace("ws://", "")) // is the connection trying to connect to us
                    {
                        throw new Exception("client handshake was invalid");
                    }
                    else
                    {
                        response = handshake.GetHostResponse()
                            .Replace("{ORIGIN}", Origin)
                            .Replace("{LOCATION}", Location + handshake.Fields["path"]);
                    }
                    break;
                case WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00:
                    if (handshake.Fields == null ||
                        handshake.Fields["origin"] != Origin || // is the connection comming from the right place
                        handshake.Fields["host"] != Location.Replace("ws://", "")) // is the connection trying to connect to us
                    {
                        throw new Exception("client handshake was invalid");
                    }
                    else
                    {
                        // calculate the handshake proof
                        // the following code is to conform with the protocol

                        var key1 = handshake.Fields["sec-websocket-key1"];
                        var key2 = handshake.Fields["sec-websocket-key2"];

                        // concat all digits and count the spaces
                        var sb1 = new StringBuilder();
                        var sb2 = new StringBuilder();
                        int spaces1 = 0;
                        int spaces2 = 0;

                        for (int i = 0; i < key1.Length; i++)
                        {
                            if (Char.IsDigit(key1[i]))
                                sb1.Append(key1[i]);
                            else if (key1[i] == ' ')
                                spaces1++;
                        }

                        for (int i = 0; i < key2.Length; i++)
                        {
                            if (Char.IsDigit(key2[i]))
                                sb2.Append(key2[i]);
                            else if (key2[i] == ' ')
                                spaces2++;
                        }

                        // divide the digits with the number of spaces
                        Int32 result1 = (Int32)(Int64.Parse(sb1.ToString()) / spaces1);
                        Int32 result2 = (Int32)(Int64.Parse(sb2.ToString()) / spaces2);

                        // get the last 8 byte of the client handshake
                        byte[] challenge = new byte[8];
                        Array.Copy(handshake.Raw, size - 8, challenge, 0, 8);

                        // convert the results to 32 bit big endian byte arrays
                        byte[] result1bytes = BitConverter.GetBytes(result1);
                        byte[] result2bytes = BitConverter.GetBytes(result2);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(result1bytes);
                            Array.Reverse(result2bytes);
                        }

                        // concat the two integers and the 8 bytes from the client
                        byte[] answer = new byte[16];
                        Array.Copy(result1bytes, 0, answer, 0, 4);
                        Array.Copy(result2bytes, 0, answer, 4, 4);
                        Array.Copy(challenge, 0, answer, 8, 8);

                        // compute the md5 hash
                        MD5 md5 = System.Security.Cryptography.MD5.Create();
                        MD5Answer = md5.ComputeHash(answer);

                        // put the relevant info into the response (the 
                        response = handshake.GetHostResponse()
                            .Replace("{ORIGIN}", Origin)
                            .Replace("{LOCATION}", Location + handshake.Fields["path"]);

                        // just echo the subprotocol for now. This should be picked up and made avaialbe to the application implementation.
                        if (handshake.Fields.Keys.Contains("sec-websocket-protocol"))
                            response = response.Replace("{PROTOCOL}", handshake.Fields["sec-websocket-protocol"]);
                        else
                            response = response.Replace("Sec-WebSocket-Protocol: {PROTOCOL}\r\n", "");
                    }
                    break;
                case WebSocketProtocolIdentifier.Unknown:
                default:
                    throw new Exception("client handshake was invalid"); // the client handshake was not valid
            }

            // send the handshake, line by line
            Log.Debug("sending handshake");
            byte[] byteResponse = Encoding.UTF8.GetBytes(response);

            // if this is using the draft_ietf_hybi_thewebsocketprotocol_00 protocol, we need to send to answer to the challenge
            if (handshake.Protocol == WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00)
            {
                //Log.Debug("send: answer to challenge");
                int byteResponseLength = byteResponse.Length;
                Array.Resize(ref byteResponse, byteResponseLength + MD5Answer.Length);
                Array.Copy(MD5Answer, 0, byteResponse, byteResponseLength, MD5Answer.Length);


            }
            state.workSocket.BeginSend(byteResponse, 0, byteResponse.Length, 0, SendCallback, state);

        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (HandShakeState)ar.AsyncState;
            state.workSocket.EndSend(ar);
            state.callback.BeginInvoke(state.handshake, state.workSocket, null, null);
        }
    }
}
