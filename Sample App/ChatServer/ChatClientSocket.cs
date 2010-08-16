using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class ChatClientSocket : WebSocket<ChatMessage>
    {

        User me;
        
        public override void Incomming(ChatMessage msg)
        {
            if (msg.Message.Contains("/nick"))
            {
                me.Name = msg.Message.Replace("/nick ","");
                Send("you are now know as " + me.Name);
            }
            else
            {
                foreach (var user in ChatServer.Users)
                {
                    user.WebSocket.Send(msg.Sender.Name + ": " + msg.Message);
                }
            }
        }

        public override void Connected(ClientHandshake handshake)
        {
            me = new User() { Name = "john doe", WebSocket = this };
            ChatServer.Users.Add(me);
            
            foreach (var user in ChatServer.Users)
            {
                if (user != me)
                {
                    user.WebSocket.Send(me.Name+" connected");
                }
            }
        }

        public override void Disconnected()
        {
            foreach (var user in ChatServer.Users)
            {
                if (user != me)
                {
                    user.WebSocket.Send(me.Name + " disconnected");
                }
            }
            ChatServer.Users.Remove(me);
        }
    }
}
