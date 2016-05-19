using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pomelo.NetCore.Node.Models;

namespace Pomelo.NetCore.Node.Common
{
    public static class Dotnet
    {
        public static bool Restore(string projectPath)
        {
            throw new NotImplementedException();
        }

        public static List<ProjectInfo> GetProjectInfo(string projectPath)
        {
            var ret = new List<ProjectInfo>();
            foreach (var file in FileHelper.SearchAllFiles(projectPath, "project.json"))
            {
                var info = new ProjectInfo();
                var fileDirectoryName = FileHelper.GetFileDirectoryName(file);
                var filePath = FileHelper.GetFileRelativeDirectory(projectPath, file);
                info.Path = filePath;
                var content = File.ReadAllText(file);
                var root = JObject.Parse(content);
                var titleProp = root.Property("title");
                string title = titleProp != null ? titleProp.Value.ToString() : fileDirectoryName;
                info.Title = title;
                ret.Add(info);
            }
            return ret;
        }
    }
}
