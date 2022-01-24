using ModFilesClient.Models;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModFilesClient.Services
{
    public class ModsService : IModsService
    {
        public ModsService()
        {
        }

        public IEnumerable<Mod> GetModsFolders(string rootFolder)
        {
            List<Mod> d = new List<Mod>();
            foreach (string item in Directory.GetDirectories(rootFolder).Select(s => Path.GetFileName(s)))
            {
                d.Add(new Mod { ModName = item });
                //TODO add all the contents of the mod into each mod while making it
            }
            return d;
        }
    }
}