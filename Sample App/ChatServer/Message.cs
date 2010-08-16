using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class MessageFactory : ISubProtocolModelFactory<ChatMessage>
    {
        public ChatMessage Create(string data, WebSocketConnection connection)
        {
            var m = new ChatMessage();
            m.Message = data;
            m.Sender = ChatServer.Users.SingleOrDefault(x => x.WebSocket.Connection == connection);
            return m;
        }
    }


    public class ChatMessage
    {
        public User Sender { get; set; }
        public string Message { get; set; }
    }
}
