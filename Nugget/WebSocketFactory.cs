using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget
{
    /// <summary>
    /// Class for registering and instantiating WebSocketClients
    /// </summary>
    class WebSocketFactory
    {
        private Dictionary<string, Type> ClientTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Register a new web socket client
        /// </summary>
        /// <typeparam name="T">The web socket client type</typeparam>
        /// <param name="path">The path that the client should respond to</param>
        public void Register<T>(string path) where T : WebSocket
        {
            if (!ClientTypes.ContainsKey(path))
            {
                if (path == "")
                    path = "/";
                ClientTypes[path] = typeof(T);
            }
            else
            {
                throw new Exception("Path: '"+path+"' has already been set");
            }
        }

        /// <summary>
        /// Instantiate a new client
        /// </summary>
        /// <param name="path">The path the client was registered at</param>
        /// <returns>The instantiated WebSocketClient</returns>
        public WebSocket Create(string path, Socket socket)
        {
            if (path == "")
                path = "/";
            if (ClientTypes.ContainsKey(path))
            {
                var ctors = ClientTypes[path].GetConstructors();
                var ctor = ctors
                var obj = ctor.Invoke(new object[] { socket });
                var wsc = (WebSocket)obj;
                
                return wsc;
            }
            else
            {
                throw new Exception("Path: '" + path + "' is not defined");
            }
        }
    }
}
