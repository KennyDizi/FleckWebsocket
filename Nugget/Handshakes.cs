using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace Nugget
{
    public class ClientHandshake
    {
        public string Origin { get; set; }
        public string Host { get; set; }
        public string ResourcePath { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public ArraySegment<byte> ChallengeBytes { get; set; }
        public HttpCookieCollection Cookies { get; set; }
        public string SubProtocol { get; set; }
        public Dictionary<string,string> AdditionalFields { get; set; }
    }

    class ServerHandshake
    {
        public string Origin { get; set; }
        public string Location { get; set; }
        public byte[] AnswerBytes { get; set; }
        public string SubProtocol { get; set; }
        public Dictionary<string, string> AdditionalFields { get; set; }
    }
}
