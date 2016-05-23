using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pomelo.NetCore.Node.Models
{
    public class Commit
    {
        public string Hash { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public long Datetime { get; set; }
        public long Additions { get; set; }
        public long Deletions { get; set; }
        public long FilesChange { get; set; } 
        public string Summary { get; set; }
    }
}