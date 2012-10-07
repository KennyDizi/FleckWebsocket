using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nugget.Server
{
    public class DataReceivedEventArgs : EventArgs
    {
        public IEnumerable<DataFragment> Fragments { get; set; }

        public DataReceivedEventArgs(IEnumerable<DataFragment> fragments)
        {
            Fragments = fragments;
        }

    }
}
