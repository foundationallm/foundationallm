using Azure.Messaging;

namespace FoundationaLLM.Common.Models.Events
{
    /// <summary>
    /// Event arguments required for event set event delegates.
    /// </summary>
    public class EventSetEventArgs : EventArgs
    {
        /// <summary>
        /// The namespace associated with the event set.
        /// </summary>
        public required string Namespace { get; set; }

        /// <summary>
        /// The list of subjects associated with the event.
        /// </summary>
        public required IList<CloudEvent> Events { get; set; }
    }

    /// <summary>
    /// Multicast delegate used by the Azure Event Grid event service to provide support 
    /// for subscribing to event namespaces.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The <see cref="EventSetEventArgs"/> that contains the details about the events being raised.</param>
    public delegate void EventSetEventDelegate(object sender, EventSetEventArgs e);
}
