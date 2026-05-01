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

## FluentDataGridEntityHelpers deep dive

`Components\Controls\FluentDataGridEntityHelpers.cs` is the main runtime column factory used by `Components\Pages\Home.razor`. Once the page resolves an `IEntityType` from EF Core metadata, it passes that metadata object into `ColumnsRenderFragment(...)` and lets the helper turn model properties into real `PropertyColumn<TEntity, TProperty>` instances.

The flow inside the helper is:

1. `ColumnsRenderFragment(...)` asks `GetPropertyColumns(type)` for the list of grid-eligible `IProperty` instances.
2. It opens a render-tree region per property so each generated column is emitted as a separate unit in the fragment.
3. `AddPropertyColumnComponent(...)` constructs `PropertyColumn<,>` with the entity CLR type and the property's CLR type.
4. `BuildPropertyExpression(...)` builds the strongly typed lambda the grid needs for its `Property` parameter.
5. The helper sets `Title` from `property.GetColumnName()` and merges any caller-supplied attributes, such as `Sortable = true`.

That means the page never has to know whether the selected entity is `Product`, `Customer`, or some scaffolded view type. The helper converts EF Core metadata into a fully typed grid definition at render time.

`GetPropertyColumns(...)` is where most of the guardrails live. It filters out properties that are technically present in the model but poor fits for a generic text-oriented grid:

- key properties, because the sample uses them internally for identity and selection more than for display
- foreign-key properties, which would otherwise add a lot of implementation-detail columns
- concurrency tokens, which are usually storage mechanics rather than user-facing data
- array-valued CLR properties, because `PropertyColumn` expects a scalar-ish value path
- SQL Server types `json`, `xml`, `geography`, `geometry`, `uniqueidentifier`, `hierarchyid`, and `rowversion`

The SQL type exclusions are especially important in AdventureWorks. The sample intentionally points at a scaffolded SQL Server model that contains provider-specific types. Rather than failing or rendering poor default text for those members, the helper excludes them up front so the generated grid remains broadly usable across many tables and views.

`BuildPropertyExpression(...)` deserves special attention because it is what keeps the dynamic grid strongly typed. The helper creates a parameter expression for the entity CLR type, accesses `property.PropertyInfo`, and wraps that access in a closed generic `Func<TEntity, TProperty>`. The resulting expression is then passed into a runtime-constructed `PropertyColumn<TEntity, TProperty>`. Even though the page itself is assembling the grid dynamically, each generated column still behaves like a normal strongly typed Fluent UI grid column.

There are also two deliberate naming choices in the helper:

- it uses `property.GetColumnName()` for the title, so the UI reflects the actual database column name rather than only the CLR property name
- it iterates `type.GetProperties()`, which means inherited mapped properties are considered along with properties declared directly on the entity type

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
