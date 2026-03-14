# BlazorMapTiles

`BlazorMapTiles` is a .NET 7 sample that combines Azure Maps, Blazor, and a custom vector tile service. The app renders OpenStreetMap-derived railway and station data from the checked-in `Assets\tiles.db` database and overlays it in an Azure Maps control.

## What the sample demonstrates

- Hosting a Blazor app with both server and WebAssembly render modes
- Serving vector tiles from ASP.NET Core controllers at `Tiles/{z}/{x}/{y}.pbf`
- Using `AzureMapsControl.Components` to render custom tile layers
- Sharing UI pages between hosting models through the `Shared` project

## Solution structure

- `Client` contains the Blazor WebAssembly front end and Azure Maps client configuration.
- `Server` hosts the app, exposes the tile endpoint, and loads `Assets\tiles.db`.
- `Shared` contains the reusable Razor pages, including the map pages.
- `VectorTile` contains the vector tile support library used by the server.

Open `BlazorMapTiles.sln` to load the full sample in Visual Studio.

## Prerequisites

- .NET 7 SDK
- An Azure Maps subscription key

## Configuration

The server and client read the Azure Maps key from configuration. Update `Server\appsettings.json` with your own value:

```json
"AzureMaps": {
  "SubscriptionKey": "<your-key>"
}
```

The sample includes a local tile database at `Assets\tiles.db`, so no separate data import step is required for a basic run.

## Running the sample

From the repository root:

```powershell
dotnet run --project .\BlazorMapTiles\Server\BlazorMapTiles.Server.csproj
```

You can also open `BlazorMapTiles.sln` and start the `BlazorMapTiles.Server` project from Visual Studio.

## Hosting mode switch

The host page supports switching render mode through either of these options:

- Query string: `?blazor-mode=ServerPrerendered`
- Environment variable: `ASPNETCORE_BLAZOR_MODE`

If neither is supplied, the sample defaults to `WebAssemblyPrerendered`.

## Useful pages

- `/` displays the main Azure Maps view with railway and station layers.
- `/editor` loads the drawing-enabled map experience for interacting with map geometry.

