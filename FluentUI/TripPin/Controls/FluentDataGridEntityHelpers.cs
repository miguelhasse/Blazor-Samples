using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.OData.Edm;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Provides helper methods for generating column render fragments for entity sets in a FluentDataGrid component.
/// </summary>
/// <remarks>This static class is intended for use with Blazor components that display OData entities using a
/// FluentDataGrid. The methods assist in dynamically creating column definitions based on the metadata of the provided
/// entity set. All members are thread-safe and stateless.</remarks>
public static class FluentDataGridEntityHelpers
{
    /// <summary>
    /// Creates a render fragment that generates property column components for each declared property of the specified
    /// entity set's entity type.
    /// </summary>
    /// <remarks>The returned render fragment iterates over all declared properties of the entity type and
    /// creates a column component for each. This is typically used in dynamic table or grid rendering
    /// scenarios.</remarks>
    /// <param name="entitySet">The EDM entity set whose entity type's declared properties will be rendered as columns. Cannot be null.</param>
    /// <param name="resolveType">A function that resolves the .NET type for a given property name. Used to determine the type of each property
    /// column.</param>
    /// <param name="additonalAttributesFunc">An optional function that provides additional attributes for each property column component. If null, no extra
    /// attributes are added.</param>
    /// <returns>A render fragment that, when rendered, outputs property column components for all declared properties of the
    /// entity set's entity type.</returns>
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
