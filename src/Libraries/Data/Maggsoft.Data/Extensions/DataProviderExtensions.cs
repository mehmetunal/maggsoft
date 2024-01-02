using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Maggsoft.Data.DataProviders
{
    public static class DataProviderExtensions
    {
        public static string GetCurrentConnectionString(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}