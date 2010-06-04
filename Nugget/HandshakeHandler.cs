using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace Nugget
{
    class HandshakeHandler
    {
        public Handshake Shake(Socket socket, string origin, string location)
        {
            Handshake handshake = null;
            using (var stream = new NetworkStream(socket))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                var pathStripper = new Regex(@"ws:\/\/(\w+:[0-9]+).*"); // cut the path just after the port number
                var strippedLocation = "";
                if (pathStripper.IsMatch(location))
                {
                    strippedLocation = pathStripper.Replace(location, "$1");
                }


                var handshakeBuilder = new StringBuilder();
                var handshakeLine = "";
                do
                {
                    handshakeLine = reader.ReadLine();
                    handshakeBuilder.AppendLine(handshakeLine);
                } while (handshakeLine != "");

                string shake = handshakeBuilder.ToString();
                handshake = new Handshake(handshakeBuilder.ToString());

                if (handshake.Fields == null || handshake.Fields["origin"].Value != origin || handshake.Fields["host"].Value != strippedLocation)
                {
                    throw new Exception("client handshake was invalid");
                }

                string response = handshake.GetHostResponse()
                    .Replace("{ORIGIN}", origin)
                    .Replace("{LOCATION}", location + handshake.Fields["path"].Value);

                foreach (var line in response.Split('\n'))
                {
                    writer.WriteLine(line);
                    Console.WriteLine(line);
                }
            }
            return handshake;
        }
    }
}
