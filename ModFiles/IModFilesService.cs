using System.Collections.Generic;

namespace ModFiles
{
    public interface IModFilesService
    {
        IEnumerable<string> GetMods(string modFileSource);
        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);
        void CleanTarget(string deletionTarget);
    }
}
