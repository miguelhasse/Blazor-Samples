// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

using FluentUI.TripPin;
using FluentUI.TripPin.Model;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.OData.Extensions.Client;

var serviceRoot = new Uri("https://services.odata.org/TripPinRESTierService/(S(1hmjktdokfcn3zoxsca01sjn))/");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient()
    .ConfigureHttpClientDefaults(config =>
    {
        config.ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
    });
builder.Services.AddODataClient("TripPin").AddHttpClient();
builder.Services.AddScoped<Container>(sp =>
{
    var clientFactory = sp.GetRequiredService<IODataClientFactory>();
    var container = clientFactory.CreateClient<Container>(serviceRoot, "TripPin");
    container.BuildingRequest += (sender, eventArgs) =>
    {
        eventArgs.Headers.Remove("OData-MaxVersion");
        eventArgs.Headers.Remove("OData-Version");
    };
    return container;
});

builder.Services.AddFluentUIComponents().AddDataGridODataAdapter();

await builder.Build().RunAsync();
