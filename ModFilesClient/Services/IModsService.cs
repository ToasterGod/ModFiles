using System.Collections.Generic;

namespace ModFilesClient.Services
{
    public interface IModsService
    {
        IEnumerable<string> GetModsFolders(string rootFolder);
    }
}
