using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget
{
    class ModelFactoryWrapper
    {
        private object factory;
        public ModelFactoryWrapper(object modelFactory)
        {
            factory = modelFactory;
        }

        public object Create(string data)
        {
            var method = factory.GetType().GetMethods().SingleOrDefault(x => x.Name == "Create");
            var model = method.Invoke(factory, new object[] { data });
            return model;
        }
    }
}
