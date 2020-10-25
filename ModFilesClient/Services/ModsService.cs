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

        public IEnumerable<string> GetModsFolders(string rootFolder)
        {
            return Directory.EnumerateDirectories(rootFolder).Select(s => Path.GetFileName(s));
        }
    }
}