﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pomelo.NetCore.Node.Models;
using Pomelo.NetCore.Node.Common;

namespace Pomelo.NetCore.Node.Hubs
{
    public partial class PomeloHub
    {
        public static List<Process> ProcessPool = new List<Process>();

        public object GetProjectInfo(string projectName)
        {
            try
            {
                string path = Path.Combine(Config.RootPath, projectName);
                if (!Directory.Exists(path))
                    return new {isSucceeded = false, msg = $"项目\"{projectName}\"不存在"};

                var info = Dotnet.GetProjectInfo(path);
                return new { isSucceeded = true, projects = info };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public object RunBash()
        {
            var proc = new Process();
            try
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = "bash";
                proc.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    Clients.Group("process-" + ((Process)sender).Id).OnOutputDataReceived(proc.OutputSequence++, e.Data);
                };
                proc.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    Clients.Group("process-" + ((Process)sender).Id).OnOutputDataReceived(proc.OutputSequence++, e.Data);
                };
                proc.Start();
            }
            catch (Exception ex)
            {
                return new { isSucceeded = false, msg = ex.ToString() };
            }

            ProcessPool.Add(proc);

            // 将Caller加入process-id广播组
            Groups.Add(Context.ConnectionId, "process-" + proc.Id);

            return new { isSucceeded = true, pid = proc.Id };
        }

