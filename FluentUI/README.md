# FluentUI Samples

The `FluentUI` folder contains two samples built around the same idea: use runtime metadata to generate `FluentDataGrid` instances instead of hand-authoring column definitions. One sample gets its metadata from EF Core and SQL Server; the other gets it from an OData service model and a generated client proxy.

## Projects

| Project | Hosting model | Metadata source | Data source |
|---|---|---|---|
| `AdventureWorks\FluentUI.AdventureWorks.csproj` | Interactive server rendering | EF Core `IEntityType` metadata | SQL Server AdventureWorks database |
| `TripPin\FluentUI.TripPin.csproj` | Blazor WebAssembly | OData EDM metadata | Public TripPin OData v4 service |

Both current projects target `net10.0`. Package versions are managed centrally in `Directory.Packages.props`.

## Shared design pattern

Although the data sources are different, both samples follow the same runtime pipeline:

1. Discover available entities from a metadata model.
2. Build a navigation menu from that metadata.
3. Accept `?entity=...` in the main page.
4. Materialize an `IQueryable<T>` for the selected entity set.
5. Generate `PropertyColumn<,>` components for scalar properties and `TemplateColumn<TEntity>` components for complex types dynamically.
6. Create a custom equality comparer so row multi-select works for types only known at runtime.
7. Render everything through a generic `PaginatedDataGrid<T>`.

That makes the samples useful as references for admin explorers, diagnostics tools, and schema-driven internal applications.

## Package layout

`Directory.Packages.props` centralizes the versions for:

- `Microsoft.FluentUI.AspNetCore.Components`
- Fluent UI DataGrid adapters for Entity Framework and OData
- EF Core SQL Server packages
- OData client packages

The presence of both `.NET 9` and `.NET 10` package properties in that file reflects central package management, but the sample projects themselves currently compile for `net10.0`.

## AdventureWorks in technical terms

`AdventureWorks` is a server-rendered schema explorer over a restored SQL Server sample database.

Key implementation points:

- `Program.cs` registers `AddFluentUIComponents().AddDataGridEntityFrameworkAdapter()`
- `AddPooledDbContextFactory<AdventureWorksContext>` keeps context creation efficient for interactive requests
- SQL Server options enable `HierarchyId`, `NetTopologySuite`, and retry-on-failure
- `Components\Layout\NavMenu.razor` groups entities by schema and links to `/?entity=<schema.Table>`
- `Components\Pages\Home.razor` reflects over `DbContext.Set<TEntity>()` to create the runtime query
- `Components\Controls\FluentDataGridEntityHelpers.cs` classifies each property into one of three tiers: scalar properties become `PropertyColumn<,>` instances; `xml`, `json`, `geography`, and `geometry` properties become `TemplateColumn<TEntity>` instances with built-in summarising renderers; keys, foreign keys, and opaque types (`uniqueidentifier`, `hierarchyid`, `rowversion`) are excluded entirely

`PaginatedDataGrid.razor` switches between paging and virtualization by either creating a `PaginationState` or leaving it `null`.

Docs: [`AdventureWorks\Readme.md`](AdventureWorks/Readme.md)

## TripPin in technical terms

`TripPin` is a browser-hosted metadata explorer for the public OData sample service.

Key implementation points:

- `Program.cs` registers `AddODataClient("TripPin").AddHttpClient()`
- A scoped `Container` is built from the TripPin service root and the generated proxy types under `Connected Services\TripPinService`
- `BuildingRequest` removes `OData-MaxVersion` and `OData-Version` headers before requests are sent
- `Layout\NavMenu.razor` enumerates EDM entity sets from `Container.Format.LoadServiceModel()`
- `Pages\Home.razor` resolves the CLR type for the selected entity set and reflects over `Container.CreateQuery<TEntity>(...)`
- `Controls\FluentDataGridEntityHelpers.cs` skips collection and complex properties because they do not map cleanly to simple grid columns

This sample is entirely browser-hosted; there is no local API or database in the solution.

Docs: [`TripPin\Readme.md`](TripPin/Readme.md)

## Running the samples

From the repository root:

```powershell
dotnet build .\FluentUI.Samples.sln

dotnet run --project .\FluentUI\AdventureWorks\FluentUI.AdventureWorks.csproj
dotnet run --project .\FluentUI\TripPin\FluentUI.TripPin.csproj
```

AdventureWorks also requires a valid `ConnectionStrings:AdventureWorks` entry in `AdventureWorks\appsettings.json`.

## Which sample to inspect for what

| If you need a reference for... | Start with |
|---|---|
| Dynamic Fluent UI grid columns from EF Core metadata | `AdventureWorks` |
| Dynamic Fluent UI grid columns from OData metadata | `TripPin` |
| Reflection-based row equality for metadata-driven grids | both samples' `Home.razor` pages |
| Fluent UI navigation built from runtime schema information | both `NavMenu.razor` files |
| Server-rendered interactive Fluent UI app | `AdventureWorks` |
| Pure WebAssembly Fluent UI app | `TripPin` |
