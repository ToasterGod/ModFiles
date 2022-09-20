using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Configuration;
using CommunityToolkit.Mvvm.Input;

using ModFilesClient.Helpers;
using ModFilesClient.Models;
using ModFilesClient.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModFilesClient.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IModsService modsService;
        private IConfiguration configuration;
        string sourceRoot;
        string targetRoot;
        string modpackRoot;

        #region mod variables
        private List<Mod> allMods;
        private List<Mod> allActiveMods;

        public bool ModVisibility
        {
            get => modVisibility;
            set => SetProperty(ref modVisibility, value);
        }
        private bool modVisibility;

        public ObservableCollection<Mod> Mods
        {
            get => mods;
            set => SetProperty(ref mods, value);
        }
        private ObservableCollection<Mod> mods;

        public Mod SelectedMod
        {
            get => selectedMod;
            set => SetProperty(ref selectedMod, value);
        }
        private Mod selectedMod;

        public string ModSearchText
        {
            get => modSearchText;
            set => SetProperty(ref modSearchText, value);
        }
        private string modSearchText;

        public ObservableCollection<Mod> ActiveMods
        {
            get => activeMods;
            set => SetProperty(ref activeMods, value);
        }
        private ObservableCollection<Mod> activeMods = new ObservableCollection<Mod>();

        public Mod SelectedActiveMod
        {
            get => selectedActiveMod;
            set => SetProperty(ref selectedActiveMod, value);
        }
        private Mod selectedActiveMod;

        public string ActiveModSearchText
        {
            get => activeModSearchText;
            set => SetProperty(ref activeModSearchText, value);
        }
        private string activeModSearchText;
        #endregion

        #region modpack variables
        private ModPackList allModPacks;

        public bool ModPackVisibility
        {
            get => modPackVisibility;
            set
            {
                SetProperty(ref modPackVisibility, value);
                ModVisibility = !value;
            }
        }
        private bool modPackVisibility;

        public bool PackSelectedVisibility
        {
            get => packSelectedVisibility;
            set => SetProperty(ref packSelectedVisibility, value);
        }
        private bool packSelectedVisibility;

        public ModPack SelectedModPack
        {
            get => selectedModPack;
            set
            {
                SetProperty(ref selectedModPack, value);
                if (value is not null)
                {
                    Mods = new ObservableCollection<Mod>(value.Mods);
                    PackSelectedVisibility = true;
                }
                else
                {
                    PackSelectedVisibility = false;
                }
            }
        }
        private ModPack selectedModPack;

        public IEnumerable<ModPack> ModPacks
        {
            get => modPacks;
            set => SetProperty(ref modPacks, value);
        }
        private IEnumerable<ModPack> modPacks;

        public string ModPackSearchText
        {
            get => modPackSearchText;
            set => SetProperty(ref modPackSearchText, value);
        }
        private string modPackSearchText;
        #endregion

        #region Commands
        public AsyncRelayCommand NewPackCommand { get; }
        public AsyncRelayCommand DeletePackCommand { get; }
        public AsyncRelayCommand EditPackCommand { get; }

        public AsyncRelayCommand UsePackCommand { get; }
        public AsyncRelayCommand SavePackCommand { get; }
        public AsyncRelayCommand AddModCommand { get; }
        public AsyncRelayCommand RemoveModCommand { get; }
        public AsyncRelayCommand UpdateModCommand { get; }
        public AsyncRelayCommand CancelSelectionCommand { get; }
        #endregion
        public MainViewModel(IConfiguration configuration, IModsService modsService)
        {
            sourceRoot = configuration.GetValue<string>("FolderSettings:RootFolder");
            targetRoot = configuration.GetValue<string>("FolderSettings:TargetFolder");
            modpackRoot = configuration.GetValue<string>("FolderSettings:ModPackList");

            NewPackCommand = new AsyncRelayCommand(async () => await NewPack());
            UsePackCommand = new AsyncRelayCommand(async () => await UsePackAsync());
            SavePackCommand = new AsyncRelayCommand(async () => await SavePackAsync());
            DeletePackCommand = new AsyncRelayCommand(async () => await DeletePackAsync());
            EditPackCommand = new AsyncRelayCommand(async () => await EditPack());

            AddModCommand = new AsyncRelayCommand(async () => await AddModsAsync());
            RemoveModCommand = new AsyncRelayCommand(async () => await RemoveModsAsync());
            UpdateModCommand = new AsyncRelayCommand(async () => await UpdateMods());
            CancelSelectionCommand = new AsyncRelayCommand(async () => await CancelSelectionAsync());

            allModPacks = new ModPackList();
            allActiveMods = new List<Mod>();
            this.modsService = modsService;
            this.configuration = configuration;
            ModPackVisibility = true;
            UpdateModCommand.Execute(null);
        }

        private ObservableCollection<Mod> ToObservableCollection(List<Mod> enumerable) => new ObservableCollection<Mod>(enumerable);

        #region ModPack methods
        private Task CancelSelectionAsync()
        {
            SelectedModPack = null;
            SelectedMod = null;
            ModPackVisibility = true;
            return Task.CompletedTask;
        }

        private Task UpdateModPacksAsync()
        {
            File.WriteAllText(modpackRoot, JsonSerializer.Serialize(allModPacks));
            SelectedMod = null;
            ModPackVisibility = true;
            return Task.CompletedTask;
        }

        private async Task DeletePackAsync()
        {
            allModPacks.Remove(selectedModPack);
            await UpdateModPacksAsync();
            FilterModPacks();
        }

        private Task UsePackAsync()
        {

            if (ActiveMods.Count == 0)
            {
                allActiveMods = selectedModPack.Mods;
            }
            ActivatePackAsync();
            FilterModPacks();
            FilterMods();

            return Task.CompletedTask;
        }

        private Task ActivatePackAsync()
        {
            List<string> allActiveModPaths = new List<string>(Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories));
            List<string> pathsToActivate = new List<string>();

            List<FileInfo> filesToActivate = new List<FileInfo>();
            List<FileInfo> activeFiles = new List<FileInfo>();

            allActiveMods.ForEach(mod =>
            pathsToActivate.AddRange(Directory.GetFiles($"{sourceRoot}\\{mod.ModName}\\Nativepc", "*", SearchOption.AllDirectories)));

            pathsToActivate.ForEach(modPath =>
            filesToActivate.Add(new FileInfo(modPath)));

            allActiveModPaths.ForEach(modPath =>
            activeFiles.Add(new FileInfo(modPath)));

            List<FileInfo> tempActivePaths = new List<FileInfo>(activeFiles);
            List<FileInfo> tempPathsToActivate = new List<FileInfo>(filesToActivate);
            foreach (FileInfo activeFile in activeFiles)
            {
                foreach (FileInfo fileToActivate in filesToActivate)
                {
                    if (File.Exists(activeFile.FullName))
                    {
                        if (fileToActivate.Name == activeFile.Name)
                        { //TODO make sure it iterates through all elements
                            for (int i = 0; i < tempPathsToActivate.Count; i++)
                            {
                                if (tempPathsToActivate[i].Name == activeFile.Name)
                                {
                                    tempPathsToActivate.Remove(tempPathsToActivate[i]);
                                }
                            }
                        }
                        else if (!activeFiles.Any(f => f.Name == fileToActivate.Name))
                        {
                            for (int i = 0; i < tempActivePaths.Count; i++)
                            {
                                if (tempActivePaths[i].Name == activeFile.Name)
                                {
                                    File.Delete(tempActivePaths[i].FullName);
                                    tempActivePaths.Remove(tempActivePaths[i]);

                                    foreach (var item in Directory.GetDirectories(targetRoot, "*", SearchOption.AllDirectories))
                                    {
                                        if ((Directory.GetFiles(item).Length + Directory.GetDirectories(item).Length) == 0)
                                        {
                                            Directory.Delete(item);
                                            //TODO remove all empty directories
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            activeFiles = tempActivePaths;
            filesToActivate = tempPathsToActivate;

            foreach (FileInfo filePath in filesToActivate)
            {
                string[] pathParts = filePath.FullName.Split(Path.DirectorySeparatorChar);
                int startAfter = Array.IndexOf(pathParts, "Nativepc"); //TODO fix hardcoding

                string? subFolder = Path.GetDirectoryName($"{targetRoot}\\{string.Join(Path.DirectorySeparatorChar.ToString(), pathParts, startAfter, pathParts.Length - startAfter)}");
                if (subFolder is not null && !Directory.Exists(subFolder))
                {
                    Directory.CreateDirectory(subFolder);
                }

                DirectoryInfo dir = new DirectoryInfo(string.Join(Path.DirectorySeparatorChar.ToString(), pathParts, 0, pathParts.Length - 1));
                FileInfo file = dir.GetFiles().FirstOrDefault(f => f.FullName == filePath.FullName);

                string targetFilePath = Path.Combine(subFolder, file.Name);
                file.CopyTo(targetFilePath);

            }

            return Task.CompletedTask;
        }

        private async Task SavePackAsync() //TODO check this
        {
            allModPacks.Add(SelectedModPack);
            try
            {
                File.WriteAllText(modpackRoot, JsonSerializer.Serialize(allModPacks));
            }
            catch (Exception ex)
            {
                ErrorMessage.ShowError(ex, "Try putting the Modloader folder somewhere public.");
            }
            OnPropertyChanged("allModPacks");
            ModPackVisibility = true;
            FilterModPacks();
        }

        private Task NewPack()
        {
            SelectedModPack = null;
            SelectedModPack = new ModPack
            {
                ID = Guid.NewGuid(),
                Name = "New Pack",
                Mods = new List<Mod>()
            };
            FilterModPacks();
            FilterMods();

            return Task.CompletedTask;
        }

        private Task EditPack()
        {
            allActiveMods = SelectedModPack.Mods;
            FilterMods();
            return Task.CompletedTask;
        }

        private void FilterModPacks()
        {
            ModPacks = null;
            if (string.IsNullOrWhiteSpace(ModPackSearchText))
            {
                ModPacks = allModPacks;
            }
            else
            {
                ModPacks = allModPacks.Where(p => p.Name.ToLower().StartsWith(ModPackSearchText.ToLower()));
            }
        }
        #endregion

        #region Mod methods
        public Task UpdateMods()
        {
            allModPacks.Clear();
            if (File.Exists(modpackRoot))
            {
                IEnumerable<ModPack> test = JsonSerializer.Deserialize<IEnumerable<ModPack>>(File.ReadAllText(modpackRoot));
                foreach (ModPack pack in test)
                {
                    allModPacks.Add(new ModPack
                    {
                        ID = pack.ID,
                        Name = pack.Name,
                        Mods = pack.Mods
                    });
                }
            }
            FilterModPacks();
            allMods = modsService.GetModsFolders(sourceRoot).ToList();

            return Task.CompletedTask;
        }

        private Task RemoveModsAsync()
        {
            List<Mod> tempList = allActiveMods;
            Mods.Add(SelectedActiveMod);

            tempList.Remove(SelectedActiveMod);
            ActiveMods = new ObservableCollection<Mod>(allActiveMods);
            OnPropertyChanged("Mods");

            return Task.CompletedTask;
        }

        private Task AddModsAsync()
        {
            allActiveMods.Add(SelectedMod);

            List<Mod> templist = Mods.ToList();
            templist.Remove(SelectedMod);
            Mods = new ObservableCollection<Mod>(templist);
            SelectedMod = null;
            ActiveMods = new ObservableCollection<Mod>(allActiveMods);


            return Task.CompletedTask;
        }

        private void FilterMods()
        {
            ModPackVisibility = false;
            Mods = null;
            if (string.IsNullOrWhiteSpace(ModSearchText))
            {
                Mods = ToObservableCollection(allMods);
                ActiveMods = ToObservableCollection(allActiveMods);
            }
            else
            {
                ActiveMods = (ObservableCollection<Mod>)allActiveMods.Where(p => p.ModName.ToLower().StartsWith(ModPackSearchText.ToLower()));
                Mods = ToObservableCollection(allMods.Where(p => p.ModName.ToLower().StartsWith(ModPackSearchText.ToLower())).ToList());
            }


            //List<Mod> templist = new List<Mod>(Mods.ToList());
            //foreach (Mod mod in ActiveMods)
            //{
            //    templist.Remove(mod);
            //}
            //Mods = ToObservableCollection(templist);
            //templist = null;
        }
        #endregion
    }
}