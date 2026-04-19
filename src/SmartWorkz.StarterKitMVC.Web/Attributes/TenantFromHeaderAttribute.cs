using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SmartWorkz.StarterKitMVC.Web.Attributes;

/// <summary>
/// Model binder attribute for extracting tenant ID from X-Tenant-Id header.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class TenantFromHeaderAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BindingSource.Custom;
}

/// <summary>
/// Model binder for TenantFromHeaderAttribute.
/// </summary>
public class TenantFromHeaderModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var httpContext = bindingContext.HttpContext;
        var tenantId = httpContext.Request.Headers["X-Tenant-Id"].ToString();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            tenantId = httpContext.Items["TenantId"]?.ToString();
        }

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            bindingContext.Result = ModelBindingResult.Success(tenantId);
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Provider for TenantFromHeaderModelBinder.
/// </summary>
public class TenantFromHeaderModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var parameterType = context.Metadata.ModelType;

        if (parameterType != typeof(string))
        {
            return null;
        }

        if (!context.Metadata.ParameterName.Contains("tenant", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new TenantFromHeaderModelBinder();
    }
}
