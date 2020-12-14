using System;
using System.Collections.Generic;
using System.Text;

namespace ModFilesClient.Models
{
    public class ModPack
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<string> Mods { get; set; }
    }
}
