using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Pomelo.NetCore.Node.Models
{
    public class Process : System.Diagnostics.Process
    {
        public ulong Sequence { get; set; } = 0;
    }
}
