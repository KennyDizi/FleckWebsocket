using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nugget
{
    public enum WebSocketProtocolIdentifier
    {
        Unsupported,
        draft_hixie_thewebsocketprotocol_75,
        //draft_hixie_thewebsocketprotocol_76,
        //draft_ietf_hybi_thewebsocketprotocol_00,
    }

    class Handshake
    {
        public WebSocketProtocolIdentifier Protocol { get; private set; }

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

        public GroupCollection Fields { get; private set; }

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

            Protocol = WebSocketProtocolIdentifier.Unsupported;
        }

        public string GetHostResponse()
        {
            return HostResponses[Protocol];
        }

    }
}
