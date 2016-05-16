using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pomelo.NetCore.Node.Models
{
    public class ProjectInfo
    {
        [JsonProperty("Project")]
        public List<Command> Commands { get; set; }

        public ProjectInfo()
        {
            Commands = new List<Command>();
        }
    }

    public class Command
    {
        public string Title { get; set; }

        public string Path { get; set; }
    }
}
