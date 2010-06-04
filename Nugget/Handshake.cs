using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nugget
{
    /// <summary>
    /// Supported web socket protocols
    /// </summary>
    public enum WebSocketProtocolIdentifier
    {
        Unknown,
        draft_hixie_thewebsocketprotocol_75,
        //draft_hixie_thewebsocketprotocol_76,
        //draft_ietf_hybi_thewebsocketprotocol_00,
    }

    /// <summary>
    /// Represents a handshake. The class knows the format of the handshake, both from the client and the host.
    /// </summary>
    class Handshake
    {
        /// <summary>
        /// The web socket protocol the client is using
        /// </summary>
        public WebSocketProtocolIdentifier Protocol { get; private set; }

        // supported handshakes from the client
        private Dictionary<WebSocketProtocolIdentifier, string> ClientPatterns = new Dictionary<WebSocketProtocolIdentifier,string>()
        {
            {
                WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75, 
                @"^(?<connect>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\n" +
                @"Upgrade:\sWebSocket\n" +
                @"Connection:\sUpgrade\n" +
                @"Host:\s(?<host>[^\n]+)\n" +
                @"Origin:\s(?<origin>[^\n]+)\n\n$"
            },
        };

        // respose handshakes
        private Dictionary<WebSocketProtocolIdentifier, string> HostResponses = new Dictionary<WebSocketProtocolIdentifier, string>()
        {
            {
                WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75, 
                "HTTP/1.1 101 Web Socket Protocol Handshake\n"+
                "Upgrade: WebSocket\n"+
                "Connection: Upgrade\n"+
                "WebSocket-Origin: {ORIGIN}\n"+
                "WebSocket-Location: {LOCATION}\n"+
                "\n"
            },
        };

        /// <summary>
        /// Gets the fields of the handshake
        /// </summary>
        public GroupCollection Fields { get; private set; }

        /// <summary>
        /// Instantiates a new Handshake class
        /// </summary>
        /// <param name="handshake">the handshake received from the client</param>
        public Handshake(string handshake)
        {
            handshake = handshake.Replace("\r\n", "\n");
            foreach (var pattern in ClientPatterns)
            {
                var regex = new Regex(pattern.Value);
                var match = regex.Match(handshake);
                if (match.Success)
                {
                    Fields = match.Groups;
                    Protocol = pattern.Key;
                    return;
                }
            }

            Protocol = WebSocketProtocolIdentifier.Unknown;
        }

        /// <summary>
        /// Get the expected response to the handshake. The string contains placeholders for fields that needs to be filled out, before sending the handshake to the client.
        /// </summary>
        /// <returns>string</returns>
        public string GetHostResponse()
        {
            return HostResponses[Protocol];
        }

    }
}
