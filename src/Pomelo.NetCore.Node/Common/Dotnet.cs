using System;
using System.Collections.Generic;
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

        public static CommandInfos GetCommands(string projectPath)
        {
            var commandInfos = new CommandInfos();
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
                var commandArray = SearchCommand(root);
                if (commandArray == null)
                    return null;
                command.CommandArray = commandArray;
                commandInfos.Commands.Add(command);
            }
            return commandInfos;
        }

        private static IEnumerable<string> SearchCommand(JObject root)
        {
            List<string> commands = new List<string>();
            var commandsProperty = root.Property("commands");
            if (commandsProperty == null)
                return null;

            foreach (var jToken in commandsProperty.Values())
            {
                var property = (JProperty) jToken;
                commands.Add(property.Name);
            }
            return commands;
        }
    }
}
