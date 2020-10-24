using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ModFilesClient.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel
        {
            get
            {
                return GetMainViewModel().Result;
            }
        }

        public static Task<MainViewModel> GetMainViewModel()
        {
            MainViewModel vm = App.ServiceProvider.GetRequiredService<MainViewModel>();

            return Task.FromResult(vm);
        }
    }
}
