using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using ModFiles;

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
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.folders.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .Build();
                    services.AddSingleton<IConfiguration>(configuration);
                    services.ConfigureWritable<FolderParams>(configuration.GetSection("modFiles"));
                    services.AddScoped<IModFilesService, ModFilesService>();
                    services.AddScoped<Executor>();
                    services.AddScoped<ValidateParams>();
                });
        }
    }
}
