# FluentUI Samples

This folder contains two sample applications that demonstrate [`Microsoft.FluentUI.AspNetCore.Components`](https://www.fluentui-blazor.net) DataGrid integration against different data backends. The projects show how to use advanced DataGrid features — virtualization, pagination, multi-select, and dynamic column generation — in both server-rendered and WebAssembly Blazor apps.

## Included projects

### FluentUI.AdventureWorks

An **interactive server-rendered** Blazor app that uses Entity Framework Core with SQL Server and the Fluent UI DataGrid Entity Framework adapter. The app inspects the EF Core model at runtime and renders a fully sortable, paginated or virtualized grid for any entity in the AdventureWorks database without hardcoding column definitions.

Highlights:

- Pooled `IDbContextFactory<AdventureWorksContext>` for efficient database connections
- Runtime reflection and LINQ expression trees to generate `FluentDataGrid` columns dynamically
- Toggle between **virtualized** (fixed-height, render-on-scroll) and **paginated** (20 rows per page) modes
- Multi-select with a reflection-based `IEqualityComparer<T>` built from EF Core primary key metadata
- Entity and schema discovery via query string parameter (`?entity=<schema.Table>`)
- Supports SQL Server `HierarchyId` and NetTopologySuite geospatial columns

### FluentUI.TripPin

A **Blazor WebAssembly** app that connects to the public [TripPin OData v4 sample service](https://services.odata.org/TripPinRESTierService) and renders entity sets through the Fluent UI DataGrid OData adapter.

Highlights:

- Typed OData client proxy generated from the service CSDL metadata
- `AddODataClient` / `Microsoft.OData.Extensions.Client` for dependency-injected service access
- Dynamic NavMenu and DataGrid columns derived from the OData service model at runtime
- No local database required — runs entirely in the browser against a public service

## Solution and target frameworks

- Solution: `..\FluentUI.Samples.sln`
- Both projects target `net9.0` and `net10.0`
- NuGet package versions are managed centrally in `Directory.Packages.props`


## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- For `FluentUI.AdventureWorks`: a SQL Server instance with the [AdventureWorks sample database](https://learn.microsoft.com/en-us/sql/samples/adventureworks-install-configure) restored
- For `FluentUI.TripPin`: no local infrastructure required

## Configuration

### AdventureWorks

Update `AdventureWorks\appsettings.json` so `ConnectionStrings:AdventureWorks` points at your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "AdventureWorks": "Data Source=localhost;Initial Catalog=AdventureWorks;Integrated Security=True;Encrypt=False;"
  }
}
```

> **Note:** `Integrated Security=True` uses Windows authentication. Replace with `User ID=...;Password=...` for SQL authentication. Set `Encrypt=True` for production environments.

To regenerate the EF Core model after schema changes, run:

```powershell
.\AdventureWorks\scaffold.cmd
```

### TripPin

No configuration is needed. The app targets the public TripPin OData service endpoint defined in `TripPin\Program.cs`.

## Running the samples

From the repository root:

```powershell
# AdventureWorks (server-rendered, requires SQL Server)
dotnet run --project .\FluentUI\AdventureWorks\FluentUI.AdventureWorks.csproj

# TripPin (WebAssembly, no local database required)
dotnet run --project .\FluentUI\TripPin\FluentUI.TripPin.csproj
```

Or open `FluentUI.Samples.sln` in Visual Studio and start the project you want to run.

Default ports:

| Project | URL |
|---------|-----|
| AdventureWorks | `https://localhost:7173` |
| TripPin | `http://localhost:5000` |

## Architecture notes

### Dynamic column generation

Both projects avoid hardcoded column lists. Instead, helper classes use reflection and LINQ expression trees to inspect the data model at runtime and produce `RenderFragment` column definitions for `FluentDataGrid`:

- **AdventureWorks** — reads `IEntityType` metadata from the EF Core model
- **TripPin** — reads `IEdmEntitySet` metadata from the OData CSDL

### Virtualization vs. pagination

`PaginatedDataGrid.razor` exposes a `Virtualize` parameter:

| Mode | Container height | Rows rendered | Navigation |
|------|-----------------|---------------|------------|
| `Virtualize = false` (default) | Grows with content | All rows loaded in pages | Pagination control |
| `Virtualize = true` | Fixed 400 px | Only visible rows | Scroll |

### Equality comparison for multi-select

A custom `IEqualityComparer<T>` is built dynamically from primary key properties (EF Core) or all non-shadow properties (OData). This ensures the DataGrid can correctly track selected rows even for entity types that are only known at runtime.

## Additional references

- [Fluent UI Blazor documentation](https://www.fluentui-blazor.net)
- [Microsoft.FluentUI.AspNetCore.Components on GitHub](https://github.com/microsoft/fluentui-blazor)
- [AdventureWorks sample database](https://learn.microsoft.com/en-us/sql/samples/adventureworks-install-configure)
- [OData TripPin tutorial](https://learn.microsoft.com/en-us/odata/webapi/getting-started)
- [ASP.NET Core Blazor documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor)
- [`AdventureWorks\Readme.md`](AdventureWorks/Readme.md)
- [`TripPin\Readme.md`](TripPin/Readme.md)

