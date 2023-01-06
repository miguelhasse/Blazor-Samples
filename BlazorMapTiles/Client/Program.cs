using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMapTiles.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddHttpClient(string.Empty, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddAzureMaps(configuration => configuration.SubscriptionKey = builder.Configuration["AzureMaps:SubscriptionKey"]);

            await builder.Build().RunAsync();
        }
    }
}
