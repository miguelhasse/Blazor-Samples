using System.Linq.Expressions;
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
/// It simplifies the process of generating columns dynamically from entity metadata. The generated columns exclude
/// properties with certain database types (such as 'json', 'xml', 'geography', etc.) and properties that are keys,
/// foreign keys, concurrency tokens, or arrays.</remarks>
public static class FluentDataGridEntityHelpers
{
    /// <summary>
    /// Creates a render fragment that generates property column components for each property of the specified entity
    /// type.
    /// </summary>
    /// <param name="type">The entity type whose properties will be rendered as columns.</param>
    /// <param name="additonalAttributesFunc">An optional function that provides additional attributes for each property column component. If null, no
    /// additional attributes are applied.</param>
    /// <returns>A render fragment that renders property column components for all properties of the specified entity type.</returns>
    public static RenderFragment ColumnsRenderFragment(IEntityType type, Func<IProperty, IDictionary<string, object>>? additonalAttributesFunc = null)
    {
        return builder =>
        {
            var sequence = -1;
            foreach (var property in GetPropertyColumns(type))
            {
                builder.OpenRegion(sequence++);
                builder.AddPropertyColumnComponent(property, additonalAttributesFunc);
                builder.CloseRegion();
            }
        };
    }

    private static void AddPropertyColumnComponent(this RenderTreeBuilder builder, IProperty property, Func<IProperty, IDictionary<string, object>>? additonalAttributesFunc)
    {
        builder.OpenComponent(0, typeof(PropertyColumn<,>).MakeGenericType(property.DeclaringType.ClrType, property.ClrType));
        builder.AddAttribute(1, "Property", BuildPropertyExpression(property));
        builder.AddAttribute(2, "Title", property.GetColumnName());
        builder.AddMultipleAttributes(3, additonalAttributesFunc != null ? additonalAttributesFunc(property) : null);
        builder.CloseComponent();
    }

    private static IEnumerable<IProperty> GetPropertyColumns(IEntityType type)
    {
        return type.GetProperties().Where(p => p.GetColumnType() switch
        {
            "json" => false,
            "xml" => false,
            "geography" => false,
            "geometry" => false,
            "uniqueidentifier" => false,
            "hierarchyid" => false,
            "rowversion" => false,
            _ => !p.IsKey() && !p.IsForeignKey() && !p.IsConcurrencyToken && !p.ClrType.IsArray
        });
    }

    private static LambdaExpression BuildPropertyExpression(IProperty property)
    {
        var parameter = Expression.Parameter(property.DeclaringType.ClrType, "c");
        return Expression.Lambda(
            typeof(Func<,>).MakeGenericType(property.DeclaringType.ClrType, property.ClrType),
            Expression.Property(parameter, property.PropertyInfo!), parameter);
    }
}
