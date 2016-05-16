using System.Diagnostics;

namespace Pomelo.NetCore.Node.Common
{
    public class ProcessHelper
    {
        public static Process Run(string workingDirectory, string fullFileName, string args)
        {
            var proc = new Process();
            proc.StartInfo.WorkingDirectory = workingDirectory;
            proc.StartInfo.FileName = fullFileName;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            return proc;
        }
    }
}
