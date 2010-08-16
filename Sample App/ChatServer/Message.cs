using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class MessageFactory : ISubProtocolModelFactory<Message>
    {
        public Message Create(string msg)
        {
            var m = new Message();
            if (msg.Contains("/"))
            {
                var words = msg.Split(' ');
                m.Command = words[0].Substring(1);
                foreach (var word in words)
                {
                    m.Arguments.Add(word);
                }
            }
            else
            {
                m.Msg = msg;
            }
            return m;
        }
    }


    public class Message
    {
        public string Command { get; set; }
        public List<string> Arguments { get; set; }
        public string Msg { get; set; }

        public Message()
        {
               
        }

        public Message(string msg)
        {
            Msg = msg;
        }
    }
}
