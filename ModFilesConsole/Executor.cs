using ModFiles;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModFilesConsole
{
    internal class Executor
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
            Console.WriteLine($"{folderParam}");

            //TODO! Change this block to be more efficient
            //If anyone is changed set isChanged

            //add the folderparam == null check in execute
            if (folderParam == null)
            {
                Console.WriteLine("The required files are empty, please enter the needed directories...");
                folderParam = UpdateValues(0, new FolderParams());
                isChanged = true;
            }
            else
            {
                int chosenOption = 0;
                do
                {
                    //TODO! if a single one is empty go here
                    //while loop to ask the question again when parse fails?
                    Console.WriteLine(
                        "which would you like to change?\n" +
                        "0: all three options\n" +
                        $"1: the root directory where the original modfiles are stored (currently in {folderParam.SourceRoot})\n" +
                        $"2: the target directory where the modfiles are copied into the game (currently in {folderParam.TargetRoot})\n" +
                        $"3: the name of the target mod folder where the files are copied into (currently named {folderParam.ModFolderName})\n" +
                        "9: quit");
                    string input = Console.ReadLine();
                    string msg = string.Empty;
                    if (Int32.TryParse(input, out chosenOption))
                    {
                        if (chosenOption > -1 && chosenOption < 4)
                        {
                            folderParam = UpdateValues(chosenOption, folderParam);
                        }
                        else
                        {
                            msg = "not a valid selection";
                        }
                    }
                    else
                    {
                        msg = "not a valid number";
                    }
                    Console.WriteLine(msg);
                } while (chosenOption != 9);
            }

            //Keep as it is! Update as we do already
            isChanged = !folderParam.Equals(options.Value);//The values differs
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

        private FolderParams UpdateValues(int choice, FolderParams folderParam)
        {
            FolderParams result = folderParam;
            string response = string.Empty;
            switch (choice)
            {
                case 0:
                    for (int i = 1; i < 4; i++)
                    {
                        result = UpdateValues(i, result);
                    }
                    break;

                case 1:
                    result.SourceRoot = UpdateValue(result.SourceRoot);
                    break;

                case 2:
                    result.TargetRoot = UpdateValue(result.TargetRoot);
                    break;

                case 3:
                    result.ModFolderName = UpdateValue(result.ModFolderName);
                    break;

                default:
                    break;
            }
            return result;
        }

        private static string UpdateValue(string current)
        {
            Console.WriteLine($"current value is \"{current}\", enter a new line to change it or press enter to keep current");
            string response = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(response))
            {
                return response;
            }

            return current;
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