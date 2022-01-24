using ModFilesClient.Models;

using System.Collections.Generic;

namespace ModFilesClient.Services
{
    public interface IModsService
    {
        IEnumerable<Mod> GetModsFolders(string rootFolder);
    }
}