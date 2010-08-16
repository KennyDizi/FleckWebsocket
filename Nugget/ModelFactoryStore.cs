using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace Nugget
{
    class ModelFactoryStore
    {
        private UnityContainer container = new UnityContainer();
        private Dictionary<string, object> instances = new Dictionary<string, object>();
        private Dictionary<string, Type> modelTypes = new Dictionary<string, Type>();

        public void Register<TModel>(ISubProtocolModelFactory<TModel> factory, string subprotocol)
        {
            //container.RegisterInstance<ISubProtocolModelFactory<TModel>>(factory);
            instances.Add(subprotocol, factory);
            modelTypes.Add(subprotocol, typeof(TModel));
        }

        public object Get(string subprotocol)
        {
            return instances[subprotocol];
        }

        public ISubProtocolModelFactory<TModel> Get<TModel>(string subprotocol)
        {
            return (ISubProtocolModelFactory<TModel>)instances[subprotocol];
        }

        public Type GetModelType(string subprotocol)
        {
            return modelTypes[subprotocol];
        }

    }
}
