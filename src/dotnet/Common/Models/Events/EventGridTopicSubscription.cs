namespace FoundationaLLM.Common.Models.Events
{
    /// <summary>
    /// The identifier of an Azure Event Grid topic subscription.
    /// </summary>
    /// <param name="Topic">The name of the Azure Event Grid namespace topic.</param>
    /// <param name="Subscription">The name of the Azure Event Grid namespace topic subscription.</param>
    public record EventGridTopicSubscription(
            string Topic,
            string Subscription)
    {
    }
}
