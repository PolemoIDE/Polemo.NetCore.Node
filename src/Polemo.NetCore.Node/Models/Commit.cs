using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Polemo.NetCore.Node.Models
{
    public class Commit
    {
        [MaxLength(40)]
        public string Hash { get; set; }
        [MaxLength(64)]
        public string Author { get; set; }
        [MaxLength(64)]
        public string Email { get; set; }
        public long Datetime { get; set; }
        public long Additions { get; set; }
        public long Deletions { get; set; }
        public long FilesChange { get; set; } 
    }
}