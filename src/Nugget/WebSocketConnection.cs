using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Microsoft.Practices.Unity;

namespace Nugget
{

    public class WebSocketConnection
    {
        public Socket Socket 
        {
            get
            {
                if (Sender.Socket != null)
                    return Sender.Socket;
                if (Receiver.Socket != null)
                    return Receiver.Socket;

                return null;
            }
            set
            {
                Sender.Socket = value;
                Receiver.Socket = value;
            }
        }
        
        
        public WebSocketWrapper WebSocket 
        { 
            get
            {
                if (Sender.WebSocket != null)
                    return Sender.WebSocket;
                if (Receiver.WebSocket != null)
                    return Receiver.WebSocket;

                return null;
            }
            set
            {
                Sender.WebSocket = value;
                Receiver.WebSocket = value;
            }
        }

        private Sender Sender { get; set; }
        private Receiver Receiver { get; set; }

        #region ctors

        public WebSocketConnection()
        {
            Sender = new Sender();
            Receiver = new Receiver();
            Sender.Connection = this;
            Receiver.Connection = this;
        }

        public WebSocketConnection(WebSocketWrapper websocket)
        {
            Sender = new Sender();
            Receiver = new Receiver();
            Sender.WebSocket = websocket;
            Receiver.WebSocket = websocket;
            Sender.Connection = this;
            Receiver.Connection = this;
        }

        public WebSocketConnection(Socket socket, WebSocketWrapper websocket)
        {
            Sender = new Sender(socket, websocket);
            Receiver = new Receiver(socket, websocket);
            Sender.Connection = this;
            Receiver.Connection = this;
        }

        public WebSocketConnection(Socket socket, WebSocketWrapper websocket, SubProtocolModelFactoryWrapper modelfactory)
        {
            Sender = new Sender(socket, websocket);
            Receiver = new Receiver(socket, websocket, modelfactory);
            Sender.Connection = this;
            Receiver.Connection = this;
        }

        #endregion

        public void SetModelFactory(object factory)
        {
            Receiver.Factory = new SubProtocolModelFactoryWrapper(factory);
        }

        public void Send(string data)
        {
            Sender.Send(data);
        }

        public void StartReceiving()
        {
            Receiver.Receive();
        }

    }

}
