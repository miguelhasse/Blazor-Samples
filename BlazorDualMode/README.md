# BlazorDualMode

`BlazorDualMode` is a .NET 7 sample application that demonstrates how to run the same Blazor application under both **server-side** and **WebAssembly** hosting models without rebuilding. The hosting mode can be switched at runtime via a query string parameter or an environment variable, making it an ideal reference for teams evaluating or migrating between Blazor deployment strategies.

## What the sample demonstrates

- Runtime selection between Blazor Server and Blazor WebAssembly hosting models
- Switching modes via the `blazor-mode` query string parameter or the `ASPNETCORE_BLAZOR_MODE` environment variable
- Sharing UI components across both hosting models through a common `Shared` project
- Interface-driven service registration so each hosting model can provide its own implementation
- Component state persistence across prerendering and hydration using `PersistentComponentState`
- A weather forecast feature with both a basic and a state-preserving variant

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- Visual Studio 2022, VS Code with the C# extension, or the .NET CLI

## Building the sample

```powershell
# Restore and build
dotnet restore BlazorDualMode.sln
dotnet build   BlazorDualMode.sln

# Release build
dotnet build -c Release BlazorDualMode.sln
```

## Running the sample

Start the `Server` project; it serves both hosting modes:

```powershell
dotnet run --project .\BlazorDualMode\Server\BlazorDualMode.Server.csproj
```

Or open `BlazorDualMode.sln` in Visual Studio, set `BlazorDualMode.Server` as the startup project, and press **F5**.

The application will be available at `https://localhost:5001` by default.

## Switching hosting modes

### Query string

Append `?blazor-mode=<value>` to any URL:

| Value | Behaviour |
|-------|-----------|
| `Server` | Server-side rendering, no prerender |
| `ServerPrerendered` | Server-side rendering with prerender |
| `WebAssembly` | WebAssembly, no prerender |
| `WebAssemblyPrerendered` | WebAssembly with prerender (default) |

Example: `https://localhost:5001/?blazor-mode=ServerPrerendered`

### Environment variable

Set `ASPNETCORE_BLAZOR_MODE` before launching the server:

```powershell
$env:ASPNETCORE_BLAZOR_MODE = "ServerPrerendered"
dotnet run --project .\BlazorDualMode\Server\BlazorDualMode.Server.csproj
```

## How dual-mode hosting works

### 1. Host page selects the boot script

`_Host.cshtml.cs` reads the mode from the query string or environment variable and resolves a `RenderMode` enum value. It also sets a `HostingMode` string (`"server"` or `"webassembly"`) used by the host page to load the correct Blazor boot script:

```html
<script src="_framework/blazor.@(Model.HostingMode).js" autostart="false"></script>
```

### 2. Interface-based services

Both projects register identical `IWeatherForecastService` interfaces but with different implementations:

- **Client** — sends an HTTP request to `/api/SampleData/WeatherForecasts`
- **Server** — generates random data in-process

This pattern allows shared Razor components to call the service without knowing which hosting model is active.

### 3. State persistence across prerendering

`FetchDataPreserveState.razor` uses `PersistentComponentState` to serialise fetched data during server prerendering and restore it on the client after hydration, avoiding a redundant network call:

```csharp
persistingSubscription = ApplicationState.RegisterOnPersisting(PersistForecasts);

forecasts = ApplicationState.TryTakeFromJson<WeatherForecast[]>("fetchdata", out var restored)
    ? restored!
    : await ForecastService.GetForecastAsync();
```

### 4. Runtime environment detection

`RuntimeMode.cs` exposes a static flag that components can read to determine whether they are executing in the browser:

```csharp
public static bool IsWebAssembly =
    RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
```

## Pages and components

| Path | Description |
|------|-------------|
| `/` | Home page |
| `/counter` | Interactive counter — demonstrates client-side state |
| `/fetchdata` | Weather forecast table (basic) |
| `/fetchdata-preserve-state` | Weather forecast table with state persistence |

## Comparison of hosting models

| Aspect | Server-side | WebAssembly |
|--------|-------------|-------------|
| **Execution** | .NET runs on the server | .NET runs in the browser via WebAssembly |
| **Initial load** | Fast — no large download | Slower — downloads .NET runtime |
| **Interactivity** | Requires SignalR connection | Works fully offline after load |
| **Latency** | Network round-trips on each interaction | Instant local execution |
| **Scalability** | Server resources per connection | Workload offloaded to client |

## Additional references

- [Prerender and integrate ASP.NET Core Razor components](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/prerendering-and-integration)
- [ASP.NET Core Blazor hosting models](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models)
- [Persist prerendered state in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/prerendering-and-integration#persist-prerendered-state)
