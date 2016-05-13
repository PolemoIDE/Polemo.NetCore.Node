using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polemo.NetCore.Node.Hubs
{
    public partial class PolemoHub
    {
        public Task<object> AutoComplete(string projectName, string fileRelativePath, int rowNum, int colNum, string codeContent)
        {
            throw new NotImplementedException();
        }

        public Task<object> CodeCheck(string projectName, string fileRelativePath, string codeContent)
        {
            throw new NotImplementedException();
        }

        public Task<object> Highlight(string projectName, string fileRelativePath, string codeContent)
        {
            throw new NotImplementedException();
        }
    }
}
