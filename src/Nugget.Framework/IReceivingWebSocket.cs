using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget.Framework
{
    public interface IReceivingWebSocket<T>
    {
        void Incoming(T data);
    }
}
