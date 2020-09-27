using ModFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModFilesConsole
{
    class Executor
    {
        private readonly IModFilesService modFilesService;
        private FolderParams folderParam;
        private readonly IWritableOptions<FolderParams> options;
        public Executor(IModFilesService modFilesService, IWritableOptions<FolderParams> options)
        {
            this.modFilesService = modFilesService;
            this.options = options;
            folderParam = options.Value;
        }


        public void Execute()
        {
            folderParam = GetFolders(folderParam);
            var avilableMods = modFilesService.GetMods(folderParam.SourceRoot);
            ShowMods(avilableMods);
            int[] selectedMods = SelectMods();

            modFilesService.CleanTarget(Path.Combine(folderParam.TargetRoot, folderParam.ModFolderName));
            CopyMods(avilableMods, selectedMods);
        }

        private FolderParams GetFolders(FolderParams folderParam)
        {
            bool isChanged = false;
            Console.WriteLine($"current sourceRoot = {folderParam.SourceRoot}, enter new value or empty to keep existing.");
            var newRoot = Console.ReadLine();
            if (!string.IsNullOrEmpty(newRoot))
            {
                folderParam.SourceRoot = newRoot;
                isChanged = true;
            }
            Console.WriteLine($"current targetRoot = {folderParam.TargetRoot}, enter new value or empty to keep existing.");
            var newTarget = Console.ReadLine();
            if (!string.IsNullOrEmpty(newTarget))
            {
                isChanged = true;
                folderParam.TargetRoot = newTarget;
            }
            Console.WriteLine($"current modFolderName = {folderParam.ModFolderName}, enter new value or empty to keep existing.");
            var newModFolder = Console.ReadLine();
            if (!string.IsNullOrEmpty(newModFolder))
            {
                isChanged = true;
                folderParam.ModFolderName = newModFolder;
            }
            if (isChanged)
            {
                options.Update(newFolderParam =>
                    {
                        newFolderParam.SourceRoot = folderParam.SourceRoot;
                        newFolderParam.TargetRoot = folderParam.TargetRoot;
                        newFolderParam.ModFolderName = folderParam.ModFolderName;
                    }); 
            }
            return folderParam;
        }


        private void CopyMods(IEnumerable<string> mods, int[] selectedMods)
        {
            foreach (var mod in selectedMods)
            {
                modFilesService.DirectoryCopy(mods.ToArray()[mod - 1], folderParam.TargetRoot, true);
            }
        }

        private static int[] SelectMods()
        {
            Console.WriteLine($"which mods do you wish to use?{Environment.NewLine}Separate the mods with a single space.");
            var selectedMods = Console.ReadLine();
            int[] selection = Array.ConvertAll(selectedMods.Split(' '), s => int.Parse(s));
            return selection;
        }

        private static void ShowMods(IEnumerable<string> mods)
        {
            int index = 0;
            foreach (var mod in mods)
            {

                Console.WriteLine($"{++index}.  {Path.GetFileName(mod)}");
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
