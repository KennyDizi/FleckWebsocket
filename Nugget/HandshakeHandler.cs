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

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        public HandshakeHandler(string origin, string location)
        {
            Origin = origin;
            Location = location;
        }

        /// <summary>
        /// Shake hands with the client
        /// </summary>
        /// <param name="socket">the socket connecting the client</param>
        /// <param name="origin">the allowed origin of the client</param>
        /// <param name="location">the location of the server</param>
        /// <returns></returns>
        public Handshake Shake(Socket socket)
        {
            Handshake handshake = null;

            using (var stream = new NetworkStream(socket))
            using (var writer = new StreamWriter(stream))
            {
                // read the client handshake
                var handshakeBuilder = new StringBuilder();
                Log.Debug("reading client handshake");
                byte[] byteHandshake = new byte[1024];

                int size = socket.Receive(byteHandshake);
                

                // use the Handshake class to identify the protocol, and parse out the relevant information from the handshake
                handshake = new Handshake(byteHandshake, size);
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
                foreach (var line in response.Split('\n'))
                {
                    Log.Debug("send: " + line);
                    writer.WriteLine(line);
                }
                writer.Flush();

                // if this is using the draft_ietf_hybi_thewebsocketprotocol_00 protocol, we need to send to answer to the challenge
                if (handshake.Protocol == WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00)
                {
                    Log.Debug("send: answer to challenge");
                    socket.Send(MD5Answer);
                }
                 

            }
            return handshake;
        }
    }
}
