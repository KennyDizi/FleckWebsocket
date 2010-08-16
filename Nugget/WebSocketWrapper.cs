using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Nugget
{
    public class WebSocketWrapper
    {
        private object webSocket;
        MethodInfo[] incomming;
        Type[] modelTypes;

        public WebSocketWrapper(object ws)
        {
            webSocket = ws;
            var interfaces = webSocket.GetType().GetInterfaces().Where(x => x.Name == "IReceivingWebSocket`1").ToArray();
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
            
        }

        public void Incomming(object model)
        {
            bool methodFound = false;
            for(int i = 0; i < incomming.Length; i++)
            {
                if (model.GetType() == modelTypes[i])
                {
                    incomming[i].Invoke(webSocket, new object[] { model });
                    methodFound = true;
                    break; // only one method will fit the model
                }
            }

            if (!methodFound)
            {
                var socketName = webSocket.GetType().Name;
                var modelName = model.GetType().Name;
                Log.Warn(socketName + " can't handle model of type: " + modelName);
            }

        }

        public void Connected(ClientHandshake handshake)
        {
            ((IWebSocket)webSocket).Connected(handshake);
        }

        public void Disconnected()
        {
            ((IWebSocket)webSocket).Disconnected();
        }
    }
}
