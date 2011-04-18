using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nugget.Server;

namespace Nugget.Framework
{
    public interface ISubProtocolModelFactory<TModel>
    {
        TModel Create(string data, WebSocketConnection connection);
        bool IsValid(TModel model);
    }
}
