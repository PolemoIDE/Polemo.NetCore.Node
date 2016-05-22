using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
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

            Task.Factory.StartNew(() =>
            {
                proc.WaitForExit();
                ProcessPool.Remove(proc);

                // 推送进程退出消息
                Clients.Group("process-" + proc.Id).OnOutputDataReceived(proc.OutputSequence++, $"Process has exited with code {proc.ExitCode}.");
                Clients.Group("process-" + proc.Id).OnProcessExited(proc.ExitCode);

                proc.Dispose();
            });

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

            Task.Factory.StartNew(() =>
            {
                proc.WaitForExit();
                ProcessPool.Remove(proc);

                // 推送进程退出消息
                Clients.Group("process-" + proc.Id).OnOutputDataReceived(proc.OutputSequence++, $"Process has exited with code {proc.ExitCode}.");
                Clients.Group("process-" + proc.Id).OnProcessExited(proc.ExitCode);

                proc.Dispose();
            });
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
            
            lock (this)
            {
                proc.InputSequence = sequence;
                proc.StandardInput.Write(inputChar);
                return new { isSucceeded = true, @char = inputChar, sequence = sequence };
            }
        }
        public async Task<object> CreateProject(string projectName, string projectType)
        {
            string url = "";
            string dest = Path.Combine(Config.RootPath, projectName);
            switch (projectType)
            {
                case "ConsoleApp":
                    url = "https://github.com/PomeloIDE/Pomelo.NetCore.Template/archive/console-app.zip";
                    break;
                case "WebApplicationEmpty:":
                    url = "https://github.com/PomeloIDE/Pomelo.NetCore.Template/archive/web-application-empty.zip";
                    break;
                case "WebApplicationMvc":
                    url = "https://github.com/PomeloIDE/Pomelo.NetCore.Template/archive/web-application-mvc.zip";
                    break;
                default:
                    url = "";
                    break;
            }
            if (String.IsNullOrEmpty(url)){
                return new { isSucceeded = false, msg = "projectType Not Found"};
            }
            var tmpFile = Path.GetTempPath() + "codecomb_" + Guid.NewGuid().ToString() + ".zip";
            using (var webClient = new HttpClient() { Timeout = new TimeSpan(1, 0, 0), MaxResponseContentBufferSize = 1024 * 1024 * 50 })
            {
                var bytes = await webClient.GetByteArrayAsync(url);
                File.WriteAllBytes(tmpFile, bytes);
                Console.WriteLine("Downloaded");
            }
            using (var fileStream = new FileStream(tmpFile, FileMode.Open))
            using (var archive = new ZipArchive(fileStream))
            {
                foreach (var x in archive.Entries)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(dest + x.FullName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(dest + x.FullName));
                    if (x.Length == 0 && string.IsNullOrEmpty(Path.GetExtension(x.FullName)))
                        continue;
                    using (var entryStream = x.Open())
                    using (var destStream = File.OpenWrite(dest + x.FullName))
                    {
                        entryStream.CopyTo(destStream);
                    }
                }
            }
            File.Delete(tmpFile);
            ListFiles(new DirectoryInfo(dest), projectName);
            return new { isSucceeded = true, msg = "Success Create Project:" + projectName + " Type:" + projectType}; 
        }
        public object OpenProject(string projectName, string gitUrl, string gitUserNickName, string gitUserPassword, string gitUserEmail)
        {
            string path = Path.Combine(Config.RootPath, projectName);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists){
                if (gitUrl.Contains("https://"))
                    gitUrl = "https://"+gitUserNickName + ':' + gitUserPassword + '@' + gitUrl.Substring(8, gitUrl.Length - 8);
                else
                    gitUrl = "http://"+gitUserNickName + ':' + gitUserPassword + '@' + gitUrl.Substring(7, gitUrl.Length - 7);
                var proc = Process.Start("git --no-pager clone " + gitUrl+ " " + projectName);
                while (!proc.WaitForExit(500));
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();                
                if (proc.ExitCode != 0)
                    return new { isSucceeded = false, msg = error};
                Process.Start("git --no-pager config  --global --add user.name" + gitUserNickName);
                Process.Start("git --no-pager config  --global --add user.email" + gitUserEmail);
                return new { isSucceeded = true, msg = "Success Clone and open Project"};
            }
            else
                return new { isSucceeded = true, msg = "Success OpenProject"};
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

        public object WriteFile(string projectName, string fileRelativePath, string fileContent)
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

                File.WriteAllText(path, fileContent);
                bool isRestored = true;
                if (hasRestore)
                {
                    string projectPath = Path.Combine(Config.RootPath, projectName);
                    isRestored = Dotnet.Restore(projectPath);
                }
                return new { isSucceeded = true, isNew = isNew, hasRestore = hasRestore, isRestored = isRestored };
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
        
        public object ListFolder(string projectName, string baseDirectory)
        {
            try 
            {
                string path = Path.Combine(Config.RootPath, projectName, baseDirectory);
                return new { isSucceeded = true, msg = DirTree(new DirectoryInfo(path), path) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
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
        public object DirTree(FileSystemInfo info, string dirPath)
        {
            List<object> results = new List<object>(); ;
            DirectoryInfo dir = info as DirectoryInfo;
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {

                FileInfo file = files[i] as FileInfo;
                if (file == null)
                    results.Add(DirTree(files[i], dirPath));
                else
                {
                    if (CodeComb.Package.OS.Current == CodeComb.Package.OSType.Windows)
                        results.Add(new { name = file.Name, path = file.FullName.Replace(dirPath + "\\", "") });
                    else
                        results.Add(new { name = file.Name, path = file.FullName.Replace(dirPath + "/", "") });
                }
            }
            string path = dir.FullName;
            return new { files = results, dirName = new { display= dir.Name, full= dir.FullName } };
        }
    }
}
