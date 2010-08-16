using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatServer
{
    public class User
    {
        public string Name { get; set; }
        public ChatClientSocket WebSocket { set; get; }
    }
}
