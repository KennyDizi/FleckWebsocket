using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Nugget
{
    /// <summary>
    /// Handles the handshaking between the client and the host, when a new connection is created
    /// </summary>
    class HandshakeHandler
    {
        /// <summary>
        /// Shake hands with the client
        /// </summary>
        /// <param name="socket">the socket connecting the client</param>
        /// <param name="origin">the allowed origin of the client</param>
        /// <param name="location">the location of the server</param>
        /// <returns></returns>
        public Handshake Shake(Socket socket, string origin, string location)
        {
            Handshake handshake = null;
            using (var stream = new NetworkStream(socket))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                // read the client handshake
                var handshakeBuilder = new StringBuilder();
                var handshakeLine = "";
                do
                {
                    handshakeLine = reader.ReadLine();
                    handshakeBuilder.AppendLine(handshakeLine);
                } while (handshakeLine != "");

                // use the Handshake class to identify the protocol, and parse out the relevant information from the handshake
                handshake = new Handshake(handshakeBuilder.ToString());

                // check if the client handshake is valid
                switch (handshake.Protocol)
                {
                    case WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75:
                        if (handshake.Fields == null ||
                            handshake.Fields["origin"].Value != origin || // is the connection comming from the right place
                            handshake.Fields["host"].Value != location.Replace("ws://", "")) // is the connection trying to connect to us
                        {
                            throw new Exception("client handshake was invalid");
                        }
                        break;

                    case WebSocketProtocolIdentifier.Unknown:
                    default:
                        throw new Exception("client handshake was invalid"); // the client handshake was not valid
                }

                // put the relevant information into the handshake to send back to the client
                string response = handshake.GetHostResponse()
                    .Replace("{ORIGIN}", origin)
                    .Replace("{LOCATION}", location + handshake.Fields["path"].Value);

                // send the handshake, line by line
                foreach (var line in response.Split('\n'))
                {
                    writer.WriteLine(line);
                }
            }
            return handshake;
        }

    }
}
