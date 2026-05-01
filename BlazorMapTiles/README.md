# BlazorMapTiles

`BlazorMapTiles` is a hosted Blazor sample that serves custom vector tiles from ASP.NET Core and renders them through Azure Maps. The sample is intentionally end-to-end: it includes the map UI, the HTTP tile endpoint, a local MBTiles database, and a small vector tile library used to decode and re-encode tile payloads.

## Solution structure

| Project or asset | Purpose |
|---|---|
| `Server\BlazorMapTiles.Server.csproj` | ASP.NET Core host, tile controller, SQLite-backed tile repository, server-side rendering |
| `Client\BlazorMapTiles.Client.csproj` | WebAssembly entry point and Azure Maps client configuration |
| `Shared\BlazorMapTiles.Shared.csproj` | Shared Razor pages and Azure Maps UI |
| `VectorTile\Hasseware.VectorTile.csproj` | `netstandard2.0` tile encoder/decoder and geometry helpers |
| `Assets\tiles.db` | MBTiles SQLite database checked into the repo |

The `Server`, `Client`, and `Shared` projects target `net7.0`. The `VectorTile` library targets `netstandard2.0`.

## Runtime architecture

### UI side

`Shared\Pages\Index.razor` and `Shared\Pages\Editor.razor` both create an `AzureMap` component. On the map `Ready` event they register a `VectorTileSource` whose tile URL template points back to the sample server:

```text
tiles/{z}/{x}/{y}.pbf
```

When Azure Maps confirms that the source has been added, the page injects two layers:

| Layer type | Source layer name | Responsibility |
|---|---|---|
| `LineLayer` | `railway` | Renders rail lines with zoom-based width and opacity interpolation |
| `BubbleLayer` | `station` | Renders station points |

`Editor.razor` adds `DrawingToolbarOptions` on top of the same tile source so you can inspect drawing events and emitted geometry.

### Server side

The HTTP pipeline is configured in `Server\Startup.cs`:

- MVC + JSON options
- server-side Blazor
- static files and hosted WebAssembly assets
- dependency registration for the MBTiles repository and tile service
- Azure Maps configuration binding

The important registrations are:

- singleton `CustomTilesRepository`, initialized with `..\..\Assets\tiles.db`
- scoped `IVectorTileService` implemented by `CustomTileService`

## Tile request lifecycle

`Server\Controllers\TilesController.cs` owns the vector tile endpoint. For `.pbf` requests the flow is:

```text
HTTP GET /Tiles/{zoom}/{x}/{y}.pbf
    -> validate XYZ coordinates
    -> IVectorTileService.GetVectorTile(...)
    -> convert incoming Y from XYZ to TMS with WebMercator.FlipYCoordinate
    -> read compressed tile blobs from SQLite
    -> decompress with GZipStream
    -> decode vector tile layers with Hasseware.VectorTile
    -> optionally filter to requested ?layer= values
    -> merge layers with the same name
    -> re-encode tile content
    -> emit weak ETag and one-minute Expires header
```

Important response behavior:

- `200 OK` with `image/pbf` when the tile contains features
- `304 Not Modified` when `If-None-Match` matches the generated weak ETag
- `204 No Content` when the tile contains no usable geometry
- `400 Bad Request` for invalid tile coordinates or formats

The controller also exposes a quadkey-based route, although the shared UI uses the XYZ tile path.

## Data access details

`Server\Storage\CustomTilesRepository.cs` opens the MBTiles file in read-only shared-cache mode and queries the `Tiles` table by `ZoomLevel`, `Column`, and `Row`. The repository returns raw `Stream` instances so the tile service can decide how to decode them.

`Server\Services\CustomTileService.cs` then:

1. flips the Y coordinate from XYZ to TMS
2. decompresses each blob with `GZipStream`
3. decodes vector tile layers from the stream
4. filters by requested layer names if present
5. merges duplicate layer names into a single output layer with extent `4096` and version `2`

## Vector tile library

`VectorTile\Hasseware.VectorTile.csproj` is a small reusable library that depends on:

- `protobuf-net` for vector tile serialization
- `NetTopologySuite.IO.GeoJSON4STJ` for GeoJSON conversion helpers

The library contains:

- `Encoder.cs` and `Decoder.cs`
- `VectorTileLayer` and `VectorTileFeature`
- coordinate conversion helpers
- GeoJSON extension methods

That split keeps the ASP.NET Core host focused on transport and storage while the vector tile encoding logic stays independent.

## Hosted render mode support

Like `BlazorDualMode`, this sample can switch between server and WebAssembly hosting at request time. `Server\Pages\_Host.cshtml.cs` reads `blazor-mode` or `ASPNETCORE_BLAZOR_MODE`, defaults to `WebAssemblyPrerendered`, and then selects `blazor.server.js` or `blazor.webassembly.js` in the host page.

This matters because the map pages live in `Shared`, so the same Razor UI can run under either hosting model while still talking to the same tile endpoint.

## Configuration

Both hosting modes need an Azure Maps subscription key.

`Server\appsettings.json`:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "<your-key>"
  }
}
```

`Client\wwwroot\appsettings.json`:

```json
{
  "AzureMaps": {
    "SubscriptionKey": "<your-key>"
  }
}
```

The shared extension method in `Shared\Extensions\ServiceCollectionExtensions.cs` forwards that value into `AzureMapsControl.Components`.

## Routes

| Route | File | Purpose |
|---|---|---|
| `/` | `Shared\Pages\Index.razor` | Main railway/station map |
| `/editor` | `Shared\Pages\Editor.razor` | Map plus drawing tools |
| `/Tiles/{zoom}/{x}/{y}.pbf` | `Server\Controllers\TilesController.cs` | Vector tile endpoint consumed by Azure Maps |

## Build and run

From the repository root:

```powershell
dotnet restore .\BlazorMapTiles.sln
dotnet build .\BlazorMapTiles.sln
dotnet run --project .\BlazorMapTiles\Server\BlazorMapTiles.Server.csproj
```

If you want to test a different hosted render mode:

```powershell
$env:ASPNETCORE_BLAZOR_MODE = "ServerPrerendered"
dotnet run --project .\BlazorMapTiles\Server\BlazorMapTiles.Server.csproj
```

## Useful modification points

- Swap `CustomTileService` for `MapboxTileService` if you want to proxy remote vector tiles instead of the local MBTiles file.
- Change the `Tiles` URL template or add `?layer=` filtering if you want multiple thematic views.
- Adjust the layer expressions in `Index.razor` and `Editor.razor` to change railway stroke interpolation or station styling.

## Related files

- `Server\Startup.cs`
- `Server\Controllers\TilesController.cs`
- `Server\Services\CustomTileService.cs`
- `Server\Storage\CustomTilesRepository.cs`
- `Shared\Pages\Index.razor`
- `Shared\Pages\Editor.razor`
- `VectorTile\VectorTileLayer.cs`
