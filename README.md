# Blazor Samples

This repository collects several focused Blazor samples that explore different hosting models, UI stacks, and data access patterns. Each sample is organized as its own solution or project group so you can inspect a single scenario in isolation or compare approaches side by side.

## Included samples

### BlazorDualMode

Demonstrates how to switch a Blazor app between server and WebAssembly hosting at runtime by using either the `blazor-mode` query string parameter or the `ASPNETCORE_BLAZOR_MODE` environment variable.

Key ideas:

- Runtime selection between server and WebAssembly boot scripts
- Shared components rendered through different hosting modes
- Query-string and environment-driven configuration

Docs: [`BlazorDualMode\README.md`](BlazorDualMode/README.md)  
Entry point: `BlazorDualMode.sln`

### BlazorMapTiles

Shows how to host an Azure Maps-based Blazor application that consumes a custom vector tile endpoint backed by OpenStreetMap-derived data stored in `Assets\tiles.db`.

Key ideas:

- Azure Maps integration from a Blazor app
- Custom vector tile delivery from ASP.NET Core controllers
- Shared UI across server and WebAssembly hosting modes

Docs: [`BlazorMapTiles\README.md`](BlazorMapTiles/README.md)  
Entry point: `BlazorMapTiles.sln`

### FluentUI samples

Contains two samples that showcase `Microsoft.FluentUI.AspNetCore.Components` with different data sources.

- `FluentUI.AdventureWorks` uses a SQL Server-backed Entity Framework Core model and the Fluent UI DataGrid Entity Framework adapter.
- `FluentUI.TripPin` uses the public TripPin OData service and the Fluent UI DataGrid OData adapter.

Docs: [`FluentUI\README.md`](FluentUI/README.md)  
Entry point: `FluentUI.Samples.sln`

## Getting started

1. Install the .NET SDK required by the sample you want to run.
2. Open the corresponding solution file in Visual Studio, or run the sample with `dotnet run`.
3. Follow the sample-specific configuration steps in the linked project README before launching apps that require external services or credentials.

## References

- [Managed identities for Azure Maps](https://techcommunity.microsoft.com/t5/azure-maps-blog/managed-identities-for-azure-maps/ba-p/3666312)
- [Prerender and integrate ASP.NET Core Razor components](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/prerendering-and-integration)
- [Mapbox Vector Tile Specification](http://mapbox.github.io/vector-tile-spec/)
