using Defra.PTS.Application.Api.Services.Configuration;
using Defra.PTS.Application.Functions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Defra.PTS.Application.Functions
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    var connection = string.Empty;
#if DEBUG
                    connection = configuration["sql_db"];
#else
                    connection = configuration.GetConnectionString("sql_db");
#endif
                    services.AddDefraRepositoriesServices(connection);
                    services.AddDefraApiServices();
                })
                .Build();

            host.Run();
        }
    }
}
