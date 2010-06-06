using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class ChatClientSocket : WebSocket
    {

        User me;

        public override void Incomming(string data)
        {
            if (data.StartsWith("/nick "))
            {
                me.Name = data.Replace("/nick ", "");
                Send("You are now know as " + me.Name);
            }
            else
            {
                foreach (var user in ChatServer.Users)
                {
                    if (user.WebSocket != this)
                    {
                        user.WebSocket.Send(me.Name + ": " + data);
                    }
                    else
                    {
                        Send("me: " + data);
                    }
                }
            }
        }

        public override void Connected()
        {
            me = new User() { Name = "john doe", WebSocket = this };
            ChatServer.Users.Add(me);
            
            foreach (var user in ChatServer.Users)
            {
                if (user.WebSocket != this)
                {
                    user.WebSocket.Send(me.Name+" connected");
                }
            }
        }

        public override void Disconnected()
        {
            foreach (var user in ChatServer.Users)
            {
                if (user.WebSocket != this)
                {
                    user.WebSocket.Send(me.Name + " disconnected");
                }
            }
            ChatServer.Users.Remove(me);
        }
    }
}
