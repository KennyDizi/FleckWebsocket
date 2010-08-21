using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget;
using JSONParser;

namespace SubProtocol
{
    class Post
    {
        public string Author { get; set; }
        public string Body { get; set; }

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Author) && 
                   !String.IsNullOrEmpty(Body);
        }
    }

    class PostFactory : ISubProtocolModelFactory<Post>
    {
        public Post Create(string data, WebSocketConnection connection)
        {
            var js = JSON.Parse(data);
            if (js.hasOwnProperty("author") && js.hasOwnProperty("body"))
            {
                return new Post() { Author = js.author, Body = js.body };
            }
            return null;
        }

        public bool IsValid(Post p)
        {
            
            if (p == null)
                return false;
            else
                return p.IsValid();
        }
    }

    class PostSocket : WebSocket<Post>
    {
        public override void Incomming(Post post)
        {
            Console.WriteLine("{0} posted {1}",post.Author,post.Body);
        }

        public override void Disconnected()
        {
            Console.WriteLine("--- disconnected ---");
        }

        public override void Connected(ClientHandshake handshake)
        {
            Console.WriteLine("--- connected ---");
        }
    }

    class Server
    {
        static void Main(string[] args)
        {
            var nugget = new WebSocketServer(8181, "null", "ws://localhost:8181");
            nugget.RegisterHandler<PostSocket>("/subsample");
            nugget.SetSubProtocolModelFactory(new PostFactory(), "post");
            nugget.Start();
            Console.WriteLine("Server started, open client.html in a websocket-enabled browser");
            
            var input = Console.ReadLine();
            while (input != "exit")
            {
                input = Console.ReadLine();
            }

        }
    }
}
