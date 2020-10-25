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
        private List<string> mods;

        public List<string> Mods
        {
            get { return mods; }
            set { Set(ref mods, value, true); }
        }

        public string SelectedMod
        {
            get { return selectedMod; }
            set { Set(ref selectedMod, value, true); }
        }

        private string selectedMod;
        private readonly IModsService modsService;

        public RelayCommand GetCommand { get; }

        public MainViewModel(IConfiguration configuration, IModsService modsService)
        {
            GetCommand = new RelayCommand(async () => await GetAllAsync());
            this.modsService = modsService;
            Mods = new List<string>(this.modsService.GetModsFolders(configuration.GetValue<string>("RootFolder")));
        }

        private Task GetAllAsync()
        {
            return Task.CompletedTask;
        }
    }
}
