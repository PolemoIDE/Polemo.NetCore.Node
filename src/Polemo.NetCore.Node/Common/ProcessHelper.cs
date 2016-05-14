using System.Diagnostics;

namespace Polemo.NetCore.Node.Common
{
    public class ProcessHelper
    {
        public static string Run(string workingDirectory, string fullFileName, string args)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = workingDirectory,
                    FileName = fullFileName,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    StandardErrorEncoding = System.Text.Encoding.UTF8,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                }
            };
            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }
    }
}
