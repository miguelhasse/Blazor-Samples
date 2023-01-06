using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorDualMode.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDualMode.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddHttpClient(string.Empty, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

            Console.WriteLine($"Environment: {builder.HostEnvironment.Environment}");
            Console.WriteLine(builder.Configuration.GetDebugView());

            var host = builder.Build();

            //using (var serviceScope = host.Services.CreateScope())
            //{
            //    var http = serviceScope.ServiceProvider.GetService<HttpClient>();
            //    using var response = await http.GetAsync("settings");

            //    if (response.IsSuccessStatusCode)
            //    {
            //        using var stream = await response.Content.ReadAsStreamAsync();
            //        builder.Configuration.AddJsonStream(stream);
            //    }
            //}

            await host.RunAsync();
        }
    }
}
