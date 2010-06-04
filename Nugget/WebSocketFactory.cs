using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Microsoft.Practices.Unity;

namespace Nugget
{
    /// <summary>
    /// Class for registering and instantiating WebSocketClients
    /// </summary>
    class WebSocketFactory
    {
        private Dictionary<string, Type> types = new Dictionary<string, Type>();
        private UnityContainer container = new UnityContainer();

        /// <summary>
        /// Register a new web socket client
        /// </summary>
        /// <typeparam name="T">The web socket client type</typeparam>
        /// <param name="path">The path that the client should respond to</param>
        public void Register<T>(string path) where T : WebSocket
        {
            if (!types.ContainsKey(path))
            {
                types[path] = typeof(T);
                container.RegisterType<T>();
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
            if (types.ContainsKey(path))
            {
                var ws = (WebSocket)container.Resolve(types[path]);
                ws.Socket = socket;
                ws.Connected();
                return ws;
            }
            else
            {
                throw new Exception("Path: '" + path + "' is not defined");
            }
        }
    }
}
