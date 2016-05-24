using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.NetCore.Node.Common
{
    public class ExecuteGit
    {
        public class GitReturnStatus
        {
            public int ExitCode { get; set; }
            public string StdErr { get; set; }
            public string StdOut { get; set; }
        }

        public static GitReturnStatus Execute(string workingDir, string argument)
        {
            var proc = new Process();
            var ret = new GitReturnStatus();
            proc.StartInfo.WorkingDirectory = workingDir;
            proc.StartInfo.FileName = "git";
            proc.StartInfo.Arguments = argument;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            proc.Start();
            ret.StdOut = proc.StandardOutput.ReadToEnd();
            ret.StdErr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            ret.ExitCode = proc.ExitCode;
            return ret;
        }
    }
}
