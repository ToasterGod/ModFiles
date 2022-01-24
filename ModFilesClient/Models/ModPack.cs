using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ModFilesClient.Models
{
    public class ModPackList: ObservableCollection<ModPack>
    {

    }

    public class ModPack
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public List<Mod> Mods { get; set; } //the path to any mods in the modpack
    }

    public class Mod
    {
        public string ModName { get; set; }
        public IEnumerable<string> Files { get; set; }
    }
}
