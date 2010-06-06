using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatServer
{
    class User
    {
        public string Name { get; set; }
        public ChatClientSocket WebSocket { set; get; }
    }
}
