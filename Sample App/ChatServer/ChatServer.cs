﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;

namespace ChatServer
{
    class ChatServer
    {
        WebSocketServer wss;
        public static List<User> Users = new List<User>();

        public ChatServer()
        {
            wss = new WebSocketServer(8181, "http://localhost:8080", "ws://localhost:8181");
            wss.RegisterHandler<ChatClientSocket>("/chat");
            wss.RegisterHandler<ChatAdminSocket>("/admin");
            wss.Start();
            KeepAlive();
        }

        private void KeepAlive()
        {
            string r = Console.ReadLine();
            while (r != "quit")
            {
                if(r == "users")
                {
                    Console.WriteLine(Users.Count);
                }
                r = Console.ReadLine();
            }
        }
    }
}
