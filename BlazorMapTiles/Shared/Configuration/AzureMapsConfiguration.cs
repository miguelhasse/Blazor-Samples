namespace BlazorMapTiles.Configuration
{
    public sealed class AzureMapsConfiguration
    {
        /// <summary>
        /// The Azure AD registered app ID. This is the app ID of an app registered in your Azure AD tenant.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// The AAD tenant that owns the registered app specified by `AppId`.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The Azure Maps client ID, This is an unique identifier used to identify the maps account.
        /// Must be specified for AAD and anonymous authentication types.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Subscription key from your Azure Maps account.
        /// Must be specified for subscription key authentication type.
        /// </summary>
        public string SubscriptionKey { get; set; }
    }
}
