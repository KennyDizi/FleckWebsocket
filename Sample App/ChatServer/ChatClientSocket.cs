using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    public class ChatClientSocket : IWebSocket<Message>, IWebSocket<string>
    {

        User me;


        public void Incomming(string data)
        {
            var x = 2;
        }

        public void Incomming(Message msg)
        {
            if (msg.Command == "nick")
            {
                me.Name = msg.Arguments[0];
                //Send("You are now known as " + me.Name);
                return;
            }
            
            foreach (var user in ChatServer.Users)
            {
                if (user.WebSocket != this)
                {
                    //user.WebSocket.Send(me.Name + ": " + msg.Msg);
                }
                else
                {
                    //Send("me: " + msg.Msg);
                }
            }
            
        }

        public void Connected(ClientHandshake handshake)
        {
            me = new User() { Name = "john doe", WebSocket = this };
            ChatServer.Users.Add(me);
            
            foreach (var user in ChatServer.Users)
            {
                if (user.WebSocket != this)
                {
                    //user.WebSocket.Send(me.Name+" connected");
                }
            }
        }

        public void Disconnected()
        {
            foreach (var user in ChatServer.Users)
            {
                if (user.WebSocket != this)
                {
                    //user.WebSocket.Send(me.Name + " disconnected");
                }
            }
            ChatServer.Users.Remove(me);
        }

        
    }
}
