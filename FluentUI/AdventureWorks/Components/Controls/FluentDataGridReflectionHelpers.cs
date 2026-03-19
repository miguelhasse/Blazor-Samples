using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Provides helper methods for generating render fragments and property column components for all public properties of
/// a specified type, typically for use in Blazor data grids.
/// </summary>
/// <remarks>This class is intended to support dynamic column generation in Blazor components by reflecting over
/// the public properties of a data model type. The generated columns correspond to properties that are value types,
/// strings, or URIs. Additional attributes can be supplied for each column via a callback function. These helpers are
/// useful for building flexible, data-driven UI grids where the set of columns is determined at runtime.</remarks>
public static class FluentDataGridReflectionHelpers
{
    /// <summary>
    /// Creates a render fragment that generates property column components for each public property of the specified
    /// type.
    /// </summary>
    /// <remarks>This method is typically used in Blazor components to dynamically generate columns based on
    /// the properties of a data model. The order and inclusion of columns correspond to the properties returned by
    /// GetPropertyColumns. If additonalAttributesFunc is provided, its output is applied to each generated column
    /// component.</remarks>
    /// <param name="type">The type whose public properties will be rendered as columns.</param>
    /// <param name="additonalAttributesFunc">An optional function that provides additional attributes for each property column component. The function
    /// receives a PropertyInfo and returns a dictionary of attribute names and values. If null, no additional
    /// attributes are applied.</param>
    /// <returns>A RenderFragment that renders property column components for each public property of the specified type.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "ASP0006:Do not use non-literal sequence numbers", Justification = "Reviewed")]
    public static RenderFragment ColumnsRenderFragment(Type type, Func<PropertyInfo, IDictionary<string, object>>? additonalAttributesFunc = null)
    {
        return builder =>
        {
            int sequence = -1;
            foreach (var propertyInfo in GetPropertyColumns(type))
            {
                builder.OpenRegion(sequence++);
                builder.AddPropertyColumnComponent(propertyInfo, additonalAttributesFunc);
                builder.CloseRegion();
            }
        };
    }

    /// <summary>
    /// Generates a collection of render fragments for each property column of the specified type.
    /// </summary>
    /// <param name="type">The type whose property columns are used to generate render fragments.</param>
    /// <param name="additonalAttributesFunc">An optional function that provides additional attributes for each property column. If null, no additional
    /// attributes are applied.</param>
    /// <returns>An enumerable collection of render fragments, each representing a property column of the specified type.</returns>
    public static IEnumerable<RenderFragment> ColumnRenderFragments(Type type, Func<PropertyInfo, IDictionary<string, object>>? additonalAttributesFunc = null)
    {
        foreach (var propertyInfo in GetPropertyColumns(type))
        {
            yield return builder => builder.AddPropertyColumnComponent(propertyInfo, additonalAttributesFunc);
        }
    }

    private static void AddPropertyColumnComponent(this RenderTreeBuilder builder, PropertyInfo propertyInfo, Func<PropertyInfo, IDictionary<string, object>>? additonalAttributesFunc)
    {
        builder.OpenComponent(0, typeof(PropertyColumn<,>).MakeGenericType(propertyInfo.ReflectedType!, propertyInfo.PropertyType));
        builder.AddAttribute(1, "Property", BuildPropertyExpression(propertyInfo));
        builder.AddAttribute(2, "Title", propertyInfo.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? propertyInfo.Name);
        builder.AddAttribute(3, "Format", propertyInfo.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString);
        builder.AddMultipleAttributes(4, additonalAttributesFunc != null ? additonalAttributesFunc(propertyInfo) : null);
        builder.CloseComponent();
    }

    private static IEnumerable<PropertyInfo> GetPropertyColumns(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(p =>
        {
            var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            return type.IsValueType || type == typeof(string) || type == typeof(Uri);
        });
    }

    private static LambdaExpression BuildPropertyExpression(PropertyInfo propertyInfo)
    {
        var parameter = Expression.Parameter(propertyInfo.ReflectedType!, "c");
        return Expression.Lambda(
            typeof(Func<,>).MakeGenericType(propertyInfo.ReflectedType!, propertyInfo.PropertyType),
            Expression.Property(parameter, propertyInfo), parameter);
    }
}