using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ModFilesClient.Configuration
{
    public class FolderSettings
    {
        public string RootFolder { get; set; }
    }
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services, HostBuilderContext builderContext)
        {
            services.Configure<FolderSettings>(settings => builderContext.Configuration.GetSection("FolderSettings").Bind(settings));
            return services;
            //TODO use this instead of injecting IConfiguration
            //Inject as IOption<FolderSettings> option
            //Use FolderSettings folderSettings = option.Value
        }
    }

}
