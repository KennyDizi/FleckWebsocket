using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading;

namespace Nugget
{
    /// <summary>
    /// Supported web socket protocols
    /// </summary>
    public enum WebSocketProtocolIdentifier
    {
        Unknown,
        draft_hixie_thewebsocketprotocol_75,
        draft_ietf_hybi_thewebsocketprotocol_00, // aka draft-hixie-thewebsocketprotocol-76
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
            {
                WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00,
                @"^(?<connect>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\n" + 
                @"((?<field_name>[^:\s]+):\s(?<field_value>[^\n]+)\n)+"
            }
        };

        // respose handshakes
        private Dictionary<WebSocketProtocolIdentifier, string> HostResponses = new Dictionary<WebSocketProtocolIdentifier, string>()
        {
            {
                WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75, 
                "HTTP/1.1 101 Web Socket Protocol Handshake\r\n"+
                "Upgrade: WebSocket\r\n"+
                "Connection: Upgrade\r\n"+
                "WebSocket-Origin: {ORIGIN}\r\n"+
                "WebSocket-Location: {LOCATION}\r\n"+
                "\r\n"
            },
            {
                WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00,
                "HTTP/1.1 101 Web Socket Protocol Handshake\r\n"+
                "Upgrade: WebSocket\r\n"+
                "Connection: Upgrade\r\n"+
                "Sec-WebSocket-Origin: {ORIGIN}\r\n"+
                "Sec-WebSocket-Location: {LOCATION}\r\n"+
                "Sec-WebSocket-Protocol: {PROTOCOL}\r\n"+
                "\r\n"
            },
        };

        /// <summary>
        /// Gets the fields of the handshake
        /// </summary>
        public Dictionary<string,string> Fields { get; private set; }
        public byte[] Raw { get; private set; }

        /// <summary>
        /// Instantiates a new Handshake class
        /// </summary>
        /// <param name="handshake">the handshake received from the client</param>
        public Handshake(byte[] handshakeRaw, int length)
        {
            Raw = handshakeRaw;
            var handshake = Encoding.UTF8.GetString(handshakeRaw, 0, length).Replace("\r\n", "\n");
            Log.Debug("client handshake:\n"+handshake);            

            foreach (var pattern in ClientPatterns)
            {
                var regex = new Regex(pattern.Value);
                var match = regex.Match(handshake);
                if (match.Success)
                {
                    Protocol = pattern.Key;
                    SetFields(match.Groups);
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

        private void SetFields(GroupCollection gc)
        {
            switch (Protocol)
            {
                case WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75:
                    Fields = new Dictionary<string, string>();
                    Fields.Add("connect", gc["connect"].ToString());
                    Fields.Add("path", gc["path"].ToString());
                    Fields.Add("host", gc["host"].ToString());
                    Fields.Add("origin", gc["origin"].ToString());

                    break;
                case WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00:
                    Fields = new Dictionary<string, string>();

                    for (int i = 0; i < gc["field_name"].Captures.Count; i++)
                    {
                        Fields.Add(gc["field_name"].Captures[i].ToString().ToLower(), gc["field_value"].Captures[i].ToString());
                    }

                    Fields.Add("connect", gc["connect"].ToString());
                    Fields.Add("path", gc["path"].ToString());

                    break;
                case WebSocketProtocolIdentifier.Unknown:    
                default:
                    break;
            }
        }

    }
}
