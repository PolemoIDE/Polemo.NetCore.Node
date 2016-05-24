using System.Diagnostics;

namespace Pomelo.NetCore.Node.Common
{
    public class ProcessHelper
    {
        public static Process Run(string workingDirectory, string fullFileName, string args)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = workingDirectory,
                    FileName = fullFileName,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            return proc;
        }
    }
}
