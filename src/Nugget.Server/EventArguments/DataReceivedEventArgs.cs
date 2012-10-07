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

        [Obsolete("GetFragmentPayloadAsString is not just here for convenience while I get the server working for the new protocol", false)]
        public string GetFragmentPayloadAsString()
        {
            var sb = new StringBuilder();
            foreach (var f in Fragments)
            {
                if (f.Masked)
                    f.UnMaskPayload();
                sb.Append(Encoding.UTF8.GetString(f.GetPayload()));
            }
            return sb.ToString();
        }

    }
}
