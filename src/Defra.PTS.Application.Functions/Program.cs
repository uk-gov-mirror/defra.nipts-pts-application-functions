using Defra.PTS.Application.Api.Services.Configuration;
using Defra.PTS.Application.Functions.Configuration;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Defra.PTS.Application.Functions
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>
        /// Configures and runs the Azure Functions host.
        /// </summary>
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<JsonOptions>(options =>
                    {
                        options.SerializerOptions.PropertyNamingPolicy = null;
                    });

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
