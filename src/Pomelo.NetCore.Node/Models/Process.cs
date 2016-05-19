using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Pomelo.NetCore.Node.Models
{
    public class Process : System.Diagnostics.Process
    {
        public ulong InputSequence { get; set; } = 0;
        public ulong OutputSequence { get; set; } = 0;
    }
}
