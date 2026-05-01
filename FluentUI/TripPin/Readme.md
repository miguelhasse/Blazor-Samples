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

## Column generation rules

`Controls\FluentDataGridEntityHelpers.cs` iterates over `entitySet.EntityType.DeclaredProperties` and skips:

- collection properties
- complex properties

For each remaining property it:

- resolves the declaring CLR type
- builds a `LambdaExpression` for `PropertyColumn<,>`
- assigns the OData property name as the column title
- applies any additional attributes such as `Sortable = true`

This keeps the sample focused on scalar properties that map cleanly to a tabular UI.

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

## Files worth reading first

- `Program.cs`
- `Layout\NavMenu.razor`
- `Pages\Home.razor`
- `Controls\PaginatedDataGrid.razor`
- `Controls\FluentDataGridEntityHelpers.cs`
- `Connected Services\TripPinService\ConnectedService.json`
