using System.Collections.Generic;
using System.IO;

namespace ModFilesClient.Services
{
    public class ModsService : IModsService
    {
        public ModsService()
        {
        }

        public IEnumerable<string> GetModsFolders(string rootFolder)
        {
            return Directory.EnumerateDirectories(rootFolder);
        }
    }
}
