using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using ModFilesClient.Helpers;
using ModFilesClient.Models;
using ModFilesClient.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
        private ObservableCollection<Mod> activeMods;

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
        private bool savePack;

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
                if (value != null)
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

        public AsyncRelayCommand TempPackCommand { get; }
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
            TempPackCommand = new AsyncRelayCommand(async () => await TempPackAsync());
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

        private Task CancelSelectionAsync()
        {
            SelectedModPack = null;
            SelectedMod = null;
            ModPackVisibility = true;
            return Task.CompletedTask;
        }

        #region ModPack methods
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

        private Task TempPackAsync()
        {
            selectedModPack.Mods = ActiveMods.ToList();
            if (!savePack)
            {
                FilterModPacks();
                FilterMods();
                ActivatePackAsync();
                ModPackVisibility = false;
            }
            return Task.CompletedTask;
        }

        private Task ActivatePackAsync()
        {
            List<string> activeModPaths = Directory.GetDirectories(targetRoot, SearchOption.AllDirectories.ToString()).ToList();
            List<string> modPathsToActivate = new List<string>();
            List<string> c = new List<string>();
            foreach (Mod mod in ActiveMods)
            {
                modPathsToActivate.AddRange(Directory.GetDirectories($"{targetRoot}\\{mod.ModName}\\Nativepc", SearchOption.AllDirectories.ToString()));
            }

            foreach (string obj in activeModPaths)
            {
                foreach (string item in modPathsToActivate)
                {
                    if (obj != item)
                    {
                        activeModPaths.Remove(obj);
                    }
                }
                foreach (var item in modPathsToActivate)
                {
                    if (item == obj)
                    {
                        modPathsToActivate.Remove(item);
                    }
                }
            }
            //string source = Path.Join(sourceRoot, mod.ModName);
            //string? subFolder = Path.GetDirectoryName(targetRoot);
            //if (subFolder != null && !Directory.Exists(subFolder))
            //{
            //    Directory.CreateDirectory(subFolder);
            //}
            //File.Copy(source, targetRoot, true);

            return Task.CompletedTask;
        }

        private async Task SavePackAsync()
        {
            savePack = true;
            await TempPackAsync();
            allModPacks.Add(SelectedModPack);
            try
            {
                File.WriteAllText(modpackRoot, JsonSerializer.Serialize(allModPacks));
            }
            catch (Exception ex)
            {
                ErrorMessage.ShowError(ex, "Try putting the Modloader map somewhere public.");
            }
            savePack = false;
            OnPropertyChanged("allModPacks");
            ModPackVisibility = true;
        }

        private Task NewPack()
        {
            SelectedModPack = new ModPack
            {
                ID = Guid.NewGuid(),
                Name = "New Pack",
                Mods = new List<Mod>()
            };
            FilterModPacks();
            FilterMods();
            ModPackVisibility = false;

            return Task.CompletedTask;
        }

        private Task EditPack()
        {
            ModPackVisibility = false;
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
        }
        #endregion
    }
}