using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Logging;
using Polemo.NetCore.Node.Common;

namespace Polemo.NetCore.Node.Hubs
{
    public partial class PolemoHub
    {
        public Task<object> GetCommands(string projectName)
        {
            throw new NotImplementedException();
        }

        public Task<object> RunCommand(string projectName, string cmd, string projectPath)
        {
            throw new NotImplementedException();
        }

        public Task<object> ConsoleWrite(string sessionId, int sequence, char inputChar)
        {
            throw new NotImplementedException();
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
                
                if (file.Name.Equals("project.json", StringComparison.OrdinalIgnoreCase))
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
                    directory.Delete();
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

        public Task<object> RenameFile(string projectName, string fileDirectory, string oldFileName, string newFileName)
        {
            throw new NotImplementedException();
        }

        public Task<object> RenameFolder(string projectName, string baseDirectory, string oldDirectoryName, string newDirectoryName)
        {
            throw new NotImplementedException();
        }
    }
}
