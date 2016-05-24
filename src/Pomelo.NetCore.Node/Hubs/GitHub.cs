using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pomelo.NetCore.Node.Models;
using Pomelo.NetCore.Node.Common;

namespace Pomelo.NetCore.Node.Hubs
{
    public partial class PomeloHub
    {
        private static List<Commit> Parse(string src)
        {

            var ret = new List<Commit>();
            src = src.Replace("\r\n", "\n");
            var logs = src.Split(new string[] { "--split--" }, StringSplitOptions.None);

            for (var j = 1; j < logs.Count(); j++)
            {
                var commit = new Commit
                {
                    Additions = 0,
                    Deletions = 0,
                    FilesChange = 0,
                };

                var tmp = logs[j].Split('\n');
                for (var i = 0; i < tmp.Count(); i++)
                {

                    if (string.IsNullOrWhiteSpace(tmp[i]))
                        continue;
                    var splited = tmp[i].Split(new string[] { "-::-" }, StringSplitOptions.None);
                    if (splited.Count() == 5)
                    {
                        commit.Hash = splited[0];
                        commit.Author = splited[1];
                        commit.Email = splited[2];
                        commit.Datetime = Convert.ToInt64(splited[3]);
                        commit.Summary = splited[4];
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public object GetGitLogs(string projectName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "--no-pager log -20 --numstat --format=--split--%n%H-::-%an-::-%ae-::-%at-::-%s";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                return new { isSucceeded = true, logs = Parse(output) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public class FileDiff
        {
            public string NewFilename { get; set; }
            public string OldFilename { get; set; }
            public string Diff { get; set; }
            public string Type { get; set; }
        }

        public object GetGitDiff(string projectName, string commit)
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
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error };

                var diff = Diff2html.GetDiff(output);
                var html = Diff2html.DiffToHTML(diff);
                var list = new List<FileDiff>();
                foreach (var file in html)
                {
                    list.Add(new FileDiff
                    {
                        OldFilename = file.Item1,
                        NewFilename = file.Item2,
                        Diff = file.Item3,
                        Type = file.Item4
                    });
                }

                return new { isSucceeded = true, msg = list };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public object GetUncommitDiff(string projectName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "--no-pager diff --no-color --cached";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error };

                var diff = Diff2html.GetDiff(output);
                var html = Diff2html.DiffToHTML(diff);
                var list = new List<FileDiff>();
                foreach (var file in html)
                {
                    list.Add(new FileDiff
                    {
                        OldFilename = file.Item1,
                        NewFilename = file.Item2,
                        Diff = file.Item3,
                        Type = file.Item4
                    });
                }

                return new { isSucceeded = true, msg = list };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }

        public object GetGitBranches(string projectName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "branch -a";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                output = output.Replace("\r\n", "\n");
                var _branches = output.Split('\n');
                var branches = new List<string>();
                var nowBranch = "";
                for (var i = 0; i < _branches.Count() - 1; i++)
                {
                    if (_branches[i][0] == '*')
                        nowBranch = _branches[i].Substring(1, _branches[i].Length - 1);
                    branches.Add(_branches[i].Substring(1, _branches[i].Length - 1));

                }
                return new { isSucceeded = true, branches = branches, nowBranch = nowBranch };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }
        public object CreateGitBranches(string projectName, string branchName, string baseBranchName = "")
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "checkout -b" + branchName + baseBranchName;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error };
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }
        public object SwitchGitBranches(string projectName, string branchName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "checkout " + branchName;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error };
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }
        public object DeleteGitBranches(string projectName, string branchName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "branch -D " + branchName;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error };
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }

        public object CreateGitCommit(string projectName, string title, string description)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                proc.StartInfo.Arguments = "commit -a -m \"" + title + "\" -m \"" + description + "\"";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = output + error };
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }

        public object CreateGitPush(string projectName/*, string repository, string refspec*/)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                // proc.StartInfo.Arguments = "push" + " " + repository + " " + refspec ?? "";
                proc.StartInfo.Arguments = "push" ;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = output + error };
                return new { isSucceeded = true, msg = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }
        public object CreateGitPull(string projectName/*, string repository, string refspec*/)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName);
                proc.StartInfo.FileName = "git";
                // proc.StartInfo.Arguments = "pull" + " " + repository + " " + refspec;
                proc.StartInfo.Arguments = "pull";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                proc.Start();
                proc.WaitForExit();
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = output + error };
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
