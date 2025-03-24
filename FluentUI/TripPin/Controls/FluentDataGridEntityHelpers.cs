using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.OData.Edm;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Provides helper methods for generating columns in a Fluent UI data grid based on an entity set.
/// </summary>
public static class FluentDataGridEntityHelpers
{
    /// <summary>
    /// Generates a RenderFragment for the columns of a data grid based on the provided entity set.
    /// </summary>
    /// <param name="entitySet">The entity set to generate columns for.</param>
    /// <param name="resolveType">A function to resolve the type of a property.</param>
    /// <param name="additonalAttributesFunc">An optional function to provide additional attributes for each column.</param>
    /// <returns>A RenderFragment representing the columns of the data grid.</returns>
    public static RenderFragment ColumnsRenderFragment(IEdmEntitySet entitySet, Func<string, Type> resolveType, Func<IEdmProperty, IDictionary<string, object>>? additonalAttributesFunc = null)
    {
        return builder =>
        {
            var sequence = -1;
            foreach (var property in entitySet.EntityType.DeclaredProperties)
            {
                builder.OpenRegion(sequence++);
                builder.AddPropertyColumnComponent(property, resolveType, additonalAttributesFunc);
                builder.CloseRegion();
            }
        };
    }

    /// <summary>
    /// Adds a property column component to the RenderTreeBuilder.
    /// </summary>
    /// <param name="builder">The RenderTreeBuilder to add the component to.</param>
    /// <param name="property">The property to create a column for.</param>
    /// <param name="resolveType">A function to resolve the type of the property.</param>
    /// <param name="additonalAttributesFunc">An optional function to provide additional attributes for the column.</param>
    private static void AddPropertyColumnComponent(this RenderTreeBuilder builder, IEdmProperty property, Func<string, Type> resolveType, Func<IEdmProperty, IDictionary<string, object>>? additonalAttributesFunc)
    {
        if (property.Type.IsCollection() || property.Type.IsComplex())
        {
            return;
        }

        var declaringType = resolveType(property.DeclaringType.FullTypeName());
        var propertyInfo = declaringType.GetProperty(property.Name)!;

        var parameter = Expression.Parameter(declaringType, "c");
        var propertyExpression = Expression.Lambda(
            typeof(Func<,>).MakeGenericType(declaringType, propertyInfo.PropertyType),
            Expression.Property(parameter, propertyInfo), parameter);

        builder.OpenComponent(0, typeof(PropertyColumn<,>).MakeGenericType(declaringType, propertyInfo.PropertyType));
        builder.AddAttribute(1, "Property", propertyExpression);
        builder.AddAttribute(2, "Title", property.Name);
        builder.AddMultipleAttributes(3, additonalAttributesFunc != null ? additonalAttributesFunc(property) : null);
        builder.CloseComponent();
    }
}
