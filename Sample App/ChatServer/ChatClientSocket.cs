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
        public static int num = 0;
        private int myNum;

        public ChatClientSocket()
        {
            num++;
            myNum = num;
        }

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
                    if (user.Socket != this)
                    {
                        user.Socket.Send(me.Name + ": " + data);
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
            me = new User() { Name = "john doe", Socket = this };
            ChatServer.Users.Add(me);
            
            foreach (var user in ChatServer.Users)
            {
                if (user.Socket != this)
                {
                    user.Socket.Send(me.Name+" connected");
                }
            }
        }

        public override void Disconnected()
        {
            foreach (var user in ChatServer.Users)
            {
                if (user.Socket != this)
                {
                    user.Socket.Send(me.Name + " disconnected");
                }
            }
        }
    }
}
