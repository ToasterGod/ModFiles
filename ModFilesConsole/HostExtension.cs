using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ModFilesConsole
{
    public static class HostExtension
    {
        public static void Execute(this IHost host)
        {
            host.Services.GetService<Executor>().Execute();
        }
    }

}
