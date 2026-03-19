# BlazorMapTiles

`BlazorMapTiles` is a .NET 7 sample that combines **Azure Maps**, **Blazor**, and a **custom vector tile service** to render OpenStreetMap-derived railway and station data. Geographic data is stored in the checked-in `Assets\tiles.db` MBTiles database and served to the browser as Protocol Buffers (PBF) vector tiles, which Azure Maps renders on the fly with custom styling.

## What the sample demonstrates

- Hosting a Blazor app with both **server-side** and **WebAssembly** render modes
- Serving **vector tiles** (`.pbf`) from ASP.NET Core controllers at `Tiles/{z}/{x}/{y}.pbf`
- Using `AzureMapsControl.Components` to add custom `VectorTileSource`, `LineLayer`, and `BubbleLayer` overlays
- Reading and decompressing tiles from a **SQLite MBTiles** database with `Microsoft.Data.Sqlite`
- Encoding and decoding the **Mapbox Vector Tile** specification using Protocol Buffers (`protobuf-net`)
- Sharing UI pages between hosting models through the `Shared` project
- HTTP caching with **ETags** for efficient tile delivery
- Web Mercator coordinate math (tile ↔ lat/lng, QuadKey support)

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- An [Azure Maps subscription key](https://learn.microsoft.com/en-us/azure/azure-maps/quick-demo-map-app#create-an-azure-maps-account)
- Visual Studio 2022, VS Code with the C# extension, or the .NET CLI

## Configuration

Both the server and the client require an Azure Maps subscription key.

**`Server\appsettings.json`** (used for server-side rendering and tile loading):

```json
{
  "AzureMaps": {
    "SubscriptionKey": "<your-key>"
  }
}
```

**`Client\wwwroot\appsettings.json`** (used when running in WebAssembly mode):

```json
{
  "AzureMaps": {
    "SubscriptionKey": "<your-key>"
  }
}
```

The tile database `Assets\tiles.db` is already checked in, so no separate data import is required.

## Building the sample

```powershell
dotnet restore .\BlazorMapTiles\BlazorMapTiles.sln
dotnet build   .\BlazorMapTiles\BlazorMapTiles.sln
```

## Running the sample

```powershell
dotnet run --project .\BlazorMapTiles\Server\BlazorMapTiles.Server.csproj
```

Or open `BlazorMapTiles.sln` in Visual Studio, set `BlazorMapTiles.Server` as the startup project, and press **F5**.

## Useful pages

| Path | Description |
|------|-------------|
| `/` | Main Azure Maps view with railway lines (LineLayer) and station points (BubbleLayer) |
| `/editor` | Drawing-enabled map for creating and editing geometry |

## Hosting mode switch

The host page supports switching the Blazor render mode at runtime:

| Method | Example |
|--------|---------|
| Query string | `?blazor-mode=ServerPrerendered` |
| Environment variable | `ASPNETCORE_BLAZOR_MODE=ServerPrerendered` |

If neither is supplied, the sample defaults to `WebAssemblyPrerendered`.

## How vector tile delivery works

```
Browser requests tile (z, x, y)
        ↓
TilesController.GetTile(z, x, y)
        ↓
CustomTileService  →  CustomTilesRepository (SQLite SELECT)
        ↓
GZip decompression of raw MBTiles blob
        ↓
VectorTile.Decoder  (Protocol Buffers deserialization)
        ↓
VectorTile.Encoder  (re-encode for HTTP response)
        ↓
ETag check → 304 Not Modified if tile unchanged
        ↓
HTTP 200  Content-Type: application/x-protobuf
```

Tiles are cached on the client using standard HTTP ETags. The server sets a one-minute `Expires` header so the browser avoids redundant round-trips for tiles it has already rendered.

## Map layer configuration

`Index.razor` adds two layers over the custom tile source after the map is ready:

| Layer | Source layer | Renders |
|-------|-------------|---------|
| `LineLayer` | `railway` | Rail lines; stroke width interpolates from 1 px (zoom 4) to 4 px (zoom 16) |
| `BubbleLayer` | `stations` | Station points; radius and colour configurable via expressions |

## Additional references

- [Azure Maps documentation](https://learn.microsoft.com/en-us/azure/azure-maps/)
- [Mapbox Vector Tile Specification](http://mapbox.github.io/vector-tile-spec/)
- [MBTiles Specification](https://github.com/mapbox/mbtiles-spec)
- [Managed identities for Azure Maps](https://techcommunity.microsoft.com/t5/azure-maps-blog/managed-identities-for-azure-maps/ba-p/3666312)
- [ASP.NET Core Blazor hosting models](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models)

