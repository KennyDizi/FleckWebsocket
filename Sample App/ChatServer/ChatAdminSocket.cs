using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    class ChatAdminSocket : ChatClientSocket
    {
        public override void Incomming(string data)
        {
            if (data.StartsWith("/kick "))
            {
                var name = data.Replace("/kick ", "");
                var userToKick = ChatServer.Users.SingleOrDefault(x => x.Name == name);

                if (userToKick != null)
                {
                    ChatServer.Users.Remove(userToKick);
                    try
                    {
                        userToKick.WebSocket.Socket.Close();
                    }
                    catch (Exception)
                    {
                        // blah
                    }
                    
                }
                    
            }
            else
                base.Incomming(data);
        }
    }
}
