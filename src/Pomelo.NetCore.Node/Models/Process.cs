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

        public static Process Start(string file, string args, string path)
        {
            var proc = new Process();
            proc.StartInfo.FileName = file;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.WorkingDirectory = path;
            proc.Start();
            return proc;
        }
    }
}
