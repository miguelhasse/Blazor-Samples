using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.OData.Edm;

namespace Microsoft.FluentUI.AspNetCore.Components;

public static class FluentDataGridEntityHelpers
{
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
