using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pomelo.NetCore.Node.Hubs
{
    public partial class PomeloHub
    {
        public object StartOmnisharp(string projectName)
        {
            try
            {
                var solutionPath = Path.Combine(Config.RootPath, projectName);
                var result = OmniSharp.CreateOmnisharpServerSubprocess(solutionPath);
                return new { isSucceeded = true, msg = result ? "启动成功" : "启动失败" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public async Task<object> AutoComplete(string projectName, string fileRelativePath, int rowNum, int colNum, string wordToComplete, string codeContent)
        {
            try
            {
                var solutionPath = Path.Combine(Config.RootPath, projectName);
                var fileAbsPath = Path.Combine(solutionPath, fileRelativePath);
                var para = new Dictionary<string, string>
                {
                    {"line", rowNum.ToString()},
                    {"column", colNum.ToString()},
                    {"filename", fileAbsPath},
                    {"wordToComplete", wordToComplete},
                    {"buffer", codeContent},
                    {"WantSnippet", "True"},
                    {"WantMethodHeader", "True"},
                    {"WantReturnType", "True"}
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

        public async Task<object> CodeCheck(string projectName, string fileRelativePath, string codeContent)
        {
            try
            {
                var solutionPath = Path.Combine(Config.RootPath, projectName);
                var fileAbsPath = Path.Combine(solutionPath, fileRelativePath);
                var para = new Dictionary<string, string>
                {
                    {"filename", fileAbsPath},
                    {"buffer", codeContent}
                };
                string result = await OmniSharp.GetResponse("/codecheck", solutionPath, para);
                return new { isSucceeded = true, msg = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }

        public async Task<object> Highlight(string projectName, string fileRelativePath, string codeContent)
        {
            try
            {
                var solutionPath = Path.Combine(Config.RootPath, projectName);
                var fileAbsPath = Path.Combine(solutionPath, fileRelativePath);
                var para = new Dictionary<string, string>
                {
                    {"filename", fileAbsPath},
                    {"buffer", codeContent}
                };
                string result = await OmniSharp.GetResponse("/highlight", solutionPath, para);
                return new { isSucceeded = true, msg = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                return new { isSucceeded = false, msg = ex.Message };
            }
        }
    }
}
