using BlazorDualMode.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorDualMode.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddServerSideBlazor();

            // Server Side Blazor doesn't register HttpClient by default
            if (!services.Any(s => s.ServiceType == typeof(HttpClient)))
            {
                // Setup HttpClient for server side in a client side compatible fashion
                services.AddScoped(sp =>
                {
                    // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.      
                    var uriHelper = sp.GetRequiredService<NavigationManager>();
                    return new HttpClient { BaseAddress = new Uri(uriHelper.BaseUri) };
                });
            }

            services.AddScoped<IWeatherForecastService, WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
