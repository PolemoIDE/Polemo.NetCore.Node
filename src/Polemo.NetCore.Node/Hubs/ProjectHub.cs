using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

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

        public Task<object> ReadFile(string projectName, string fileRelativePath)
        {
            throw new NotImplementedException();
        }

        public Task<object> WriteFile(string projectName, string fileRelativePath, string fileContent)
        {
            throw new NotImplementedException();
        }

        public Task<object> RemoveFile(string projectName, string fileRelativePath)
        {
            throw new NotImplementedException();
        }

        public Task<object> CreateFolder(string projectName, string baseDirectory, string directoryName)
        {
            throw new NotImplementedException();
        }

        public Task<object> RemoveFolder(string projectName, string directoryRelativePath)
        {
            throw new NotImplementedException();
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
