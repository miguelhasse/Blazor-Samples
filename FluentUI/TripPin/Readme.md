# FluentUI.TripPin

`FluentUI.TripPin` is a Blazor WebAssembly application that explores the public TripPin OData service through a metadata-driven Fluent UI grid. Like the AdventureWorks sample, it does not define pages or columns per entity set. Instead it reads the service model at runtime and builds navigation and `FluentDataGrid` columns dynamically.

## Project role in the solution

This project is the OData-backed half of `FluentUI.Samples.sln`. It combines:

- `Microsoft.FluentUI.AspNetCore.Components`
- `Microsoft.FluentUI.AspNetCore.Components.DataGrid.ODataAdapter`
- `Microsoft.OData.Client`
- `Microsoft.OData.Extensions.Client`
- a generated TripPin client proxy under `Connected Services\TripPinService`

The project targets `net10.0`.

## Startup and service registration

`Program.cs` is the entire composition root for the sample.

Important parts:

1. Define the TripPin service root:

   ```csharp
   var serviceRoot = new Uri("https://services.odata.org/TripPinRESTierService/(S(1hmjktdokfcn3zoxsca01sjn))/");
   ```

2. Register a default `HttpClient` whose base address is the WebAssembly host origin.
3. Register an OData client named `"TripPin"`.
4. Create a scoped `Container` from the generated proxy types.
5. Remove `OData-MaxVersion` and `OData-Version` headers inside `BuildingRequest`.
6. Register Fluent UI plus the OData DataGrid adapter.

The generated client metadata is embedded through `TripPinServiceCsdl.xml`, which lets the app inspect the service model at runtime without maintaining a hand-authored schema description.

## Navigation model

`Layout\NavMenu.razor` loads the service model from:

```csharp
Container.Format.LoadServiceModel()
```

It then walks `model.EntityContainer.Elements`, keeps only `EntitySet` entries, and emits links in the form:

```text
/?entity=<EntitySetName>
```

This means the navigation is a direct reflection of the service metadata, not a custom menu maintained by the application.

## Main page execution flow

`Pages\Home.razor` does the runtime composition work.

On parameter changes it:

1. Reads `?entity=` and `?virtualize=` from the query string.
2. Loads the EDM model from the OData container.
3. Resolves the requested `IEdmEntitySet`.
4. Maps the OData type name to the generated CLR type.
5. Uses reflection to call `Container.CreateQuery<TEntity>(entitySet.Name)`.
6. Generates grid columns from EDM metadata.
7. Builds a runtime equality comparer from entity keys or scalar properties.
8. Instantiates a generic `PaginatedDataGrid<TEntity>` dynamically.

The result is the OData equivalent of the AdventureWorks sample's EF Core pipeline.

## FluentDataGridEntityHelpers deep dive

`Controls\FluentDataGridEntityHelpers.cs` is the core adapter between TripPin's EDM metadata and Fluent UI's strongly typed `PropertyColumn<,>` component model. `Pages\Home.razor` already knows the selected `IEdmEntitySet`, but the grid API still expects real CLR lambdas such as `person => person.FirstName`. This helper is the runtime bridge that turns metadata into those lambdas.

`ColumnsRenderFragment(...)` returns a single `RenderFragment` that emits one grid column per eligible property. The method loops over `entitySet.EntityType.DeclaredProperties`, opens a render-tree region for each property, and delegates the actual column construction to `AddPropertyColumnComponent(...)`. That split is important because the outer method stays responsible for column ordering and stable render-tree grouping, while the inner method decides whether a property can become a column at all.

The property filter is intentionally simple and OData-specific:

- `property.Type.IsCollection()` is excluded because collection-valued navigation and primitive collections do not map to a single cell value.
- `property.Type.IsComplex()` is excluded because complex types would require nested column composition or custom templates instead of a plain `PropertyColumn`.

Everything else is treated as grid-friendly. That means the helper is deliberately driven by the EDM model, not by custom allowlists per entity set. If the TripPin metadata adds another scalar property later, the grid picks it up automatically.

For each included property, the helper performs four runtime steps:

1. Call `resolveType(property.DeclaringType.FullTypeName())` to map the EDM declaring type to the generated CLR proxy type from `Connected Services\TripPinService`.
2. Find the matching CLR `PropertyInfo` by name with `declaringType.GetProperty(property.Name)`.
3. Build an expression tree of shape `Func<TEntity, TProperty>` using `Expression.Parameter`, `Expression.Property`, and `Expression.Lambda`.
4. Instantiate `PropertyColumn<TEntity, TProperty>` with `MakeGenericType(...)`, assign the generated lambda to `Property`, set `Title` to the OData property name, and merge any caller-supplied attributes.

That last step is why `Home.razor` can pass:

```csharp
prop => new Dictionary<string, object> { { "Sortable", true } }
```

without knowing the entity's CLR shape up front. The helper preserves strong typing inside the generated column component even though the page itself is operating almost entirely through metadata and reflection.

Two smaller implementation details matter:

- the column title comes from `property.Name`, so the UI mirrors the service metadata exactly rather than using display attributes or hand-authored labels
- the helper iterates `DeclaredProperties`, not every inherited property on the CLR type, which keeps the grid aligned with the entity definition exposed by the selected OData set

The result is a narrow but reliable abstraction: TripPin can render any entity set backed by scalar EDM properties without creating per-entity Razor components or per-property column definitions.

## Pagination and virtualization

`Controls\PaginatedDataGrid.razor` mirrors the AdventureWorks pattern:

- `Virtualize = false` creates a `PaginationState` with `ItemsPerPage = 10`
- `Virtualize = true` removes pagination and lets the grid virtualize rows

The page exposes this through a `FluentSwitch`, so the behavior difference is visible without changing code.

## Generated OData client

The `Connected Services\TripPinService` folder contains the generated proxy types, the connected-service configuration, and the CSDL metadata. `ConnectedService.json` records the source endpoint used to generate those types:

```text
https://services.odata.org/TripPinRESTierService/(S(1hmjktdokfcn3zoxsca01sjn))/$metadata
```

This is valuable if you want to regenerate the proxy or compare the runtime EDM with the generated classes.

## Development endpoints

`Properties\launchSettings.json` configures:

- `https://localhost:52898`
- `http://localhost:52899`

## Build and run

From the repository root:

```powershell
dotnet build .\FluentUI.Samples.sln
dotnet run --project .\FluentUI\TripPin\FluentUI.TripPin.csproj
```

Because the sample talks to a public OData service, no local database or server setup is required.

## References

- [TripPin - OData V4 Sample Service](https://www.odata.org/blog/trippin-new-odata-v4-sample-service/)
- [OData Documentation](https://learn.microsoft.com/en-us/odata/)
- [OData Dev Blog](https://devblogs.microsoft.com/odata/)
