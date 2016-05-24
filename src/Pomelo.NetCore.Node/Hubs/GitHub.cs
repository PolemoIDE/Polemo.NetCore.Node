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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "--no-pager log -20 --numstat --format=--split--%n%H-::-%an-::-%ae-::-%at-::-%s";
                var result = ExecuteGit.Execute(workingDir, argument);
                return new { isSucceeded = true, logs = Parse(result.StdOut) };
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "--no-pager show --pretty=\"\" " + commit;
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdErr };

                var diff = Diff2html.GetDiff(result.StdOut);
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "--no-pager diff --no-color --cached";
                var result = ExecuteGit.Execute(workingDir, argument);

                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdErr };

                var diff = Diff2html.GetDiff(result.StdOut);
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "branch -a";
                var result = ExecuteGit.Execute(workingDir, argument);

                var output = result.StdOut.Replace("\r\n", "\n");
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "checkout -b" + branchName + baseBranchName;
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdErr };
                return new { isSucceeded = true, msg = result.StdOut };
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "checkout " + branchName;
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdErr };
                return new { isSucceeded = true, msg = result.StdOut };
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "branch -D " + branchName;
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdErr };
                return new { isSucceeded = true, msg = result.StdOut };
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                var argument = "commit -a -m \"" + title + "\" -m \"" + description + "\"";
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdOut + result.StdErr };
                return new { isSucceeded = true, msg = result.StdOut };
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                // proc.StartInfo.Arguments = "push" + " " + repository + " " + refspec ?? "";
                var argument = "push";
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdOut + result.StdErr };
                else
                    return new { isSucceeded = true, msg = result.StdOut };
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
                var workingDir = Path.Combine(Config.RootPath, projectName);
                // proc.StartInfo.Arguments = "pull" + " " + repository + " " + refspec;
                var argument = "pull";
                var result = ExecuteGit.Execute(workingDir, argument);
                if (result.ExitCode != 0)
                    return new { isSucceeded = false, msg = result.StdOut + result.StdErr };
                else
                    return new { isSucceeded = true, msg = result.StdOut };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }

        }



    }
}
