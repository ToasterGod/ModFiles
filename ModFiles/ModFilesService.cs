using System.Collections.Generic;
using System.IO;

namespace ModFiles
{
    public class ModFilesService : IModFilesService
    {

        public ModFilesService()
        {

        }

        public IEnumerable<string> GetMods(string modFileSource)
        {
            //return array of folders in ModRootFolder
            return Directory.EnumerateDirectories(modFileSource, "*.*", SearchOption.TopDirectoryOnly);
        }

        public void CleanTarget(string deletionTarget)
        {
            if (Directory.Exists(deletionTarget))
            {
                Directory.Delete(deletionTarget, true);
            }
        }

        public void DirectoryCopy(string modFilesSource, string modFilesTarget, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(modFilesSource);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + modFilesSource);
            }


            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(modFilesTarget);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(modFilesTarget, file.Name);
                try
                {
                    file.CopyTo(temppath, false);

                }
                catch (System.IO.IOException ex)
                {
                    System.Console.WriteLine(ex);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(modFilesTarget, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }

        }

    }
}
