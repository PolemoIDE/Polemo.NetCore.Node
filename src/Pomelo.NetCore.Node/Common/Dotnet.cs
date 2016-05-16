using System;
using System.IO;
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

        public static ProjectInfo GetProjectInfo(string projectPath)
        {
            var commandInfos = new ProjectInfo();
            foreach (var file in FileHelper.SearchAllFiles(projectPath, "project.json"))
            {
                var command = new Command();
                var fileDirectoryName = FileHelper.GetFileDirectoryName(file);
                var filePath = FileHelper.GetFileRelativeDirectory(projectPath, file);
                command.Path = filePath;
                var content = File.ReadAllText(file);
                var root = JObject.Parse(content);
                var titleProp = root.Property("title");
                string title = titleProp != null ? titleProp.Value.ToString() : fileDirectoryName;
                command.Title = title;
                commandInfos.Commands.Add(command);
            }
            return commandInfos;
        }
    }
}
