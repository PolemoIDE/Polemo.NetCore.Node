using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polemo.NetCore.Node.Models
{
    public class CommandInfos
    {
        [JsonProperty("ProjectCommands")]
        public List<Command> Commands { get; set; }

        public CommandInfos()
        {
            Commands = new List<Command>();
        }
    }

    public class Command
    {
        public string Title { get; set; }

        [JsonProperty("Commands")]
        public IEnumerable<string> CommandArray { get; set; }

        public string Path { get; set; }
    }
}
