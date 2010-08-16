using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nugget
{
    interface IReceivingWebSocket<T>
    {
        void Incomming(T data);
    }
}
