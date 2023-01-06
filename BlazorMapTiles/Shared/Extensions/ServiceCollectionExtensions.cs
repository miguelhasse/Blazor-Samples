using System;
using AzureMapsConfiguration = BlazorMapTiles.Configuration.AzureMapsConfiguration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A set of extension methods for IServiceCollection which provide support for Dashboard infrastructure.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add browser information support.
        /// </summary>
        /// <param name="services">The IServiceCollection.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAzureMaps(this IServiceCollection services, Action<AzureMapsConfiguration> configure)
        {
            return AzureMapsControl.Components.Extensions.AddAzureMapsControl(services, amcc =>
            {
                var config = new AzureMapsConfiguration();
                configure(config);

                amcc.AadAppId = config.AppId;
                amcc.AadTenant = config.TenantId;
                amcc.ClientId = config.ClientId;
                amcc.SubscriptionKey = config.SubscriptionKey;
            });
        }
    }
}
