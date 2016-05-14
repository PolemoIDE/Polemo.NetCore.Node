using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polemo.NetCore.Node.Models;

namespace Polemo.NetCore.Node.Hubs
{
    public partial class PolemoHub
    { 
        private static List<Commit> Parse(string src)
        {

            var ret = new List<Commit>();
            src = src.Replace("\r\n", "\n");
            var logs = src.Split(new string[] { "--split--" }, StringSplitOptions.None);

            for (var j =1; j < logs.Count(); j++)
            {
                var commit = new Commit{
                    Additions = 0,
                    Deletions = 0,
                    FilesChange = 0,
                 };

                var tmp = logs[j].Split('\n');                
                for (var i =0; i < tmp.Count(); i++){

                    if (string.IsNullOrWhiteSpace(tmp[i]))
                        continue;
                    var splited = tmp[i].Split(new string[] { "-::-" }, StringSplitOptions.None);
                    if (splited.Count() == 4)
                    {
                        commit.Hash = splited[0];
                        commit.Author = splited[1];
                        commit.Email = splited[2];
                        commit.Datetime = Convert.ToInt64(splited[3]);
                    }
                    else
                    {
                        splited = tmp[i].Split('\t');
                        commit.Additions += Convert.ToInt64(splited[0] == "-" ? "0" : splited[0]);
                        commit.Deletions += Convert.ToInt64(splited[1] == "-" ? "0" : splited[1]);
                        commit.FilesChange += 1;
                    }
                }
                ret.Add(commit);
            }
             
            
            return ret;
        }
        
        public async Task<object> GetGitLogs(string projectName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "--no-pager log -10 --numstat --format=--split--%n%H-::-%an-::-%ae-::-%at";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                return new { isSucceeded = true, msg = Parse(output) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }
        
        public async Task<object> GetDiff(string projectName, string commit)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "--no-pager show --pretty=\"\" " + commit;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                while (!proc.WaitForExit(500));
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error};
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }
        
        public async Task<object> GetUncommitDiff(string projectName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "--no-pager show --pretty=\"\" ";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                while (!proc.WaitForExit(500));
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error};
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
            
        }

    }
}
