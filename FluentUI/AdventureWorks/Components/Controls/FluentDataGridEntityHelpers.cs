using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Provides helper methods for generating render fragments that create property column components for all properties of
/// a specified entity type in a fluent data grid.
/// </summary>
/// <remarks>This class is intended for use with Blazor components that render data grids based on entity types.
/// It simplifies the process of generating columns dynamically from entity metadata. Scalar properties are rendered
/// as <see cref="PropertyColumn{TGridItem, TProp}"/> components. Properties with complex database types
/// (such as 'xml', 'json', 'geography', 'geometry') are rendered as <see cref="TemplateColumn{TGridItem}"/>
/// components with a built-in summarising renderer. Properties that are keys, foreign keys, concurrency tokens,
/// arrays, or have opaque types ('uniqueidentifier', 'hierarchyid', 'rowversion') are excluded entirely.</remarks>
public static class FluentDataGridEntityHelpers
{
    private enum ColumnClassification { Property, Template, Exclude }

    /// <summary>
    /// Creates a render fragment that generates property column components for each property of the specified entity
    /// type.
    /// </summary>
    /// <param name="type">The entity type whose properties will be rendered as columns.</param>
    /// <param name="additonalAttributesFunc">An optional function that provides additional attributes for each column component.
    /// If null, no additional attributes are applied.</param>
    /// <param name="templateFactory">An optional function that supplies a custom cell renderer for properties classified as template columns
    /// (those with 'xml', 'json', 'geography', or 'geometry' column types). The function receives the property and
    /// should return a delegate that takes the row entity instance as <see cref="object"/> and returns a
    /// <see cref="RenderFragment"/> for that cell. Return <see langword="null"/> to use the built-in default renderer
    /// for that property.</param>
    /// <returns>A render fragment that renders column components for all non-excluded properties of the specified entity type.</returns>
    public static RenderFragment ColumnsRenderFragment(
        IEntityType type,
        Func<IProperty, IDictionary<string, object>>? additonalAttributesFunc = null,
        Func<IProperty, Func<object, RenderFragment>?>? templateFactory = null)
    {
        return builder =>
        {
            var sequence = -1;
            foreach (var property in type.GetProperties())
            {
                var classification = ClassifyProperty(property);
                if (classification == ColumnClassification.Exclude)
                    continue;

                builder.OpenRegion(sequence++);
                if (classification == ColumnClassification.Property)
                    builder.AddPropertyColumnComponent(property, additonalAttributesFunc);
                else
                    builder.AddTemplateColumnComponent(property, templateFactory, additonalAttributesFunc);
                builder.CloseRegion();
            }
        };
    }

    private static ColumnClassification ClassifyProperty(IProperty p) => p.GetColumnType() switch
    {
        "xml" or "json" or "geography" or "geometry" => ColumnClassification.Template,
        "uniqueidentifier" or "hierarchyid" or "rowversion" => ColumnClassification.Exclude,
        _ when p.IsKey() || p.IsForeignKey() || p.IsConcurrencyToken || p.ClrType.IsArray => ColumnClassification.Exclude,
        _ => ColumnClassification.Property
    };

    private static void AddPropertyColumnComponent(this RenderTreeBuilder builder, IProperty property, Func<IProperty, IDictionary<string, object>>? additonalAttributesFunc)
    {
        builder.OpenComponent(0, typeof(PropertyColumn<,>).MakeGenericType(property.DeclaringType.ClrType, property.ClrType));
        builder.AddAttribute(1, "Property", BuildPropertyExpression(property));
        builder.AddAttribute(2, "Title", property.GetColumnName());
        builder.AddMultipleAttributes(3, additonalAttributesFunc != null ? additonalAttributesFunc(property) : null);
        builder.CloseComponent();
    }

    private static void AddTemplateColumnComponent(
        this RenderTreeBuilder builder,
        IProperty property,
        Func<IProperty, Func<object, RenderFragment>?>? templateFactory,
        Func<IProperty, IDictionary<string, object>>? additonalAttributesFunc)
    {
        var factory = templateFactory?.Invoke(property) ?? GetDefaultTemplate(property);
        var typedChildContent = CreateTypedTemplate(property.DeclaringType.ClrType, factory);

        builder.OpenComponent(0, typeof(TemplateColumn<>).MakeGenericType(property.DeclaringType.ClrType));
        builder.AddAttribute(1, "Title", property.GetColumnName());
        builder.AddAttribute(2, "ChildContent", typedChildContent);
        builder.AddMultipleAttributes(3, additonalAttributesFunc != null ? additonalAttributesFunc(property) : null);
        builder.CloseComponent();
    }

    // Creates a properly-typed RenderFragment<TEntity> from an untyped Func<object, RenderFragment>
    // by invoking the generic WrapTemplate<TEntity> helper via reflection.
    private static object CreateTypedTemplate(Type entityType, Func<object, RenderFragment> factory)
    {
        return typeof(FluentDataGridEntityHelpers)
            .GetMethod(nameof(WrapTemplate), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityType)
            .Invoke(null, [factory])!;
    }

    private static RenderFragment<TEntity> WrapTemplate<TEntity>(Func<object, RenderFragment> factory)
        => entity => factory(entity!);

    private static Func<object, RenderFragment> GetDefaultTemplate(IProperty property)
    {
        var columnType = property.GetColumnType();
        var propertyInfo = property.PropertyInfo!;
        return entity =>
        {
            var value = propertyInfo.GetValue(entity);
            return builder =>
            {
                builder.OpenElement(0, "code");
                builder.AddContent(1, BuildCellText(columnType, value));
                builder.CloseElement();
            };
        };
    }

    private static string BuildCellText(string columnType, object? value) => columnType switch
    {
        "xml" or "json" when value is string s => $"{{ {columnType} \u00b7 {s.Length:N0} chars }}",
        "xml" or "json" => $"{{ {columnType} \u00b7 null }}",
        "geography" or "geometry" => FormatGeometry(value),
        _ => value?.ToString() ?? string.Empty
    };

    private static string FormatGeometry(object? value)
    {
        if (value is null) return "null";
        // NetTopologySuite.Geometries.Point exposes X (longitude) and Y (latitude)
        if (value is NetTopologySuite.Geometries.Point pt)
            return $"({pt.Y:F4}, {pt.X:F4})";
        // Geometry.ToString() returns WKT for all other NTS geometry subtypes
        return value.ToString() ?? string.Empty;
    }

    private static LambdaExpression BuildPropertyExpression(IProperty property)
    {
        var parameter = Expression.Parameter(property.DeclaringType.ClrType, "c");
        return Expression.Lambda(
            typeof(Func<,>).MakeGenericType(property.DeclaringType.ClrType, property.ClrType),
            Expression.Property(parameter, property.PropertyInfo!), parameter);
    }
}
