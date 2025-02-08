using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.FluentUI.AspNetCore.Components;

public static class FluentDataGridReflectionHelpers
{
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