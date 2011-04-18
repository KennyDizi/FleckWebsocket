using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Nugget.Server;

namespace Nugget.Framework
{
    /// <summary>
    /// Class for registering and instantiating WebSocketClients
    /// </summary>
    public class WebSocketFactory
    {
        private Dictionary<string, Type> _types = new Dictionary<string, Type>();
        private Dictionary<WebSocketConnection, WebSocket> _userSockets = new Dictionary<WebSocketConnection, WebSocket>();
        private WebSocketServer _server;
        

        public WebSocketFactory(WebSocketServer server)
        {
            _server = server;
            _server.OnConnect += new ConnectedEventHandler(HandleConnection);
        }


        /// <summary>
        /// Register a new web socket client
        /// </summary>
        /// <typeparam name="T">The web socket client type</typeparam>
        /// <param name="path">The path that the client should respond to</param>
        public void Register<T>(string path) where T : IWebSocket
        {
            if (!_types.ContainsKey(path))
            {
                _types[path] = typeof(T);
            }
            else
            {
                throw new Exception("Path: '"+path+"' has already been set");
            }
        }


        #region handlers

        private void HandleConnection(WebSocketConnection wsc)
        {
            var path = wsc.Handshake.ResourcePath;
            if (_types.ContainsKey(path))
            {
                //var type = typeof(WebSocket).MakeGenericType(_types[path]);
                var userSocket = (WebSocket)Activator.CreateInstance(_types[path]);
                userSocket.Connection = wsc;
                _userSockets.Add(wsc, userSocket);

                userSocket.Connected(wsc.Handshake);

                wsc.OnDisconnect += new DisconnectedEventHandler(HandleDisconnect);
                wsc.OnReceive += new ReceiveEventHandler(HandleReceive);
            }
        }

        void HandleReceive(WebSocketConnection wsc, string data)
        {
            if (_userSockets.ContainsKey(wsc))
            {
                _userSockets[wsc].Incoming(data);
            }
        }

        void HandleDisconnect(WebSocketConnection wsc)
        {
            if (_userSockets.ContainsKey(wsc))
            {
                _userSockets[wsc].Disconnected();
            }
        }

        #endregion


        /*
        public void Register(Type t, string path)
        {
            if (!types.ContainsKey(path))
            {
                types[path] = t;
            }
            else
            {
                throw new Exception("Path: '" + path + "' has already been set");
            }
        }
         */


        /*
        /// <summary>
        /// Instantiate a new client
        /// </summary>
        /// <param name="path">The path the client was registered at</param>
        /// <returns>The instantiated WebSocketClient</returns>
        public WebSocketConnection Create(string path)
        {
            if (types.ContainsKey(path))
            {
                try
                {
                    var ws = container.Resolve(types[path]);
                    var wsc = new WebSocketConnection(new WebSocketWrapper(ws));
                    var sws = (ASendingWebSocket)ws;
                    sws.Connection = wsc;

                    return wsc;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        */
    }
}
