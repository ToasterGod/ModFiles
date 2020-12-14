using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Configuration;
using ModFilesClient.Models;
using ModFilesClient.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModFilesClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region mod variables
        public List<string> Mods
        {
            get { return mods; }
            set { Set(ref mods, value, true); }
        }
        private List<string> mods;

        public List<string> SelectedMods
        {
            get { return selectedMods; }
            set { Set(ref selectedMods, value, true); }
        }
        private List<string> selectedMods;

        #endregion

        #region modpack variables
        public ModPack SelectedModPack
        {
            get
            { return selectedModPack; }
            set
            {
                Set(ref selectedModPack, value, true);
                ToggleVisibility();
            }
        }
        private ModPack selectedModPack;

        public IEnumerable<ModPack> ModPacks
        {
            get { return modPacks; }
            set
            {
                Set(ref modPacks, value, true);
            }
        }
        private IEnumerable<ModPack> modPacks;
        #endregion

        public string ModPackSearchText { get; set; }
        private readonly IModsService modsService;

        public RelayCommand NewPackCommand { get; }
        public RelayCommand TempPackCommand { get; }
        public RelayCommand DeletePackCommand { get; }

        public MainViewModel(IConfiguration configuration, IModsService modsService)
        {
            NewPackCommand = new RelayCommand(async () => await NewPackAsync());
            TempPackCommand = new RelayCommand(async () => await TempPackAsync());
            DeletePackCommand = new RelayCommand(async () => await DeletePackAsync());

            this.modsService = modsService;
            Mods = new List<string>(this.modsService.GetModsFolders(configuration.GetValue<string>("RootFolder")));
        }
        private void ToggleVisibility()
        {
            if (SelectedModPack != null)
            {
                //TODO either change the view to one where you edit the modpacks files or change visibleMods to contain all the mods available for selection
            }
            else
            {

            }
        }

        private async Task DeletePackAsync()
        {

        }

        private async Task TempPackAsync()
        {

        }

        private async Task NewPackAsync()
        {

        }
    }
}