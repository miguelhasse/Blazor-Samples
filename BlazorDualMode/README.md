# BlazorDualMode

`BlazorDualMode` is a hosted Blazor sample that renders the same component tree in either Blazor Server or Blazor WebAssembly without changing the application build. The server project always owns the HTTP entry point, but each request can choose a different render mode and boot script.

## Solution structure

| Project | Purpose |
|---|---|
| `Server\BlazorDualMode.Server.csproj` | ASP.NET Core host, `_Host` page, API controller, server-side forecast service |
| `Client\BlazorDualMode.Client.csproj` | WebAssembly entry point and browser-side forecast service |
| `Shared\BlazorDualMode.Shared.csproj` | Shared pages, contracts, DTOs, and runtime helpers |

All three projects target `net7.0`.

## What the sample is demonstrating

### 1. Request-time render mode selection

`Server\Pages\_Host.cshtml.cs` resolves a `RenderMode` by reading:

1. `Request.Query["blazor-mode"]`
2. `Environment.GetEnvironmentVariable("ASPNETCORE_BLAZOR_MODE")`

If neither value parses successfully, the app defaults to `WebAssemblyPrerendered`.

That model then drives two things:

- the Razor component render mode passed into `<component type="@typeof(Client.App)" ... />`
- the boot script name in `_Host.cshtml`

```html
<script src="_framework/blazor.@(Model.HostingMode).js" autostart="false"></script>
```

`HostingMode` is `"server"` for the server render modes and `"webassembly"` for the WebAssembly render modes, so the page swaps between `blazor.server.js` and `blazor.webassembly.js` automatically.

### 2. Shared UI with mode-specific services

Shared pages depend on `IWeatherForecastService` from `Shared\IWeatherForecastService.cs`.

- `Server\WeatherForecastService.cs` generates forecast data locally.
- `Client\WeatherForecastService.cs` calls `/api/SampleData/WeatherForecasts`.
- `Server\Controllers\SampleDataController.cs` exposes the HTTP endpoint used by the browser-hosted variant.

Because both implementations satisfy the same interface, `Shared\Pages\FetchData.razor` does not care where the code is executing.

### 3. Prerender-state preservation

`Shared\Pages\FetchDataPreserveState.razor` shows the correct pattern for avoiding a duplicate data fetch during prerender + hydration:

- register a persistence callback with `PersistentComponentState`
- write the fetched payload with `PersistAsJson`
- read it back on hydration with `TryTakeFromJson`

The host page only emits `<persist-component-state />` when the chosen render mode is prerendered, so this page is also a useful reference for the contract between the host page and the component tree.

### 4. Runtime environment detection

`Shared\RuntimeMode.cs` exposes a browser check based on:

```csharp
RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"))
```

That helper gives shared code a lightweight way to branch on server vs. browser execution when interface-based abstraction is not enough.

## Request and startup flow

```text
Browser request
    -> Server\Pages\_Host.cshtml.cs resolves RenderMode
    -> Server\Pages\_Host.cshtml renders Client.App
    -> Host page chooses blazor.server.js or blazor.webassembly.js
    -> Blazor starts with autostart disabled and explicit Blazor.start(...)
    -> Shared pages resolve services from the active host
```

`_Host.cshtml` also calls `Blazor.start(...)` manually with environment and SignalR logging configuration, which makes the bootstrap path more explicit than the default template-generated host page.

## Routes in the shared UI

| Route | File | Notes |
|---|---|---|
| `/` | `Client\Pages\Index.razor` | Standard landing page |
| `/counter` | `Client\Pages\Counter.razor` | Simple interactive counter |
| `/fetchdata` | `Shared\Pages\FetchData.razor` | Forecast fetch using the active service implementation |
| `/fetch-with-preserve-state` | `Shared\Pages\FetchDataPreserveState.razor` | Same idea, but persists prerendered state |

The navigation menu in `Client\Components\NavMenu.razor` links directly to those routes.

## Render mode values

The sample accepts the built-in ASP.NET Core render mode names:

| Value | Effect |
|---|---|
| `Server` | Interactive server rendering without prerender |
| `ServerPrerendered` | Interactive server rendering with prerender |
| `WebAssembly` | Client-side WebAssembly without prerender |
| `WebAssemblyPrerendered` | WebAssembly with server prerender; default |

Example:

```text
https://localhost:5001/?blazor-mode=ServerPrerendered
```

PowerShell example:

```powershell
$env:ASPNETCORE_BLAZOR_MODE = "ServerPrerendered"
dotnet run --project .\BlazorDualMode\Server\BlazorDualMode.Server.csproj
```

## Build and run

From the repository root:

```powershell
dotnet restore .\BlazorDualMode.sln
dotnet build .\BlazorDualMode.sln
dotnet run --project .\BlazorDualMode\Server\BlazorDualMode.Server.csproj
```

Open the server URL reported by ASP.NET Core and add `?blazor-mode=...` as needed.

## Extension points

- Add more shared services by following the existing `IWeatherForecastService` split.
- Add more mode-sensitive pages in `Shared\Pages` so they compile into both hosts.
- Replace the mode resolution logic in `_Host.cshtml.cs` if you want per-user cookies, feature flags, or route-based selection instead of query string / environment values.

## Related files

- `Server\Pages\_Host.cshtml`
- `Server\Pages\_Host.cshtml.cs`
- `Server\Controllers\SampleDataController.cs`
- `Client\Program.cs`
- `Shared\Pages\FetchData.razor`
- `Shared\Pages\FetchDataPreserveState.razor`
