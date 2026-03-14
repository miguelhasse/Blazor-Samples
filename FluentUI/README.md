# FluentUI samples

This folder contains two sample apps that demonstrate `Microsoft.FluentUI.AspNetCore.Components` DataGrid integration against different back ends.

## Included projects

### FluentUI.AdventureWorks

An interactive server-rendered sample that uses Entity Framework Core with SQL Server and the Fluent UI DataGrid Entity Framework adapter. The app inspects the model at runtime and renders a grid for the selected AdventureWorks entity.

Highlights:

- Uses a pooled `AdventureWorksContext`
- Supports entity selection through query string parameters
- Enables DataGrid virtualization and server-side data access

### FluentUI.TripPin

A Blazor WebAssembly sample that connects to the public TripPin OData service and renders entity sets through the Fluent UI DataGrid OData adapter.

Highlights:

- Uses `Microsoft.OData.Extensions.Client`
- Builds a typed client container for the TripPin service
- Demonstrates DataGrid binding against OData queries in the browser

## Solution and target frameworks

- Solution: `..\FluentUI.Samples.sln`
- Both sample projects target `net9.0` and `net10.0`

## Prerequisites

- .NET 9 SDK or .NET 10 SDK
- For `FluentUI.AdventureWorks`, access to a SQL Server instance with the AdventureWorks database restored

## Configuration

### AdventureWorks

Update `AdventureWorks\appsettings.json` so `ConnectionStrings:AdventureWorks` points at your SQL Server instance:

```json
"ConnectionStrings": {
  "AdventureWorks": "Data Source=localhost;Initial Catalog=AdventureWorks;Integrated Security=True;Encrypt=False;"
}
```

If you need to regenerate the EF Core model, the sample includes `AdventureWorks\scaffold.cmd`.

### TripPin

No local configuration is required for a basic run. The app targets the public TripPin OData sample service defined in `TripPin\Program.cs`.

## Running the samples

From the repository root:

```powershell
dotnet run --project .\FluentUI\AdventureWorks\FluentUI.AdventureWorks.csproj
dotnet run --project .\FluentUI\TripPin\FluentUI.TripPin.csproj
```

Or open `FluentUI.Samples.sln` in Visual Studio and start the sample you want to inspect.

## Additional references

- [`AdventureWorks\Readme.md`](AdventureWorks/Readme.md)
- [`TripPin\Readme.md`](TripPin/Readme.md)
