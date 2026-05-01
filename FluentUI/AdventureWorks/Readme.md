# FluentUI.AdventureWorks

`FluentUI.AdventureWorks` is an interactive server-rendered Blazor application that turns the AdventureWorks SQL Server schema into a runtime-driven Fluent UI data explorer. The application does not hardcode entity pages or column definitions; it builds them from EF Core metadata every time the user selects a table or view.

## Project role in the solution

This project is the EF Core-backed half of `FluentUI.Samples.sln`. It pairs:

- `Microsoft.FluentUI.AspNetCore.Components`
- `Microsoft.FluentUI.AspNetCore.Components.DataGrid.EntityFrameworkAdapter`
- a scaffolded `AdventureWorksContext`
- a reflection-based UI layer

The project targets `net10.0`.

## Application startup

`Program.cs` configures the app as an interactive server-rendered Blazor app:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents()
    .AddDataGridEntityFrameworkAdapter();
```

The database layer is registered through `AddPooledDbContextFactory<AdventureWorksContext>`. SQL Server configuration enables:

- `UseHierarchyId()`
- `UseNetTopologySuite()`
- `EnableRetryOnFailure()`

That combination is important because the scaffolded model includes types that would otherwise require custom provider support.

## Metadata-driven navigation

`Components\Layout\NavMenu.razor` opens a context from the pooled factory, reads `context.Model.GetEntityTypes()`, groups entities by schema, and renders each entry as:

```text
/?entity=<schema>.<table-or-view>
```

This means the left navigation tree is not a hardcoded menu. If the EF Core model changes and you re-scaffold it, the navigation updates automatically.

## Main page execution flow

`Components\Pages\Home.razor` is the core of the sample.

On parameter changes it:

1. Reads `?entity=` and `?virtualize=` from the query string.
2. Creates a fresh `AdventureWorksContext`.
3. Sets `QueryTrackingBehavior.NoTracking` because the page is a read-only explorer.
4. Locates the requested `IEntityType` by matching `schema.table` or `schema.view`.
5. Uses reflection to call `DbContext.Set<TEntity>()` for the runtime CLR type.
6. Generates grid columns with `FluentDataGridEntityHelpers.ColumnsRenderFragment(...)`.
7. Builds a runtime `IEqualityComparer<T>` from primary-key metadata.
8. Instantiates `PaginatedDataGrid<TEntity>` dynamically.

The entire grid is therefore a runtime composition over EF Core metadata.

## Column generation rules

`Components\Controls\FluentDataGridEntityHelpers.cs` creates a `PropertyColumn<,>` for each eligible `IProperty`. It explicitly excludes:

- key properties
- foreign-key properties
- concurrency tokens
- array properties
- database types `json`, `xml`, `geography`, `geometry`, `uniqueidentifier`, `hierarchyid`, and `rowversion`

This is why the sample can point at many different AdventureWorks tables and still render a usable grid without special-case code for every entity type.

## Multi-select equality

Fluent UI's select column needs a stable equality comparison. Because the entity type is only known at runtime, `Home.razor` builds a comparer dynamically:

- use primary-key properties when the entity has a key
- otherwise fall back to non-shadow, non-concurrency properties
- compile a selector expression returning `object[]`
- create `EntityEqualityComparer<T>` via reflection

That comparer is passed into `PaginatedDataGrid<TEntity>` so row selection still behaves correctly across paging and virtualization.

## Pagination and virtualization

`Components\Controls\PaginatedDataGrid.razor` exposes a `Virtualize` parameter and toggles behavior like this:

- `Virtualize = false`: create a `PaginationState` with `ItemsPerPage = 20`
- `Virtualize = true`: set pagination to `null` and let the grid virtualize rows

The page exposes this switch through a `FluentSwitch` bound to the `virtualize` query parameter, so you can compare both modes against the same entity set.

## Database model and scaffolding

`Data\AdventureWorksContext.cs` is a scaffolded EF Core context over the AdventureWorks database. The model includes a large set of tables and views, including types that use SQL Server-specific features.

To regenerate the model after schema changes:

```powershell
.\FluentUI\AdventureWorks\scaffold.cmd
```

The script runs:

```text
dotnet ef dbcontext scaffold "Name=ConnectionStrings:AdventureWorks" Microsoft.EntityFrameworkCore.SqlServer --no-onconfiguring --context-dir Data --output-dir Data\Models --force
```

## Configuration

Set the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AdventureWorks": "Data Source=localhost;Initial Catalog=AdventureWorks;Integrated Security=True;Encrypt=False;"
  }
}
```

Use SQL authentication if Windows authentication is not available. For real deployments, revisit the encryption and trust settings instead of copying the sample string unchanged.

## Development endpoints

`Properties\launchSettings.json` configures:

- `https://localhost:52900`
- `http://localhost:52901`

## Build and run

From the repository root:

```powershell
dotnet build .\FluentUI.Samples.sln
dotnet run --project .\FluentUI\AdventureWorks\FluentUI.AdventureWorks.csproj
```

Then choose a table or view from the left navigation menu.

## Files worth reading first

- `Program.cs`
- `Components\Layout\NavMenu.razor`
- `Components\Pages\Home.razor`
- `Components\Controls\PaginatedDataGrid.razor`
- `Components\Controls\FluentDataGridEntityHelpers.cs`
- `Data\AdventureWorksContext.cs`
- `scaffold.cmd`
