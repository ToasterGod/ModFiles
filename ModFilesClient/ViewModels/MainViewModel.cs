using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Configuration;
using ModFilesClient.Helpers;
using ModFilesClient.Models;
using ModFilesClient.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModFilesClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region mod variables
        private List<string> allMods;
        private List<string> allActiveMods;

        public bool ModVisibility
        {
            get => modVisibility;
            set => Set(ref modVisibility, value, true);
        }
        private bool modVisibility;

        public IEnumerable<string> Mods
        {
            get => mods;
            set => Set(ref mods, value, true);
        }
        private IEnumerable<string> mods;

        public List<string> SelectedMods
        {
            get => selectedMods;
            set => Set(ref selectedMods, value, true);
        }
        private List<string> selectedMods;

        public string ModSearchText
        {
            get => modSearchText;
            set => Set(nameof(ModSearchText), ref modSearchText, value, true);
        }
        private string modSearchText;

        public IEnumerable<string> ActiveMods
        {
            get => activeMods;
            set => Set(ref activeMods, value, true);
        }
        private IEnumerable<string> activeMods;

        public List<string> SelectedActiveMods
        {
            get => selectedActiveMods;
            set => Set(ref selectedActiveMods, value, true);
        }
        private List<string> selectedActiveMods;

        public string ActiveModSearchText
        {
            get => activeModSearchText;
            set => Set(nameof(ActiveModSearchText), ref activeModSearchText, value, true);
        }
        private string activeModSearchText;
        #endregion

        #region modpack variables
        public bool ModPackVisibility
        {
            get => modPackVisibility;
            set
            {
                Set(ref modPackVisibility, value, true);
                ModVisibility = !value;
            }
        }
        private bool modPackVisibility;

        public ModPack SelectedModPack
        {
            get => selectedModPack;
            set
            {
                Set(ref selectedModPack, value, true);
                if (value != null)
                {
                    Mods = value.Mods;
                }
            }
        }
        private ModPack selectedModPack;

        public IEnumerable<ModPack> ModPacks
        {
            get => modPacks;
            set => Set(ref modPacks, value, true);
        }
        private IEnumerable<ModPack> modPacks;

        private List<ModPack> allModPacks = new List<ModPack>();
        private bool savePack;

        public string ModPackSearchText
        {
            get => modPackSearchText;
            set => Set(nameof(ModPackSearchText), ref modPackSearchText, value, true);
        }
        private string modPackSearchText;
        #endregion

        private readonly IModsService modsService;
        private IConfiguration configuration;

        public RelayCommand NewPackCommand { get; }
        public RelayCommand DeletePackCommand { get; }
        public RelayCommand EditPackCommand { get; }

        public RelayCommand TempPackCommand { get; }
        public RelayCommand SavePackCommand { get; }
        public RelayCommand AddModCommand { get; }
        public RelayCommand RemoveModCommand { get; }
        public RelayCommand CancelSelectionCommand { get; }

        public MainViewModel(IConfiguration configuration, IModsService modsService)
        {
            //TODO CanSelectMultipleItems inherited from multiselect
            NewPackCommand = new RelayCommand(async () => await NewPackAsync());
            TempPackCommand = new RelayCommand(async () => await TempPackAsync());
            SavePackCommand = new RelayCommand(async () => await SavePackAsync());
            DeletePackCommand = new RelayCommand(async () => await DeletePackAsync());
            EditPackCommand = new RelayCommand(async () => await EditPackAsync());
            CancelSelectionCommand = new RelayCommand(async () => await CancelSelectionAsync());

            this.modsService = modsService;
            this.configuration = configuration;
            ModPackVisibility = true;
        }

        private Task CancelSelectionAsync()
        {
            SelectedModPack = null;
            SelectedMods = null;
            ModPackVisibility = true;
            return Task.CompletedTask;
        }

        public Task UpdateModsAsync()
        {
            allModPacks.Clear();
            if (File.Exists("ModPacks.json"))
            {
                allModPacks.AddRange(JsonSerializer.Deserialize<IEnumerable<ModPack>>(File.ReadAllText("ModPacks.json")));
            }
            FilterModPacks();
            allMods = new List<string>(modsService.GetModsFolders(configuration.GetValue<string>("RootFolder")));
            return Task.CompletedTask;
        }

        #region ModPack methods
        private Task SaveModPacksAsync()
        {
            File.WriteAllText("ModPacks.json", JsonSerializer.Serialize(allModPacks));
            return Task.CompletedTask;
        }

        private async Task DeletePackAsync()
        {
            allModPacks.Remove(selectedModPack);
            await SaveModPacksAsync();
            FilterModPacks();
        }

        private async Task TempPackAsync()
        {
            selectedModPack.Mods = ActiveMods.ToList();
            if (!savePack)
            {
                FilterModPacks();
                FilterMods();
                //TODO activate the temppack
            }
        }

        private async Task SavePackAsync()
        {
            savePack = true;
            await TempPackAsync();
            allModPacks.Add(SelectedModPack);
            try
            {
                File.WriteAllText("ModPacks.json", JsonSerializer.Serialize(allModPacks));
            }
            catch (Exception ex)
            {
                ErrorMessage.ShowError(ex, "Try putting the Modloader map somewhere public.");
            }
            savePack = false;
        }

        private async Task NewPackAsync()
        {
            SelectedModPack = new ModPack
            {
                ID = Guid.NewGuid(),
                Name = "New Pack",
                Mods = new List<string>()
            };
            FilterModPacks();
            FilterMods();
        }

        private Task EditPackAsync()
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
        private void FilterMods()
        {
            Mods = null;
            if (string.IsNullOrWhiteSpace(ModSearchText))
            {
                Mods = allMods;
                ActiveMods = allActiveMods;
            }
            else
            {
                ActiveMods = allActiveMods.Where(p => p.ToLower().StartsWith(ModPackSearchText.ToLower()));
                Mods = allMods.Where(p => p.ToLower().StartsWith(ModPackSearchText.ToLower()));
            }
        }
        #endregion
    }
}