        public object RunCommand(string projectName, string args, string projectPath, bool useBash = false)
        {
            var proc = new Process();
            try
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WorkingDirectory = Path.Combine(Config.RootPath, projectName, projectPath);
                proc.StartInfo.FileName = "dotnet";
                proc.StartInfo.Arguments = "run " + args;
                proc.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    Clients.Group("process-" + ((Process)sender).Id).OnOutputDataReceived(proc.OutputSequence++, e.Data);
                };
                proc.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    Clients.Group("process-" + ((Process)sender).Id).OnOutputDataReceived(proc.OutputSequence++, e.Data);
                };
                proc.Start();
            }
            catch (Exception ex)
            {
                return new { isSucceeded = false, msg = ex.ToString() };
            }

            ProcessPool.Add(proc);

            // 将Caller加入process-id广播组
            Groups.Add(Context.ConnectionId, "process-" + proc.Id);

            return new { isSucceeded = true, pid = proc.Id };
        }

        public object ConsoleWrite(int pid, ulong sequence, char inputChar)
        {
            var proc = ProcessPool.SingleOrDefault(x => x.Id == pid);

            if (proc == null)
                return new { isSucceeded = false, msg = "Proccess not found." };

            if (proc.HasExited)
                return new { isSucceeded = false, msg = "Proccess has exited." };

            while (sequence != proc.InputSequence + 1)
                Thread.Sleep(100);

            proc.InputSequence = sequence;
            proc.StandardInput.Write(inputChar);

            return new { isSucceeded = true, @char = inputChar, sequence = sequence };
        }

        public Task<object> OpenProject(string projectName, string gitUrl, string SSHKey, string gitUserNickName, string gitUserEmail)
        {
            throw new NotImplementedException();
        }

        public async Task<object> ReadFile(string projectName, string fileRelativePath)
        {
            try
            {
                string path = Path.Combine(Config.RootPath, projectName, fileRelativePath);
                if (File.Exists(path))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            var text = await reader.ReadToEndAsync();
                            return new {isSucceeded = true, msg = text};
                        }
                    }
                }
                else
                {
                    return new {isSucceeded = false, msg = "文件不存在"};
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new {isSucceeded = false, msg = ex.Message};
            }
        }

        public async Task<object> WriteFile(string projectName, string fileRelativePath, string fileContent)
        {
            try
            {
                bool isNew = true, hasRestore = false;
                string path = Path.Combine(Config.RootPath, projectName, fileRelativePath);
                var file = new FileInfo(path);
                if (file.Exists)
                    isNew = false;
                
                if (file.Name.Equals("project.json"))
                    hasRestore = true;

                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        await writer.WriteAsync(fileContent);
                        bool isRestored = true;
                        if (hasRestore)
                        {
                            string projectPath = Path.Combine(Config.RootPath, projectName);
                            isRestored = Dotnet.Restore(projectPath);
                        }
                        return new { isSucceeded = true, isNew = isNew, hasRestore = hasRestore, isRestored = isRestored};
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public object RemoveFile(string projectName, string fileRelativePath)
        {
            try
            {
                string path = Path.Combine(Config.RootPath, projectName, fileRelativePath);
                var file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                    return new {isSucceeded = true, msg = "删除成功"};
                }
                return new {isSucceeded = true, msg = "文件不存在"};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new {isSucceeded = false, msg = ex.Message};
            }
        }

        public object CreateFolder(string projectName, string baseDirectory, string directoryName)
        {
            try
            {
                string path = Path.Combine(Config.RootPath, projectName, baseDirectory, directoryName);
                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                {
                    directory.Create();
                    return new { isSucceeded = true, msg = "创建成功" };
                }
                return new { isSucceeded = true, msg = "文件夹已存在" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public object RemoveFolder(string projectName, string directoryRelativePath)
        {
            try
            {
                string path = Path.Combine(Config.RootPath, projectName, directoryRelativePath);
                var directory = new DirectoryInfo(path);
                if (directory.Exists)
                {
                    directory.Delete(true);
                    return new { isSucceeded = true, msg = "删除成功" };
                }
                return new { isSucceeded = true, msg = "文件夹不存在" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public object RenameFile(string projectName, string fileDirectory, string oldFileName, string newFileName)
        {
            try
            {
                string oldFilePath = Path.Combine(Config.RootPath, projectName, fileDirectory, oldFileName);
                string newFilePath = Path.Combine(Config.RootPath, projectName, fileDirectory, newFileName);
                if(!File.Exists(oldFilePath))
                    return new { isSucceeded = false, msg = "源文件不存在" };
                if (File.Exists(newFilePath))
                    return new {isSucceeded = false, msg = "目标文件已存在"};
                File.Move(oldFilePath, newFilePath);
                return new { isSucceeded = true, msg = "重命名成功" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public object RenameFolder(string projectName, string baseDirectory, string oldDirectoryName, string newDirectoryName)
        {
            try
            {
                string oldPath = Path.Combine(Config.RootPath, projectName, baseDirectory, oldDirectoryName);
                string newPath = Path.Combine(Config.RootPath, projectName, baseDirectory, newDirectoryName);
                if (!Directory.Exists(oldPath))
                    return new { isSucceeded = false, msg = "源文件夹不存在" };
                if (Directory.Exists(newPath))
                    return new { isSucceeded = false, msg = "目标文件夹已存在" };
                Directory.Move(oldPath, newPath);
                return new { isSucceeded = true, msg = "重命名成功" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public static void CheckFile(FileInfo file, string projectName)
        {
            if (file.FullName.Contains("$safeprojectname$"))
            {

                var newFileName = file.FullName.Replace("$safeprojectname$", projectName);
                Console.WriteLine("     C: " + newFileName + " <-" + file.FullName);
                File.Move(file.FullName, newFileName);
                file = new FileInfo(newFileName);

            }
            else
                Console.WriteLine("     F: " + file.FullName);
            
            var content = File.ReadAllText(file.FullName);

            if (content.Contains("$safeprojectname$"))
                File.WriteAllText(file.FullName, content.Replace("$safeprojectname$", projectName));
        }
        public static void ListFiles(FileSystemInfo info, string projectName)
        {
            DirectoryInfo dir = info as DirectoryInfo;
            if (dir.FullName.Contains("$safeprojectname$"))
            {
                var newDirctoryName = dir.FullName.Replace("$safeprojectname$", projectName);
                Directory.Move(dir.FullName, newDirctoryName);
                dir = new DirectoryInfo(newDirctoryName);
                Console.WriteLine("C: " + newDirctoryName);
            }
            else
            {
                Console.WriteLine("D: " + dir.FullName.Replace("/Users/wph95/Hackathon/2016/azura/test/test/", ""));
            }
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {

                FileInfo file = files[i] as FileInfo;
                if (file == null)
                    ListFiles(files[i], projectName);
                else
                    CheckFile(file, projectName);

            }
        }
    }
}