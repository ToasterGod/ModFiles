using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModFilesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Execute();
        }
        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<IModFilesService, ModFilesService>();
                    services.AddScoped<Executor>();
                    services.AddScoped<ValidateParams>();
                });
        }
    }

    public static class HostExtension
    {
        public static void Execute(this IHost host)
        {
            host.Services.GetService<Executor>().Execute();
        }
    }
    class Executor
    {
        private readonly IModFilesService modFilesService;
        private FolderParams folderParam;
        public Executor(IModFilesService modFilesService, IConfiguration configuration)
        {
            folderParam = new FolderParams();
            this.modFilesService = modFilesService;
            folderParam.SourceRoot = configuration.GetValue<string>("modFilesSource");
            folderParam.TargetRoot = configuration.GetValue<string>("modFilesTarget");
            folderParam.ModFolderName = configuration.GetValue<string>("modFolderName");
        }

        public void Execute()
        {
            var avilableMods = modFilesService.GetMods(modFilesSource);
            ShowMods(avilableMods);
            int[] selectedMods = SelectMods();

            modFilesService.CleanTarget(Path.Combine(modFilesTarget, modFolderName));
            CopyMods(avilableMods, selectedMods);
        }

        private void CopyMods(IEnumerable<string> mods, int[] selectedMods)
        {
            foreach (var mod in selectedMods)
            {
                modFilesService.DirectoryCopy(mods.ToArray()[mod - 1], modFilesTarget, true);
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
