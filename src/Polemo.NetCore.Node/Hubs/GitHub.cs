using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polemo.NetCore.Node.Hubs
{
    public partial class PolemoHub
    {
        public Task<object> SwitchBranch(string projectName, string targetBranch)
        {
            throw new NotImplementedException();
        }

        public Task<object> RemoveBranch(string projectName, string targetBranch)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetGitLogs(string projectName)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetGitDiff(string projectName)
        {
            throw new NotImplementedException();
        }

        public Task<object> GitCommit(string projectName, string title, string description)
        {
            throw new NotImplementedException();
        }

        public Task<object> GitPull(string projectName)
        {
            throw new NotImplementedException();
        }
    }
}
