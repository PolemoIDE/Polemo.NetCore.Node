using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Polemo.NetCore.Node.Hubs
{
    public partial class PolemoHub
    {
        public object StartOminisharp(string projectName)
        {
            try
            {
                var solutionPath = Path.Combine(Config.RootPath, projectName);
                var result = OmniSharp.CreateOmnisharpServerSubprocess(solutionPath);
                return new {isSucceeded = true, msg = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public async Task<object> AutoComplete(string projectName, string fileRelativePath, int rowNum, int colNum, string wordToComplete)
        {
            try
            {
                var solutionPath = Path.Combine(Config.RootPath, projectName);
                var para = new Dictionary<string, string>
                {
                    {"line", rowNum.ToString()},
                    {"column", colNum.ToString()},
                    {"filename", fileRelativePath},
                    {"wordToComplete", wordToComplete},
                    {"WantSnippet", "True"},
                    {"WantMethodHeader", "True"},
                    {"WantReturnType", "True"},
                };
                string result = await OmniSharp.GetResponse("/autocomplete", solutionPath, para);
                return new {isSucceeded = true, msg = result};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
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
