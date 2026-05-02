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
6. Generates grid columns with `FluentDataGridEntityHelpers.ColumnsRenderFragment(...)`. Scalar properties become `PropertyColumn<,>` instances; `xml`, `json`, `geography`, and `geometry` properties become `TemplateColumn<TEntity>` instances with built-in summarising renderers; keys, foreign keys, and opaque types are omitted.
7. Builds a runtime `IEqualityComparer<T>` from primary-key metadata.
8. Instantiates `PaginatedDataGrid<TEntity>` dynamically.

The entire grid is therefore a runtime composition over EF Core metadata.

## FluentDataGridEntityHelpers deep dive

`Components\Controls\FluentDataGridEntityHelpers.cs` is the main runtime column factory used by `Components\Pages\Home.razor`. Once the page resolves an `IEntityType` from EF Core metadata, it passes that metadata object into `ColumnsRenderFragment(...)` and lets the helper turn model properties into grid column components.

### Three-tier column classification

The helper classifies every mapped property into one of three tiers before it produces any output:

| Tier | Outcome | Properties matched |
|---|---|---|
| **Property** | `PropertyColumn<TEntity, TProperty>` | All remaining scalar properties |
| **Template** | `TemplateColumn<TEntity>` with a built-in summariser | `xml`, `json`, `geography`, `geometry` |
| **Exclude** | column omitted | keys, foreign keys, concurrency tokens, arrays, `uniqueidentifier`, `hierarchyid`, `rowversion` |

`ClassifyProperty(IProperty)` implements this with a switch on `p.GetColumnType()` and a fallback guard on structural property roles.

### Property columns

`AddPropertyColumnComponent(...)` constructs `PropertyColumn<,>` with the entity CLR type and the property's CLR type. `BuildPropertyExpression(...)` builds the strongly typed lambda the grid needs for its `Property` parameter. The helper sets `Title` from `property.GetColumnName()` and merges any caller-supplied attributes such as `Sortable = true`.

Even though the page assembles the grid dynamically, each generated property column still behaves like a normal strongly typed Fluent UI grid column.

### Template columns

`AddTemplateColumnComponent(...)` constructs `TemplateColumn<TEntity>` for properties that have a database type the grid cannot sort or display natively.

The built-in default renderers are:

| SQL type | Default cell content |
|---|---|
| `xml` / `json` | `{ xml · 1 432 chars }` — char-count badge in a `<code>` element |
| `geometry` / `geography` (Point subtype) | `(lat, lon)` coordinate pair via NTS `Point.X` / `Point.Y` |
| `geometry` / `geography` (other shapes) | WKT string via NTS `Geometry.ToString()` |

The NTS package `Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite` is already referenced in the project, so these helpers work without extra dependencies.

### Custom template rendering

`ColumnsRenderFragment(...)` accepts an optional third parameter:

```csharp
Func<IProperty, Func<object, RenderFragment>?>? templateFactory = null
```

When provided, the factory is called for every Template-classified property. Return a `Func<object, RenderFragment>` to supply custom cell content, or return `null` to fall back to the built-in default for that property:

```csharp
FluentDataGridEntityHelpers.ColumnsRenderFragment(entityType,
    prop => new Dictionary<string, object> { { "Sortable", true } },
    templateFactory: prop => prop.GetColumnType() == "xml"
        ? entity => b => { /* custom rendering */ }
        : null);
```

### Why the Exclude tier exists

Key and foreign-key columns are used internally by the sample for selection identity and are not useful as display columns. Concurrency tokens and arrays have no single-cell representation. `uniqueidentifier`, `hierarchyid`, and `rowversion` are storage-mechanics columns with opaque representations that add noise to a generic explorer grid.

### Naming conventions in the helper

- `property.GetColumnName()` is used for the column title so the UI reflects the actual database column name rather than only the CLR property name.
- `type.GetProperties()` is used to iterate properties so inherited mapped properties are included alongside properties declared directly on the entity type.

This helper is the reason the sample can point at a large scaffolded model and still get a sensible default grid without per-entity Razor markup.

## FluentDataGridReflectionHelpers deep dive

`Components\Controls\FluentDataGridReflectionHelpers.cs` is a second, more generic column generator. Unlike `FluentDataGridEntityHelpers`, it does **not** depend on EF Core metadata. Instead it works directly from a CLR `Type` and public `PropertyInfo` objects.

That different input shape makes it useful when you have a runtime type but do not have, or do not want to depend on, `IEntityType`. In other words:

- `FluentDataGridEntityHelpers` is the EF-aware helper for database-backed entity metadata
- `FluentDataGridReflectionHelpers` is the plain reflection helper for arbitrary CLR models

The reflection helper exposes two entry points:

- `ColumnsRenderFragment(...)`, which returns one combined fragment containing all generated columns
- `ColumnRenderFragments(...)`, which yields one fragment per property when a caller needs finer-grained composition

Its property filter is broader than the EF Core helper's filter. `GetPropertyColumns(type)` includes public instance properties from the full inheritance chain and keeps only:

- value types
- `string`
- `Uri`
- nullable wrappers around those same underlying types

This is intentionally a UI-shape filter rather than a persistence-model filter. The helper is trying to identify properties that have a reasonable default single-cell representation without relying on database metadata.

When it emits a column, `AddPropertyColumnComponent(...)` adds a couple of presentation features that the EF helper does not:

- `Title` comes from `[Display(Name = ...)]` when present, otherwise it falls back to `propertyInfo.Name`
- `Format` comes from `[DisplayFormat(DataFormatString = ...)]`, which lets reflected view models opt into custom formatting without custom column templates

That design makes the reflection helper better suited for DTOs, view models, or ad hoc runtime models that carry .NET display metadata instead of EF Core mapping metadata.

At the moment, `Home.razor` uses `FluentDataGridEntityHelpers` because the page starts from `IEntityType` and wants EF-aware filtering. `FluentDataGridReflectionHelpers` still matters because it captures the reusable non-EF version of the same pattern: inspect a runtime type, build strongly typed expression trees, and emit `PropertyColumn<,>` instances without hand-written Razor columns.

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
- `Components\Controls\FluentDataGridReflectionHelpers.cs`
- `Data\AdventureWorksContext.cs`
- `scaffold.cmd`
