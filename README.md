# Blazor Samples

This repository contains a small set of focused Blazor samples that each isolate one architectural idea instead of trying to be a full application template. The codebase is split into three independent solutions so you can open, build, and reason about each scenario without unrelated dependencies in the way.

## Repository topology

| Solution | Primary scenario | Main technologies | Current target frameworks |
|---|---|---|---|
| `BlazorDualMode.sln` | Switch the same UI between Blazor Server and Blazor WebAssembly at runtime | ASP.NET Core, Razor components, prerendering, persistent component state | `net7.0` |
| `BlazorMapTiles.sln` | Serve custom vector tiles from ASP.NET Core and render them in Azure Maps | ASP.NET Core, Azure Maps, SQLite MBTiles, Protocol Buffers | `net7.0` |
| `FluentUI.Samples.sln` | Build metadata-driven Fluent UI data grids over EF Core and OData | Fluent UI Blazor, EF Core, SQL Server, OData client | `net10.0` |

Top-level folders map directly to those solutions:

```text
BlazorDualMode\
BlazorMapTiles\
FluentUI\
BlazorDualMode.sln
BlazorMapTiles.sln
FluentUI.Samples.sln
```

## Sample architecture at a glance

### `BlazorDualMode`

`BlazorDualMode` uses the classic hosted WebAssembly shape: `Server`, `Client`, and `Shared`. The server host renders `Client.App` through `_Host.cshtml`, but `_Host.cshtml.cs` decides at request time whether the page should boot `blazor.server.js` or `blazor.webassembly.js`. The selected render mode comes from the `blazor-mode` query string first and falls back to `ASPNETCORE_BLAZOR_MODE`.

The interesting part is that the UI does not change when the hosting model changes. Shared pages depend on `IWeatherForecastService`, and each hosting mode supplies its own implementation: the server implementation generates forecasts in-process, while the WebAssembly implementation calls back into the server API. One page also uses `PersistentComponentState` so you can inspect the difference between a naive prerendered fetch and a preserved prerendered payload.

Docs: [`BlazorDualMode\README.md`](BlazorDualMode/README.md)

### `BlazorMapTiles`

`BlazorMapTiles` keeps the same `Server` / `Client` / `Shared` split but adds two data-centric pieces: a checked-in `Assets\tiles.db` MBTiles database and a `VectorTile` class library. The server reads compressed vector tile blobs from SQLite, converts from TMS row addressing to XYZ addressing, decodes the Protocol Buffer payload, optionally filters layers, then re-encodes the result for HTTP delivery from `TilesController`.

The shared pages use `AzureMapsControl.Components` to register a `VectorTileSource` that points back at the server route `tiles/{z}/{x}/{y}.pbf`. Once the source is ready, the sample adds a `LineLayer` for railway geometry and a `BubbleLayer` for station points. A second page wires in Azure Maps drawing controls so you can inspect map events and geometry output.

Docs: [`BlazorMapTiles\README.md`](BlazorMapTiles/README.md)

### `FluentUI`

The `FluentUI` folder contains two samples that both build `FluentDataGrid` instances from runtime metadata instead of hardcoded column definitions. `AdventureWorks` is an interactive server-rendered app over EF Core and SQL Server. `TripPin` is a WebAssembly app over the public TripPin OData service. Both samples use the Fluent UI Blazor DataGrid adapters and both create columns, row comparers, and navigation from model metadata.

This is the most reflection-heavy part of the repository. In `AdventureWorks`, the app reads `IEntityType` metadata from the EF Core model, locates the requested entity set from `?entity=<schema.Table>`, creates `DbSet<TEntity>` through reflection, and dynamically instantiates a generic `PaginatedDataGrid<TEntity>`. In `TripPin`, the same pattern is driven from the OData EDM model and a generated client proxy produced by OData Connected Service.

Docs: [`FluentUI\README.md`](FluentUI/README.md)

## Running the samples

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) for `BlazorDualMode` and `BlazorMapTiles`
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) for the `FluentUI` samples
- An Azure Maps subscription key for `BlazorMapTiles`
- A SQL Server instance with the AdventureWorks sample database restored for `FluentUI.AdventureWorks`

### Typical commands

```powershell
dotnet build .\BlazorDualMode.sln
dotnet build .\BlazorMapTiles.sln
dotnet build .\FluentUI.Samples.sln
```

Run individual applications from the repository root:

```powershell
dotnet run --project .\BlazorDualMode\Server\BlazorDualMode.Server.csproj
dotnet run --project .\BlazorMapTiles\Server\BlazorMapTiles.Server.csproj
dotnet run --project .\FluentUI\AdventureWorks\FluentUI.AdventureWorks.csproj
dotnet run --project .\FluentUI\TripPin\FluentUI.TripPin.csproj
```

## Code organization patterns reused in the repo

### Hosted Blazor layout

Both `BlazorDualMode` and `BlazorMapTiles` use the older hosted WebAssembly pattern:

- `Server` owns the HTTP pipeline, static files, prerendering host page, and APIs
- `Client` owns the browser boot entry point and client-specific services
- `Shared` contains reusable pages and components compiled into both apps

That structure makes it easy to compare how the same Razor UI behaves when the execution boundary moves between browser and server.

### Metadata-driven UI

The Fluent UI samples avoid hand-authored table definitions. Instead they:

1. Discover available entities from a metadata source.
2. Build navigation links from that metadata.
3. Generate `PropertyColumn<,>` components at runtime.
4. Create equality comparers dynamically so multi-select works for unknown entity types.

That pattern is useful if you want a grid explorer, admin shell, or diagnostics UI over a changing schema.

### Vendored static assets

The two hosted samples include a vendored `open-iconic` asset directory under `Client\wwwroot\css`. Those files are not custom code; they exist so the classic Blazor navigation components can import `open-iconic-bootstrap.min.css` and resolve the associated font files locally.

## Documentation map

- [`BlazorDualMode\README.md`](BlazorDualMode/README.md)
- [`BlazorMapTiles\README.md`](BlazorMapTiles/README.md)
- [`FluentUI\README.md`](FluentUI/README.md)
- [`FluentUI\AdventureWorks\Readme.md`](FluentUI/AdventureWorks/Readme.md)
- [`FluentUI\TripPin\Readme.md`](FluentUI/TripPin/Readme.md)
