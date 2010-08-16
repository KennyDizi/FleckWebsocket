using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Nugget
{
    class WebSocketWrapper
    {
        private object webSocket;
        MethodInfo[] incomming;
        MethodInfo connected;
        MethodInfo disconnected;
        Type[] modelTypes;

        public WebSocketWrapper(object ws)
        {
            webSocket = ws;
            var interfaces = webSocket.GetType().GetInterfaces().Where(x => x.Name == "IWebSocket`1").ToArray();
            modelTypes = new Type[interfaces.Length];
            incomming = new MethodInfo[modelTypes.Length];

            
            var inc = webSocket.GetType().GetMethods().Where(x => x.Name == "Incomming").ToList();
            for (int i = 0; i < modelTypes.Length; i++)
            {
                modelTypes[i] = interfaces[i].GetGenericArguments().First();

                for (int j = 0; j < modelTypes.Length; j++)
                {
                    if (modelTypes[i] == inc[j].GetParameters().FirstOrDefault().ParameterType)
                    {
                        incomming[i] = inc[j];
                        break;
                    }
                }
            }

            
            connected = webSocket.GetType().GetMethods().SingleOrDefault(x => x.Name == "Connected");
            disconnected = webSocket.GetType().GetMethods().SingleOrDefault(x => x.Name == "Disconnected");

            
                
                //("IWebSocket`1").GetGenericArguments()[0];

        }

        public void Incomming(object model)
        {
            for(int i = 0; i < incomming.Length; i++)
            {
                if (model.GetType() == modelTypes[i])
                {
                    incomming[i].Invoke(webSocket, new object[] { model });
                }
                else
                {
                    //
                }
            }

        }

        public void Connected(ClientHandshake handshake)
        {
            connected.Invoke(webSocket, new object[] { handshake });
        }

        public void Disconnected()
        {
            disconnected.Invoke(webSocket, null);
        }
    }
}
