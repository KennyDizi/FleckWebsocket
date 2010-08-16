using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Nugget
{
    public class ModelFactoryWrapper
    {
        private object factory;
        public ModelFactoryWrapper(object modelFactory)
        {
            factory = modelFactory;
        }

        public object Create(string data, WebSocketConnection connection)
        {
            var method = factory.GetType().GetMethods().SingleOrDefault(x => x.Name == "Create");
            var model = method.Invoke(factory, new object[] { data, connection });
            return model;
        }
    }
}
